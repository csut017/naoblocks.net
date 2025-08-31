""" mBot2 Tangibles client

This client runs directly on an mBot2 client. It polls the server for commands
and directly executes them. 

This file needs to be deployed via mBlock. All the code must be in a single file,
otherwise mBlock won't compile it correctly.
"""

import cyberpi
import urequests as requests
import random
import time

# Base settings
NAME = cyberpi.get_name()       # This lines means we don't need to hardcode the robot name
SPEED = 15
VERSION = '1.07'

# Wifi settings - these will need to change to match the network
SERVER_BASE_ADDRESSES = ['192.168.68.111']
URL_PREFIX = 'http://'
VERSION_URL_SUFFIX = ':5000/api/v1/version'
GET_URL_SUFFIX = ':5000/api/v1/passthrough/' + NAME
WIFI_SSID= 'NaoBlocks'
WIFI_PASSWORD= 'letmein1'

# ACTIONS
ACTION_NONE = 32            # space
ACTION_BACKWARD = 66        # B
ACTION_CURVE_LEFT = 69      # E
ACTION_CURVE_RIGHT = 70     # F
ACTION_FORWARD = 65         # A
ACTION_STOP = 71            # G
ACTION_TURN_LEFT = 67       # C
ACTION_TURN_RIGHT = 68      # D

class Dispatcher():
    def __init__(self, executor, robot, prefix, can_repeat):
        self.actions = []
        self.executor = executor
        self.robot = robot
        self.prefix = prefix
        self.can_repeat = can_repeat

    def execute(self, action):
        if action == ACTION_BACKWARD:
            self.robot.display(self.prefix + 'Backward')
            self.executor.backward()
        elif action == ACTION_FORWARD:
            self.robot.display(self.prefix + 'Forward')
            self.executor.forward()
        elif action == ACTION_TURN_LEFT:
            self.robot.display(self.prefix + 'Left Turn')
            self.executor.turn_left()
        elif action == ACTION_TURN_RIGHT:
            self.robot.display(self.prefix + 'Right Turn')
            self.executor.turn_right()
        elif action == ACTION_CURVE_LEFT:
            self.robot.display(self.prefix + 'Left Curve')
            self.executor.curve_left()
        elif action == ACTION_CURVE_RIGHT:
            self.robot.display(self.prefix + 'Right Curve')
            self.executor.curve_right()

class Program():
    def __init__(self, executor, robot):
        self.actions = []
        self.executor = executor
        self.robot = robot
        self.dispatcher = Dispatcher(executor, robot, ':', True)

    def add(self, action):
        self.actions.append(action)
        cyberpi.led.play('meteor_green')

    def clear(self):
        self.actions.clear()

    def play(self):
        for action in self.actions:
            if self.robot.has_stopped:
                break

            self.dispatcher.execute(action)

    def stop(self):
        pass

class Actions():
    def backward(self):        
        pass

    def can_perform(self, _):
        return False

    def curve_left(self):
        pass

    def curve_right(self):
        pass

    def forward(self):
        pass

    def stop(self):
        pass

    def turn_left(self):
        pass

    def turn_right(self):
        pass

class ContinuousActions(Actions):
    def __init__(self, robot):
        # Identification details
        self.name = 'Continuous'
        self.code = 'C'

        # Configuration options
        self.robot = robot
        self.speed = SPEED

        # Internal state
        self.last_action = ACTION_NONE

    def backward(self):        
        cyberpi.mbot2.backward(self.speed)
        self.last_action = ACTION_BACKWARD

    def can_perform(self, action):
        return self.last_action != action

    def curve_left(self):
        cyberpi.mbot2.drive_power(self.speed, -self.speed * 2)
        self.last_action = ACTION_CURVE_LEFT

    def curve_right(self):
        cyberpi.mbot2.drive_power(self.speed * 2, -self.speed)
        self.last_action = ACTION_CURVE_RIGHT

    def forward(self):
        cyberpi.mbot2.forward(self.speed)
        self.last_action = ACTION_FORWARD

    def stop(self):
        self.last_action = ACTION_NONE

    def turn_left(self):
        cyberpi.mbot2.turn_left(self.speed)
        self.last_action = ACTION_TURN_LEFT

    def turn_right(self):
        cyberpi.mbot2.turn_right(self.speed)
        self.last_action = ACTION_TURN_RIGHT

class DiscreteActions(Actions):
    def __init__(self, robot):
        # Identification details
        self.name = 'Discrete'
        self.code = 'D'

        # Configuration options
        self.robot = robot
        self.speed = SPEED
        self.distance = 2
        self.angle = 45

        # Internal state
        self.last_action = ACTION_NONE
        self.dispatcher = Dispatcher(self, robot, '-', False)

    def backward(self):
        cyberpi.mbot2.backward(self.speed, self.distance)
        self.last_action = ACTION_BACKWARD

    def can_perform(self, action):
        return True

    def curve_left(self):
        cyberpi.mbot2.drive_power(self.speed, -self.speed * 2)
        time.sleep(self.distance)
        cyberpi.mbot2.EM_stop('all')
        self.last_action = ACTION_CURVE_LEFT

    def curve_right(self):
        cyberpi.mbot2.drive_power(self.speed * 2, -self.speed)
        time.sleep(self.distance)
        cyberpi.mbot2.EM_stop('all')
        self.last_action = ACTION_CURVE_RIGHT

    def forward(self):
        cyberpi.mbot2.forward(self.speed, self.distance)
        self.last_action = ACTION_FORWARD

    def turn_left(self):
        cyberpi.mbot2.turn(-self.angle, self.speed)
        self.last_action = ACTION_TURN_LEFT

    def turn_right(self):
        cyberpi.mbot2.turn(self.angle, self.speed)
        self.last_action = ACTION_TURN_RIGHT

class UniqueActions(DiscreteActions):
    def __init__(self, robot):
        super().__init__(robot)

        # Identification details
        self.name = 'Unique'
        self.code = 'U'

    def can_perform(self, action):
        can_perform = self.last_action != action
        return can_perform and super().can_perform(action)

class RandomValueActions(DiscreteActions):
    def __init__(self, robot):
        super().__init__(robot)

        # Identification details
        self.name = 'RandVal'
        self.code = 'RV'

    def can_perform(self, action):
        self.speed = random.randint(1, 5) * 5
        self.distance = random.randint(1, 4)
        self.angle = random.randint(2, 6) * 10 + 5
        return super().can_perform(action)


class Robot():
    def __init__(self):
        # Internal state
        self.has_stopped = True
        self.lines = 100
        self.is_running = False
        self.mode = 0
        self.is_connected = False

        # Update robot
        cyberpi.smart_camera.set_mode(mode = "line")
        cyberpi.quad_rgb_sensor.set_led('w')

        # Modes
        self.modes = [
            DiscreteActions(self),
            RandomValueActions(self),
            UniqueActions(self),
            ContinuousActions(self),
        ]

    def display(self, message, clear_screen = False):
        self.lines += 1
        if clear_screen or (self.lines > 6):
            cyberpi.console.clear()
            self.lines = 1
            cyberpi.console.print('>')
            cyberpi.console.print(NAME)
            
        cyberpi.console.println(' ')
        cyberpi.console.print(message)

    def initialise(self):
        self.display('Version ' + VERSION)
        cyberpi.wifi.connect(WIFI_SSID, WIFI_PASSWORD)
        count = 20
        self.display('Wifi connect')
        while (count > 0) and not cyberpi.wifi.is_connect():
            time.sleep(0.5)
            count -= 1

        if not cyberpi.wifi.is_connect():
            self.display('...failed')
            self.toggle_mode(0, False)
            return False

        self.display('...connected')
        self.display('Check config')
        data = {
            "action": "init",
            "version": VERSION
        }

        # Check each of the possible addresses to se if any are active
        is_connected = False
        for address in SERVER_BASE_ADDRESSES:
            try:
                version_url = URL_PREFIX + address + VERSION_URL_SUFFIX
                self.getting_url = URL_PREFIX + address + GET_URL_SUFFIX
                resp = requests.get(version_url)
                if resp.status_code == 200:
                    is_connected = True
                    break
            except:
                pass

        self.display('...done...')

        if not is_connected:
            # We don't care how it failed, only that it failed
            self.display('...failed')
            self.toggle_mode(0, False)
            return False

        self.is_connected = True
        self.display('...retrieved')
        options = resp.json()
        try:
            values = options['values']
        except KeyError:
            # No options, we can stop parsing here
            self.toggle_mode(0, False)
            return True

        try:
            self.mode = int(values['mode'])
        except:
            pass

        try:
            self.map = values['map']
        except:
            self.map = 0
            
        self.toggle_mode(0, False)
        return True

    def run(self):
        cyberpi.smart_camera.open_light()
        cyberpi.led.play('flash_red')

        self.display('Running [' + self.actions.code + ']', 'status')

        self.is_running = True
        self.has_stopped = True
        while self.is_running:
            self.display('Polling')
            response = requests.get(self.getting_url).content
            while len(response) > 0:
                command = response[0]
                response = response[1:]

                if command == ACTION_STOP:
                    self.stop()
                
                elif command == ACTION_FORWARD:
                    if self.actions.can_perform(ACTION_FORWARD):
                        self.display('Forward')
                        self.has_stopped = False
                        self.actions.forward()

                elif command == ACTION_BACKWARD:
                    if self.actions.can_perform(ACTION_BACKWARD):
                        self.display('Backward')
                        self.has_stopped = False
                        self.actions.backward()

                elif command == ACTION_TURN_LEFT:
                    if self.actions.can_perform(ACTION_TURN_LEFT):
                        self.display('Left Turn')
                        self.has_stopped = False
                        self.actions.turn_left()

                elif command == ACTION_TURN_RIGHT:
                    if self.actions.can_perform(ACTION_TURN_RIGHT):
                        self.display('Right Turn')
                        self.has_stopped = False
                        self.actions.turn_right()

                elif command == ACTION_CURVE_LEFT:
                    if self.actions.can_perform(ACTION_CURVE_LEFT):
                        self.display('Left Curve')
                        self.has_stopped = False
                        self.actions.curve_left()

                elif command == ACTION_CURVE_RIGHT:
                    if self.actions.can_perform(ACTION_CURVE_RIGHT):
                        self.display('Right Curve')
                        self.has_stopped = False
                        self.actions.curve_right()

    # Stop is a special command - we always want it to stop anything that is happening
    def stop(self, message = 'Stop', restart = True):
        cyberpi.mbot2.EM_stop('all')
        if not self.has_stopped:
            self.display(message)
            self.has_stopped = True

        self.actions.stop()
        if not restart:
            self.is_running = False
            cyberpi.led.on('y')
        else:
            self.is_running = True
            cyberpi.led.on('r')

    def toggle_mode(self, direction, is_change = True):
        self.mode += direction
        if self.mode >= len(self.modes):
            self.mode = 0
        elif self.mode < 0:
            self.mode = len(self.modes) - 1
            
        if is_change:   
            self.display('Mode change', clear_screen=True)

        self.actions = self.modes[self.mode]
        self.display(self.actions.name, 'mode')

r = Robot()

# Start the robot when the CyberPi starts
@cyberpi.event.start
def on_start():
    cyberpi.speaker.set_vol(100)
    cyberpi.smart_camera.close_light()
    if r.initialise():
        r.run()

# Handle the stop (square) button
@cyberpi.event.is_press('a')
def button_a_callback():
    cyberpi.smart_camera.close_light()
    r.stop('Halt', False)           # Don't resume execution

# Start running when a reset message is received
@cyberpi.event.receive('reset')
def reset_callback():
    r.run()

# Handle the play (triangle) button
@cyberpi.event.is_press('b')
def button_b_callback():
    if r.is_running:
        r.stop('Reset')
    else:
        cyberpi.broadcast('reset')

# Start running when a reset message is received
@cyberpi.event.receive('reset')
def reset_callback():
    r.run()
