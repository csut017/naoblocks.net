""" mBot2 test client

This client simulates running an mBot2 client.

Unfortunately, due to how mBlock deploys, this is a copy-paste job.
"""

import random
import requests
import socket
import time

### Configuration settings ###

HOST = '192.168.0.5'
SERVER = 'http://' + HOST + ':5000'
PORT = 5002

### Common code ###

class Message(object):
    def __init__(self, type, seq):
        self.type = type
        self.seq = seq
        self.values = {}

class LoginException(Exception):
    pass

class Connection(object):
    def __init__(self, address, port):
        self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._socket.connect((address, port))
        self._seq = 0
        self._data = bytearray(1024)
        self._next_pos = 0
        self._last_pos = 0
        self._pending = b''
        self._conversation = 0
        self._token = ''

    def __del__(self):
        self._socket.close()

    def start_message(self, type):
        self._socket.sendall(type.to_bytes(2, 'little'))
        self._socket.sendall(self._seq.to_bytes(2, 'little'))

    def send_value(self, key, value):
        self._socket.sendall((key + '=' + value).encode('utf-8'))

    def end_message(self):
        self._socket.sendall(b'\x00')

    def send_message(self, type, values = None):
        self.start_message(type)
        if (not values is None):
            for key, value in values.items():
                self.send_value(key, value)
        self.end_message()

    def receive_next_message(self, timeout = None):
        if (len(self._pending) - self._last_pos) == 0:
            self._socket.settimeout(timeout)
            self._pending = self._socket.recv(1024)
            self._last_pos = 0

        if len(self._pending) == 0:
            return None
        
        for pos in range(self._last_pos, len(self._pending)):
            if (self._next_pos < 4) or (self._pending[pos] != 0):
                self._data[self._next_pos] = self._pending[pos]
                self._next_pos += 1
                continue

            type = int.from_bytes(self._data[0:2], 'little')
            seq = int.from_bytes(self._data[2:4], 'little')
            msg = Message(type, seq)
            values = self._data[4:self._next_pos].decode('utf-8')
            if len(values) > 0:
                for value in values.split(','):
                    index = value.index('=')
                    if index < 0:
                        msg.values[value] = ''
                    else:
                        msg.values[value[0:index]] = value[index+1:]
            self._last_pos = pos + 1
            self._next_pos = 0
            return msg

    def login(self, name, password=None):
        address = SERVER + '/api/v1/session'
        if password is None:
            password = name
        req = {'name': name, 'password': password, 'role': 'robot'}
        resp = requests.post(address, json=req)
        if resp.status_code != 200:
            raise LoginException()

        data = resp.json()
        self._token = data['output']['token']
        self.send_message(1, { 'token': self._token })

    def register(self, name):
        address = SERVER + '/api/v1/robots/register'
        req = {'machineName': name}
        resp = requests.post(address, json=req)
        if resp.status_code != 200:
            raise Exception('Register failed')

    def download_code(self, msg):
        address = SERVER + '/api/v1/code/' + msg.values['user'] + '/' + msg.values['program']
        headers = {'Authorization': 'Bearer ' + self._token}
        resp = requests.get(address, headers=headers)
        if resp.status_code != 200:
            raise Exception('Code failed')

        data = resp.json()
        return data['output']['nodes']

    def set_state(self, state, name = None):
        values = {'state': state}
        if not name is None:
            values['name'] = name
        self.send_message(501, values)
        self._conversation = 0

    def record_error(self, message):
        self.send_message(503, {'message': message})

    def record_debug(self, source, status, func):
        values = {
            'sourceID': source,
            'status': status,
            'function': func
        }
        self.send_message(502, values)

class EngineSettings(object):
    ''' The configuration options for the engine. '''

    def __init__(self, msg):
        ''' Initialises the options. '''
        if msg is None:
            self.debug = False
            self.delay = 0
            return

        try:
            self.debug = msg.values['debug']
        except KeyError:
            self.debug = False

        try:
            self.delay = msg.values['delay']
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

    def __init__(self, conn, robot):
        ''' Initialises the engine. '''
        self._conn = conn
        self._robot = robot
        self._opts = EngineSettings(None)
        self._reset(None)
        self.is_cancelled = False
        self._variables = {}

        self._robot.play_led('rainbow')

    def configure(self, msg):
        ''' Configures the engine. '''
        self._opts = EngineSettings(msg)
        self.is_cancelled = False

    def cancel(self):
        ''' Cancels the current run. '''
        self.is_cancelled = True

    def run(self, ast):
        ''' Executes an AST. '''
        self._execute(ast, None, True)

    def trigger(self, block_name, value=None):
        ''' Triggers a block in the engine. '''
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
            for _ in range(0, seconds):
                if self.is_cancelled:
                    break
                time.sleep(1)

    def _error(self, message):
        self._conn.record_error(message)

    def _change_state(self, name, value):
        self._conn.set_state(value, name)

    def _debug(self, block, status):
        try:
            debug_id = block['sourceId']
            self._conn.record_debug(debug_id, status, block['token']['value'])
        except KeyError:
            pass

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

        self._error('Unknown expression type: ' + node_type)

    def _get_variable(self, name):
        try:
            return self._variables[name]
        except KeyError:
            self._error('Unknown variable ' + name)

    def _reset(self, state):
        ''' Resets the execution engine. '''
        self._robot.log('Reset')
        self._variables = {}
        self._blocks = {}
        self._last_function = None
        self._functions = {
            # Top level functions
            'reset': EngineFunction(self._reset, True),
            'start': EngineFunction(self._generate_register_block('start'), True),
            'go': EngineFunction(self._generate_execute_block('start'), True),

            # Robot functions
            'wait': EngineFunction(self._wait),
            'stop': EngineFunction(self._stop),
            'randomColour': EngineFunction(self._random_colour),
            
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
        }

    def _generate_register_block(self, block_name):
        ''' Generates a closure to register block. '''
        def _register_block(state):
            ''' Registers the on start block. '''
            self._blocks[block_name] = state.ast
        return _register_block

    def _generate_execute_block(self, block_name):
        ''' Generates a closure to execute the specified block. '''
        def _execute_block(state):
            ''' Executes the block if it exists. '''
            try:
                block = self._blocks[block_name]
            except KeyError:
                return

            self._execute(block['children'], None)

        return _execute_block

    def _wait(self, state):
        ''' Make the robot wait. '''
        seconds = self._evaluate(state.ast['arguments'][0], state)
        self._robot.log('Wait ' + str(seconds) + 's')
        for _ in range(0, int(seconds)):
            if self.is_cancelled:
                break
            time.sleep(1)


    def _stop(self, state):
        ''' Make the robot stop. '''
        self._robot.log('Stop')
        self._robot.stop()

    def _loop(self, state):
        ''' Repeat a set number of times. '''
        iterations = int(self._evaluate(state.ast['arguments'][0], state))
        for loop in range(iterations):
            self._robot.log('Loop ' + str(loop))
            self._change_state('loop', loop)
            self._execute(state.ast['children'], state)
            if self.is_cancelled:
                break

    def _define_variable(self, state):
        ''' Define or update a variable. '''
        self._robot.log('Variable')
        name = state.ast['arguments'][0]['token']['value']
        value = self._evaluate(state.ast['arguments'][1], state)
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
            self._execute(ast, state)

        return _execute_function

    def _add_to_variable(self, state):
        ''' Increases a variable. '''
        name = state.ast['arguments'][0]['token']['value']
        value = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Add')
        try:
            current = self._variables[name]
            new_value = (current + value)
            self._variables[name] = new_value
            self._change_state(name, new_value)
        except KeyError:
            self._error('Unknown variable ' + name)

    def _invert(self, state):
        ''' Inverts a value. '''
        value = self._evaluate(state.ast['arguments'][0], state)
        self._robot.log('Invert')
        return not value

    def _round(self, state):
        ''' Rounds a value. '''
        value = self._evaluate(state.ast['arguments'][0], state)
        self._robot.log('Round')
        return round(value, 0)

    def _check_if_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Equal')
        return value1 == value2

    def _check_less_than(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Less')
        return value1 < value2

    def _check_greater_than(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Greater')
        return value1 > value2

    def _check_not_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Not equal')
        return value1 != value2

    def _check_less_than_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Less equal')
        return value1 <= value2

    def _check_greater_than_equal(self, state):
        ''' Checks if the two sides are equal. '''
        value1 = self._evaluate(state.ast['arguments'][0], state)
        value2 = self._evaluate(state.ast['arguments'][1], state)
        self._robot.log('Greater equal')
        return value1 >= value2

    def _check_if_condition(self, state):
        ''' Check a condition block. '''
        result = self._evaluate(state.ast['arguments'][0], state)
        self._robot.log('Condition')
        if result is True:
            state.complete()
            self._execute(state.ast['children'], state)

    def _while(self, state):
        ''' Repeat while the condition is true. '''
        result = self._evaluate(state.ast['arguments'][0], state)
        while result is True:
            self._robot.log('While loop')
            state.complete()
            self._execute(state.ast['children'], state)
            result = self._evaluate(state.ast['arguments'][0], state)

    def _check_else(self, state):
        ''' Check a else block. '''
        self._execute(state.ast['children'], state)

    def _random_colour(self, state):
        ''' Returns a random colour. '''
        self._robot.log('Rand colour')
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

class App(object):
    def __init__(self, robot, conn, name, password):
        self._conn = conn
        self._engine = Engine(self._conn, robot)
        self._robot = robot
        try:
            self._robot.log('Logging on')
            self._conn.login(name, password)
            self._robot.log('...done')
        except LoginException:
            self._robot.log('Registering')
            self._conn.register(name)
            self._robot.log('...done')

        self._ast = None

    def start(self):
        self._robot.log('Starting')
        while True:
            msg = self._conn.receive_next_message()
            if msg.type == 2:       # Authenticate
                self._robot.log('...ready')
                self._conn.send_message(501, {'state': 'Waiting'})

            elif msg.type == 22:    # Download program
                self._conn.send_message(501, {'state': 'Downloading'})
                self._robot.log('Downloading')
                self._ast = self._conn.download_code(msg)
                self._robot.log('...done')

            elif msg.type == 101:   # Start execution
                pass

            elif msg.type == 201:   # Cancel execution
                pass


### Test code ###

class Robot(object):
    ''' Abstract the robot functions so we can test the engine. '''
    def play_led(self, name):
        print('Playing ' + name)

    def stop(self):
        print('Stopping')

    def log(self, message):
        print(message)

if __name__ == "__main__":
    robot = Robot()
    conn = Connection(HOST, PORT)
    app = App(robot, conn, socket.gethostname(), 'one')
    app.start()
