''' Provides the execution engine. '''
import json
import math
import random
import time
import pdb

import sensors
from movements import ArmMovement, BodyMovement, Dances, HeadMovement, Movements
from noRobot import RobotMock
import logger

try:
    from robot import Robot
except ImportError:
    Robot = RobotMock


class EngineSettings(object):
    ''' The configuration options for the engine. '''

    def __init__(self, opts):
        ''' Initialises the options. '''
        try:
            self.debug = opts['debug']
        except KeyError:
            self.debug = False

        try:
            self.delay = opts['delay']
        except KeyError:
            self.delay = 0


class EngineFunction(object):
    ''' Defines a function that can be executed on the engine. '''

    def __init__(self, func, top_level=False):
        self._func = func
        self.top_level = top_level

    def execute(self, state):
        ''' Executes the function. '''
        return self._func(state)


class ExecutionState(object):
    ''' Defines the current execution state. '''

    def __init__(self, ast, parent, function):
        self.function = function
        self.ast = ast
        self.parent = parent
        self.completed = False

    def complete(self):
        ''' Marks the state as completed. '''
        self.completed = True


class Engine(object):
    ''' The main execution engine for the robot. '''

    _is_walking = False

    BACK = 0
    LEFT = 1
    RIGHT = 2
    FRONT = LEFT + RIGHT

    def __init__(self, comms, use_robot=True, ip='127.0.0.1'):
        ''' Initialises the engine. '''
        self._comms = comms
        self._opts = EngineSettings({})
        self._reset(None)
        self.is_cancelled = False
        self._robot = None
        self._use_robot = use_robot
        self._variables = {}
        self._last_word = ''
        self._ip = ip

        self._leftFoot = None
        self._rightFoot = None
        self._leftSonar = None
        self._rightSonar = None

        with self._initialise_robot(ip) as robot:
            logger.log('[Engine] Robot is ready')
            robot.say('I am ready now')

    def configure(self, opts):
        ''' Configures the engine. '''
        self._opts = EngineSettings(opts)
        self.is_cancelled = False

    def cancel(self):
        ''' Cancels the current run. '''
        self.is_cancelled = True

    def run(self, ast):
        ''' Executes an AST. '''
        self._execute(ast, None, True)

    def trigger(self, block_name, value=None):
        ''' Triggers a block in the engine. '''
        if block_name == 'word':
            self._last_word = value
        self._generate_execute_block(block_name)(None)

    def _execute(self, ast, state, top_level=False):
        ''' Executes a block of AST. '''
        last_result = None
        for block in ast:
            if self.is_cancelled:
                break
            if block['type'] == 'Function':
                last_result = self._execute_function(block, state, top_level)
            elif block['type'] == 'Compound':
                compound_state = ExecutionState(
                    block, state, block['token']['value'])
                for child in block['children']:
                    last_result = self._execute_function(
                        child, compound_state, False)
                    if compound_state.completed:
                        break
            else:
                self._error('Unknown node type: %s' % (block['type'], ))
        return last_result

    def _execute_function(self, block, state, top_level):
        ''' Executes a function. '''
        func_name = block['token']['value']
        last_result = None
        try:
            func = self._functions[func_name]
        except KeyError:
            self._error('Unknown function: ' + func_name)
            return last_result

        if func.top_level and not top_level:
            self._error('Function ' + func_name +
                        ' cannot be executed here')
        else:
            logger.log('[Engine] Executing function "%s"', func_name)
            self._debug(block, 'start')
            func_state = ExecutionState(block, state, func_name)
            last_result = func.execute(func_state)
            if not state is None and func_state.completed:
                state.complete()
            if not top_level:
                self._do_delay()
            self._debug(block, 'end')

        return last_result

    def _do_delay(self):
        seconds = int(self._opts.delay)
        if seconds > 0:
            logger.log('[Engine] Delaying for ' + str(seconds) + 's')
            for _ in range(0, seconds):
                if self.is_cancelled:
                    break
                time.sleep(1)

    def _error(self, message):
        logger.log('[Engine] Sending error message: "' + message + '"')
        data = {
            'message': message
        }
        self._comms.send(503, data)

    def _change_state(self, name, value):
        logger.log('[Engine] Sending state change for ' + name + ' of ' + str(value))
        data = {
            'name': name,
            'value': value
        }
        self._comms.send(501, data)

    def _debug(self, block, status):
        try:
            debug_id = block['sourceId']
            logger.log('[Engine] Sending debug info for block %s [%s]', debug_id, status)
            data = {
                'sourceID': debug_id,
                'status': status,
                'function': block['token']['value']
            }
            self._comms.send(502, data)
        except KeyError:
            logger.log('[Engine] Unable to find sourceId, skipping send debug')

    def _evaluate(self, node, state):
        ''' Evaluates a node. '''
        node_type = node['token']['type']
        node_value = node['token']['value']
        if node_type == 'Text':
            return str(node_value)
        elif node_type == 'Constant':
            return str(node_value)
        elif node_type == 'Number':
            return float(node_value)
        elif node_type == 'Boolean':
            return node_value == 'TRUE'
        elif node_type == 'Identifier':
            return self._execute([node], state)
        elif node_type == 'Variable':
            return self._get_variable(node_value)
        elif node_type == 'Colour':
            return '#' + str(node_value)

        logger.log('[Engine] Unknown expression type: ' + node_type)

    def _get_variable(self, name):
        logger.log('[Engine] Retrieving variable ' + name)
        try:
            return self._variables[name]
        except KeyError:
            self._error('Unknown variable ' + name)

    def _reset(self, state):
        ''' Resets the execution engine. '''
        logger.log('[Engine] Resetting engine')
        self._variables = {}
        self._blocks = {}
        self._last_function = None
        self._functions = {
            # Top level functions
            'reset': EngineFunction(self._reset, True),
            'start': EngineFunction(self._generate_register_block('start'), True),
            'frontButton': EngineFunction(self._generate_register_block('front'), True),
            'middleButton': EngineFunction(self._generate_register_block('middle'), True),
            'rearButton': EngineFunction(self._generate_register_block('rear'), True),
            'chestButton': EngineFunction(self._generate_register_block('chest'), True),
            'wordRecognised': EngineFunction(self._generate_register_block('word'), True),
            'go': EngineFunction(self._generate_execute_block('start'), True),

            # Robot functions
            'wave': EngineFunction(self._wave),
            'dance': EngineFunction(self._dance),
            'look': EngineFunction(self._look),
            'point': EngineFunction(self._point),
            'say': EngineFunction(self._say),
            'rest': EngineFunction(self._rest),
            'wait': EngineFunction(self._wait),
            'walk': EngineFunction(self._walk),
            'stop': EngineFunction(self._stop),
            'turn': EngineFunction(self._turn),
            'randomColour': EngineFunction(self._random_colour),
            'position': EngineFunction(self._position),
            'wipe_forehead': EngineFunction(self._wipe_forehead),
            'changeLEDColour': EngineFunction(self._change_LED),
            'changeHand': EngineFunction(self._change_hand),
            'readSensor': EngineFunction(self._read_sensor),
            'lastRecognisedWord': EngineFunction(self._last_recognised_word),

            # Programming functions
            'loop': EngineFunction(self._loop),
            'while': EngineFunction(self._while),
            'variable': EngineFunction(self._define_variable),
            'function': EngineFunction(self._define_function),
            'addTo':  EngineFunction(self._add_to_variable),
            'if': EngineFunction(self._check_if_condition),
            'elseif': EngineFunction(self._check_if_condition),
            'else': EngineFunction(self._check_else),
            'not': EngineFunction(self._invert),
            'equal': EngineFunction(self._check_if_equal),
            'lessThan': EngineFunction(self._check_less_than),
            'greaterThan': EngineFunction(self._check_greater_than),
            'notEqual': EngineFunction(self._check_not_equal),
            'lessThanEqual': EngineFunction(self._check_less_than_equal),
            'greaterThanEqual': EngineFunction(self._check_greater_than_equal),
            'round': EngineFunction(self._round),

            # Behaviour functions
             'arabasque':EngineFunction(self._generate_behaviour('Arabasque', Movements.ARABESQUE)),
            'plie': EngineFunction(self._generate_behaviour('Plie', Movements.PLIE)),
            'single': EngineFunction(self._generate_behaviour('Single', Movements.SINGLE)),
            'arabasqueRight': EngineFunction(self._generate_behaviour('ArabasqueRight', Movements.ARABESQUERIGHT)),
            'fly': EngineFunction(self._generate_behaviour('Fly', Movements.FLY)),
            'ballet3': EngineFunction(self._generate_behaviour('Ballet3', Movements.BALLET3)),
            'ballet2': EngineFunction(self._generate_behaviour('Ballet2', Movements.BALLET2)),
            'ballet1': EngineFunction(self._generate_behaviour('Ballet1', Movements.BALLET1)),
            'enavont': EngineFunction(self._generate_behaviour('enavont', Movements.ENAVONT)),
            'up': EngineFunction(self._generate_behaviour('up', Movements.UP)),
            'rest': EngineFunction(self._generate_behaviour('rest', Movements.REST)),
            'leftArmOut': EngineFunction(self._generate_behaviour('leftArmOut', Movements.LEFT_ARM_OUT)),
            'rightArmOut': EngineFunction(self._generate_behaviour('rightArmOut', Movements.RIGHT_ARM_OUT)),
            'sprinkler': EngineFunction(self._generate_behaviour('sprinkler', Movements.SPRINKLER)),
            'robot_armsOutDown': EngineFunction(self._generate_behaviour('robot_armsOutDown', Movements.ROBOT_ARMS_OUT_DOWN)),
            'robot_rightUpLeftDown': EngineFunction(self._generate_behaviour('robot_rightUpLeftDown', Movements.ROBOT_RIGHT_UP_LEFT_DOWN)),
            'robot_bothUp': EngineFunction(self._generate_behaviour('robot_bothUp', Movements.ROBOT_BOTH_UP)),
            'robot_rightDownLeftUp': EngineFunction(self._generate_behaviour('robot_rightDownLeftUp', Movements.ROBOT_RIGHT_DOWN_LEFT_UP)),
            'robot_completeRobot': EngineFunction(self._generate_behaviour('robot_completeRobot', Movements.ROBOT_COMPLETE_ROBOT)),
            'macarena_armsOut': EngineFunction(self._generate_behaviour('macarena_armsOutDown', Movements.MACARENA_ARMS_OUT)),
            'macarena_palmsUp': EngineFunction(self._generate_behaviour('macarena_palmsUp', Movements.MACARENA_PALMS_UP)),
            'macarena_crossChest': EngineFunction(self._generate_behaviour('macarena_crossChest', Movements.MACARENA_CROSS_CHEST)),
            'macarena_handsBehindHead': EngineFunction(self._generate_behaviour('macarena_handsBehindHead', Movements.MACARENA_HANDS_BEHIND_HEAD)),
            'macarena_crossWaist': EngineFunction(self._generate_behaviour('macarena_crossWaist', Movements.MACARENA_CROSS_WAIST)),
            'macarena_handsOnHips': EngineFunction(self._generate_behaviour('macarena_handsOnHips', Movements.MACARENA_HANDS_ON_HIPS)),
            'macarena_circle': EngineFunction(self._generate_behaviour('macarena_circle', Movements.MACARENA_CIRCLE)),
        }

    def _generate_register_block(self, block_name):
        ''' Generates a closure to register block. '''
        def _register_block(state):
            ''' Registers the on start block. '''
            logger.log('[Engine] Registering ' + block_name + ' block')
            self._blocks[block_name] = state.ast
        return _register_block

    def _generate_behaviour(self, behaviour_name, behaviour_id):
        ''' Generates a closure for behaviour only functions. '''
        def _execute_behaviour(state):
            ''' Executes a behaviour and waits for it to complete. '''
            logger.log('[Engine] Performing behaviour "' + behaviour_name + '"')
            self._robot.startBehaviour(behaviour_id)
            self._robot.wait()
        return _execute_behaviour

    def _generate_execute_block(self, block_name):
        ''' Generates a closure to execute the specified block. '''
        def _execute_block(state):
            ''' Executes the block if it exists. '''
            try:
                block = self._blocks[block_name]
            except KeyError:
                logger.log('[Engine] ' + block_name + \
                    ' block not registered, skipping')
                return

            logger.log('[Engine] Executing ' + block_name + ' block')
            with self._initialise_robot(self._ip) as robot:
                self._robot = robot
                self._execute(block['children'], None)
                robot.rest()
                self._robot = None
            logger.log('[Engine] ' + block_name + ' block completed')

        return _execute_block

    def _initialise_robot(self, ip):
        logger.log('[Engine] Initialising robot')
        if self._use_robot:
            r = Robot(ip)
            return r
        return RobotMock()

    def _wave(self, state):
        ''' Make the robot wave. '''
        logger.log('[Engine] Waving')
        movement = BodyMovement().wave()
        self._perform_movement(state, movement, 'wave', require_standing = False)

    def _look(self, state):
        ''' Make the robot look in a direction. '''
        direction = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Looking ' + direction)
        movement = HeadMovement()
        if direction == 'left':
            movement = movement.lookLeft()
        elif direction == 'right':
            movement = movement.lookRight()
        elif direction == 'ahead':
            movement = movement.lookAhead()
        self._perform_movement(state, movement, 'look', 1, require_standing = False)

    def _point(self, state):
        ''' Make the robot point in a direction. '''
        arm = self._evaluate(state.ast['arguments'][0], state)
        direction = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Pointing ' + arm + ' arm ' + direction)
        movement = ArmMovement(ArmMovement.LEFT if arm ==
                               'left' else ArmMovement.RIGHT)
        if direction == 'out':
            movement = movement.pointOut()
        elif direction == 'down':
            movement = movement.pointDown()
        elif direction == 'up':
            movement = movement.pointUp()
        elif direction == 'ahead':
            movement = movement.pointAhead()
        self._perform_movement(state, movement, 'look', 2, require_standing = False)

    def _perform_movement(self, state, movement, name, speech=0, require_standing=True):
        if require_standing:
            posture = self._robot.getPosture()
            if posture != 'Standing' and posture != 'Sitting':
                logger.log('[Engine] Unable to ' + name + ' in the current position')
                self._robot.say('I cannot ' + name + ' in this position')
                return

        try:
            speech = self._evaluate(state.ast['arguments'][speech], state)
            self._robot.say(speech)
        except KeyError:
            logger.log('[Engine] No text to speak, skipping')
            pass
        except IndexError:
            logger.log('[Engine] No text to speak, skipping')
            pass
        except TypeError:
            logger.log('[Engine] No text to speak, skipping')
            pass

        self._robot.performMovements(movement).wait()

    def _dance(self, state):
        ''' Make the robot dance. '''
        posture = self._robot.getPosture()
        if posture != 'Standing':
            self._robot.say('I cannot dance in this posture')
            return

        dance = self._evaluate(state.ast['arguments'][0], state)
        music = self._evaluate(state.ast['arguments'][1], state)
        dances = {
            'macaranna': Dances.MACARENA,
            'gangnam': Dances.GANGNAM,
            'taichi': Dances.TAICHI
        }
        if not music:
            self._robot.muteAudioVolume()
        try:
            logger.log('[Engine] Performing dance ' + dance)
            self._robot.startBehaviour(dances[dance])
            self._robot.wait()
        except KeyError:
            logger.log('[Engine] Unknown dance ' + dance)

        if not music:
            self._robot.restoreAudioVolume()

    def _rest(self, state):
        ''' Make the robot rest. '''
        logger.log('[Engine] Resting')
        self._robot.rest()

    def _wait(self, state):
        ''' Make the robot wait. '''
        seconds = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Waiting for ' + str(seconds) + 's')
        for _ in range(0, int(seconds)):
            if self.is_cancelled:
                break
            time.sleep(1)

    def _walk(self, state):
        ''' Make the robot walk. '''
        posture = self._robot.getPosture()
        if posture != 'Standing':
            self._robot.say('I cannot walk in this posture')
            return

        self._robot.setSonars(True)
        xDist = self._evaluate(state.ast['arguments'][0], state)
        yDist = self._evaluate(state.ast['arguments'][1], state)
        self._leftFoot = self._robot.getSensor(sensors.Sensor.FOOT_LEFT)
        self._rightFoot = self._robot.getSensor(sensors.Sensor.FOOT_RIGHT)
        self._leftSonar = self._robot.getSensor(sensors.Sensor.SONAR_LEFT)
        self._rightSonar = self._robot.getSensor(sensors.Sensor.SONAR_RIGHT)

        logger.log('[Engine] Walking forwards ' + \
            str(xDist) + 's, sideways ' + str(yDist) + 's')
        x_time = abs(int(xDist))
        y_time = abs(int(yDist))
        direction = Engine.BACK
        if xDist == 0:
            x_dir = 0
        elif xDist > 0:
            x_dir = 1
            direction = direction + Engine.FRONT
        else:
            x_dir = -1
        if yDist == 0:
            y_dir = 0
        elif yDist > 0:
            y_dir = -1
            direction = Engine.RIGHT
        else:
            y_dir = 1
            direction = Engine.LEFT
        time_1 = min(x_time, y_time)
        time_2 = max(x_time, y_time) - time_1
        self._is_walking = True
        self._robot.walkStart(x_dir, y_dir, 0)
        for _ in range(0, time_1):
            if self.is_cancelled or self._is_blocked(direction):
                break
            time.sleep(1)
        if x_time > y_time:
            y_dir = 0
        if x_time < y_time:
            x_dir = 0
        self._robot.walkStart(x_dir, y_dir, 0)
        walk_cancelled = False
        for _ in range(0, time_2):
            if self.is_cancelled or self._is_blocked(direction):
                walk_cancelled = True
                break
            time.sleep(1)
        if not walk_cancelled:
            logger.log('[Engine] Stopping walk due to time expired')
        self._robot.walkStop()
        self._is_walking = False
        self._robot.setSonars(False)

    def _is_blocked(self, direction):
        ''' Checks if there are any obstacles. '''
        left_foot = self._leftFoot.read()
        right_foot = self._rightFoot.read()
        logger.log('[Engine] Foot buttons (%s,%s)', left_foot, right_foot)
        if (left_foot or right_foot):
            logger.log('[Engine] Stopping walk due to foot buttons')
            return True

        if direction & Engine.LEFT:
            dist = self._leftSonar.read()
            logger.log('[Engine] Left distance is %f', dist)
            if dist < 0.25:
                logger.log('[Engine] Stopping walk due to left sonar')
                return True

        if direction & Engine.RIGHT:
            dist = self._rightSonar.read()
            logger.log('[Engine] Right distance is %f', dist)
            if dist < 0.25:
                logger.log('[Engine] Stopping walk due to right sonar')
                return True

        return False

    def _stop(self, state):
        ''' Make the robot stop. '''
        self._robot.walkStop()
        self._is_walking = False
        self._robot.setSonars(False)

    def _turn(self, state):
        ''' Make the robot turn. '''
        posture = self._robot.getPosture()
        if posture != 'Standing':
            self._robot.say('I cannot turn in this posture')
            return

        degs = float(self._evaluate(state.ast['arguments'][0], state))
        if degs > 360:
            degs = 360
        elif degs < -360:
            degs = -360
        logger.log('[Engine] Turning ' + \
            str(degs) + ' degrees [' + str(degs) + ' degrees]')
        self._robot.walkTo(0, 0, degs).wait()

    def _wipe_forehead(self, state):
        ''' Make the robot wipe forehead. '''
        logger.log('[Engine] Wiping forehead')
        movement = BodyMovement().wipeForehead()
        self._perform_movement(state, movement, 'wipe forehead', require_standing = False)

    def _say(self, state):
        ''' Make the robot speak. '''
        value = self._evaluate(state.ast['arguments'][0], state)
        text_to_say = str(value)
        try:
            number_value = float(value)
            if number_value.is_integer():
                text_to_say = '{:.0f}'.format(number_value)
        except:
            pass
        logger.log('[Engine] Saying "' + text_to_say + '"')
        self._robot.say(text_to_say).wait()

    def _position(self, state):
        ''' Make the robot move to a position. '''
        value = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Moving to position ' + value)
        try:
            speech = self._evaluate(state.ast['arguments'][1], state)
            self._robot.say(speech)
        except (KeyError, IndexError):
            pass
        self._robot.goToPosture(value).wait()

    def _change_LED(self, state):
        ''' Change an LED. '''
        item = self._evaluate(state.ast['arguments'][0], state)
        items = {
            'CHEST': Robot.CHEST,
            'BOTH_EYES': [Robot.RIGHT_EYE, Robot.LEFT_EYE],
            'RIGHT_EYE': Robot.RIGHT_EYE,
            'LEFT_EYE': Robot.LEFT_EYE
        }
        try:
            led = items[item]
        except:
            self._error('Unknown LED ' + item)
            return

        value = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Changing LED ' + str(item) + ' to ' + value)
        self._robot.setLEDColour(led, value)

    def _change_hand(self, state):
        ''' Make the robot open or close one or two hands. '''
        hand = [Robot.LEFT_HAND]
        text = ' left hand'
        handArg = self._evaluate(state.ast['arguments'][1], state)
        actionArg = self._evaluate(state.ast['arguments'][0], state)
        if handArg == 'right':
            hand = [Robot.RIGHT_HAND]
            text = ' right hand'
        elif handArg == 'both':
            hand = [Robot.RIGHT_HAND, Robot.LEFT_HAND]
            text = ' both hands'

        logger.log('[Engine] Changing ' + text)
        self._robot.moveHands(hand, actionArg == 'open').wait()

    def _read_sensor(self, state):
        ''' Reads a robot sensor. '''
        sensor = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Reading ' + sensor + ' sensor')
        try:
            sensor = self._robot.getSensor(sensor)
            value = sensor.read()
        except KeyError:
            self._error('Unknown sensor ' + sensor)
            return

        return value

    def _last_recognised_word(self, state):
        ''' Retrieves the last recognised word. '''
        logger.log('[Engine] Retrieving last recognised word (' + \
            self._last_word + ')')
        return self._last_word

    def _loop(self, state):
        ''' Define or update a variable. '''
        iterations = int(self._evaluate(state.ast['arguments'][0], state))
        logger.log('[Engine] Starting loop with ' + str(iterations) + ' iterations')
        for loop in range(iterations):
            self._change_state('loop', loop)
            logger.log('[Engine] Executing iteration ' + str(loop))
            self._execute(state.ast['children'], state)
            if self.is_cancelled:
                break
        logger.log('[Engine] Loop completed')

    def _define_variable(self, state):
        ''' Define or update a variable. '''
        name = state.ast['arguments'][0]['token']['value']
        value = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Setting variable ' + name + ' to ' + str(value))
        self._variables[name] = value
        self._change_state(name, value)

    def _define_function(self, state):
        ''' Define a new function. This function will not allow replacing existing functions (as this could be dangerous!) '''
        name = state.ast['arguments'][0]['token']['value']

        try:
            func = self._functions[name]
            if func is None:
                # This shouldn't be possible, but check just in case
                raise KeyError()

            self._error('Function ' + name + ' already exists - cannot add')
        except KeyError:
            # Add a wrapper around the AST and add it to the functions table
            logger.log('[Engine] Defining new function ' + name)
            try:
                children = state.ast['children']
            except KeyError:
                # This doesn't make sense, but allow for functions with no children (an empty function)
                children = []
            self._functions[name] = EngineFunction(self._execute_custom_function(name, children))

    def _execute_custom_function(self, name, ast):
        ''' Generates a closure to execute a custom function. '''
        def _execute_function(state):
            ''' Executes each AST block in the function definition. '''
            logger.log('[Engine] Executing custom function "' + name + '"')
            self._execute(ast, state)
            logger.log('[Engine] Custom function "' + name + '" completed')

        return _execute_function

    def _add_to_variable(self, state):
        ''' Increases a variable. '''
        name = state.ast['arguments'][0]['token']['value']
        value = self._evaluate(state.ast['arguments'][1], state)
        try:
            current = self._variables[name]
            new_value = (current + value)
            logger.log('[Engine] Increasing variable ' + name + ' by ' + \
                str(value) + ' from ' + str(current) + ' to ' + str(new_value))
            self._variables[name] = new_value
            self._change_state(name, new_value)
        except KeyError:
            self._error('Unknown variable ' + name)

    def _invert(self, state):
        ''' Inverts a value. '''
        value = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Inverting: ' + str(value))
        return not value

    def _round(self, state):
        ''' Rounds a value. '''
        value = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Rounding: ' + str(value))
        return round(value, 0)

    def _check_if_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Checking for equality: ' + \
            str(value1) + ' and ' + str(value2))
        return value1 == value2

    def _check_less_than(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Checking for equality: ' + \
            str(value1) + ' and ' + str(value2))
        return value1 < value2

    def _check_greater_than(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Checking for equality: ' + \
            str(value1) + ' and ' + str(value2))
        return value1 > value2

    def _check_not_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Checking for equality: ' + \
            str(value1) + ' and ' + str(value2))
        return value1 != value2

    def _check_less_than_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Checking for equality: ' + \
            str(value1) + ' and ' + str(value2))
        return value1 <= value2

    def _check_greater_than_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        logger.log('[Engine] Checking for equality: ' + \
            str(value1) + ' and ' + str(value2))
        return value1 >= value2

    def _check_if_condition(self, state):
        ''' Check a condition block. '''
        result = self._evaluate(state.ast['arguments'][0], state)
        if result is True:
            state.complete()
            logger.log('[Engine] Executing if block')
            self._execute(state.ast['children'], state)

    def _while(self, state):
        ''' Check a condition block. '''
        logger.log('[Engine] Executing while block')
        result = self._evaluate(state.ast['arguments'][0], state)
        while result is True:
            logger.log('[Engine] Starting loop')
            state.complete()
            logger.log('[Engine] Executing if block')
            self._execute(state.ast['children'], state)
            result = self._evaluate(state.ast['arguments'][0], state)
            logger.log('[Engine] Finished loop')

    def _check_else(self, state):
        ''' Check a else block. '''
        logger.log('[Engine] Executing else block')
        self._execute(state.ast['children'], state)

    def _random_colour(self, state):
        ''' Returns a random colour. '''
        colours = [
            '#000000',
            '#ff0000',
            '#00ff00',
            '#0000ff',
            '#ff00ff',
            '#ffff00',
            '#00ffff',
            '#ffffff',
        ]
        index = random.randint(0, 7)
        return colours[index]
