""" mBot2 Tangibles client

This client runs directly on an mBot2 client. It scans for barcodes on paper cards
and directly executes the commands. The details are then logged to the server.

This file needs to be deployed via mBlock. All the code must be in a single file,
otherwise mBlock won't compile it correctly.
"""

import cyberpi
import urequests as requests
import random
import time

# Base settings
MAX_MESSAGES = 128
NAME = cyberpi.get_name()       # This lines means we don't need to hardcode the robot name
SPEED = 15
VERSION = '1.03'

# Wifi settings - these will need to change to match the network
SERVER_BASE_ADDRESSES = ['192.168.0.151', '192.168.0.152', '192.168.0.153', '192.168.0.154']
LOGGING_URL_PREFIX = 'http://'
LOGGING_URL_SUFFIX = ':5000/api/v1/robots/' + NAME + '/logs'
WIFI_SSID= 'NaoBlocks'
WIFI_PASSWORD= 'letmein1'

# ACTIONS
ACTION_NONE = -1
ACTION_BACKWARD = 2
ACTION_CURVE_LEFT = 5
ACTION_CURVE_RIGHT = 6
ACTION_FORWARD = 1
ACTION_PLAY_A = 10
ACTION_PLAY_B = 11
ACTION_RECORD_A = 7
ACTION_RECORD_B = 8
ACTION_REPEAT = 9
ACTION_STOP = 15
ACTION_TURN_LEFT = 3
ACTION_TURN_RIGHT = 4

# Programs
PROGRAM_ACTIONS = [ACTION_RECORD_A, ACTION_RECORD_B, ACTION_PLAY_A, ACTION_PLAY_B]

class Dispatcher():
    def __init__(self, executor, robot, prefix, can_repeat):
        self.actions = []
        self.executor = executor
        self.robot = robot
        self.prefix = prefix
        self.can_repeat = can_repeat

    def execute(self, action):
        if action == ACTION_BACKWARD:
            self.robot.display_and_log(self.prefix + 'Backward')
            self.executor.backward()
        elif action == ACTION_FORWARD:
            self.robot.display_and_log(self.prefix + 'Forward')
            self.executor.forward()
        elif action == ACTION_TURN_LEFT:
            self.robot.display_and_log(self.prefix + 'Left Turn')
            self.executor.turn_left()
        elif action == ACTION_TURN_RIGHT:
            self.robot.display_and_log(self.prefix + 'Right Turn')
            self.executor.turn_right()
        elif action == ACTION_CURVE_LEFT:
            self.robot.display_and_log(self.prefix + 'Left Curve')
            self.executor.curve_left()
        elif action == ACTION_CURVE_RIGHT:
            self.robot.display_and_log(self.prefix + 'Right Curve')
            self.executor.curve_right()
        elif action == ACTION_PLAY_A:
            self.robot.display_and_log(self.prefix + 'Play A')
            self.executor.play_a()
        elif action == ACTION_PLAY_B:
            self.robot.display_and_log(self.prefix + 'Play B')
            self.executor.play_b()
        elif self.can_repeat and action == ACTION_REPEAT:
            self.robot.display_and_log(self.prefix + 'Repeat')
            self.executor.repeat()

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

    def play_a(self):
        pass

    def play_b(self):
        pass

    def record_a(self):
        pass

    def record_b(self):
        pass

    def repeat(self):
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
        if action in PROGRAM_ACTIONS:
            return False

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

        # Programs
        self.recording_program = None
        self.recording_program_id = None
        self.playing = False
        self.program_a = Program(self, robot)
        self.program_b = Program(self, robot)

        # Internal state
        self.last_action = ACTION_NONE
        self.dispatcher = Dispatcher(self, robot, '-', False)

    def backward(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_BACKWARD)
        else:
            cyberpi.mbot2.backward(self.speed, self.distance)
            self.last_action = ACTION_BACKWARD

    def can_perform(self, action):
        if action in [ACTION_RECORD_A, ACTION_RECORD_B]:
            return self.recording_program is None
        
        return not self.playing and self.recording_program_id != action

    def curve_left(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_CURVE_LEFT)
        else:
            cyberpi.mbot2.drive_power(self.speed, -self.speed * 2)
            time.sleep(self.distance)
            cyberpi.mbot2.EM_stop('all')
            self.last_action = ACTION_CURVE_LEFT

    def curve_right(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_CURVE_RIGHT)
        else:
            cyberpi.mbot2.drive_power(self.speed * 2, -self.speed)
            time.sleep(self.distance)
            cyberpi.mbot2.EM_stop('all')
            self.last_action = ACTION_CURVE_RIGHT

    def forward(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_FORWARD)
        else:
            cyberpi.mbot2.forward(self.speed, self.distance)
            self.last_action = ACTION_FORWARD

    def play_a(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_PLAY_A)
        else:
            self.playing = True
            self.program_a.play()
            self.robot.display_and_log('Done')
            self.playing = False
            self.last_action = ACTION_PLAY_A

    def play_b(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_PLAY_B)
        else:
            self.playing = True
            self.program_b.play()
            self.robot.display_and_log('Done')
            self.playing = False
            self.last_action = ACTION_PLAY_B

    def record_a(self):
        self.recording_program_id = ACTION_PLAY_A
        self.recording_program = self.program_a
        self.program_a.clear()

    def record_b(self):
        self.recording_program_id = ACTION_PLAY_B
        self.recording_program = self.program_b
        self.program_b.clear()

    def repeat(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_REPEAT)
        else:
            self.dispatcher.execute(self.last_action)

    def stop(self):
        if self.recording_program is not None:
            self.recording_program.stop()
        self.recording_program = None
        self.recording_program_id = None

    def turn_left(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_TURN_LEFT)
        else:
            cyberpi.mbot2.turn(-self.angle, self.speed)
            self.last_action = ACTION_TURN_LEFT

    def turn_right(self):
        if self.recording_program is not None:
            self.recording_program.add(ACTION_TURN_RIGHT)
        else:
            cyberpi.mbot2.turn(self.angle, self.speed)
            self.last_action = ACTION_TURN_RIGHT

class UniqueActions(DiscreteActions):
    def __init__(self, robot):
        super().__init__(robot)

        # Identification details
        self.name = 'Unique'
        self.code = 'U'

    def can_perform(self, action):
        if action in PROGRAM_ACTIONS:
            return False

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
        self.is_sending = False

        # Logging messages
        self.messages = [''] * MAX_MESSAGES
        self.send_pos = -1
        self.write_pos = -1

        # Update robot
        cyberpi.smart_camera.set_mode(mode = "line")
        cyberpi.quad_rgb_sensor.set_led('w')

        # Modes
        self.modes = [
            ContinuousActions(self),
            DiscreteActions(self),
            RandomValueActions(self),
            UniqueActions(self),
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

    def display_and_log(self, message, type='action', clear_screen = False):
        self.display(message, clear_screen)

        next_pos = self.write_pos + 1
        self.messages[next_pos & 127] = str(cyberpi.timer.get()) + ':' + type + ':' + message
        self.write_pos = next_pos

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
                self.display(address)
                self.logging_url = LOGGING_URL_PREFIX + address + LOGGING_URL_SUFFIX
                resp = requests.post(self.logging_url, json=data)
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

        self.display_and_log('Ready [' + self.actions.code + ']', 'status')

        self.is_running = True
        self.has_stopped = True
        while self.is_running:
            if cyberpi.smart_camera.detect_label(ACTION_STOP):
                self.stop()
            
            elif cyberpi.smart_camera.detect_label(ACTION_FORWARD):
                if self.actions.can_perform(ACTION_FORWARD):
                    self.display_and_log('Forward')
                    self.has_stopped = False
                    self.actions.forward()

            elif cyberpi.smart_camera.detect_label(ACTION_BACKWARD):
                if self.actions.can_perform(ACTION_BACKWARD):
                    self.display_and_log('Backward')
                    self.has_stopped = False
                    self.actions.backward()

            elif cyberpi.smart_camera.detect_label(ACTION_TURN_LEFT):
                if self.actions.can_perform(ACTION_TURN_LEFT):
                    self.display_and_log('Left Turn')
                    self.has_stopped = False
                    self.actions.turn_left()

            elif cyberpi.smart_camera.detect_label(ACTION_TURN_RIGHT):
                if self.actions.can_perform(ACTION_TURN_RIGHT):
                    self.display_and_log('Right Turn')
                    self.has_stopped = False
                    self.actions.turn_right()

            elif cyberpi.smart_camera.detect_label(ACTION_CURVE_LEFT):
                if self.actions.can_perform(ACTION_CURVE_LEFT):
                    self.display_and_log('Left Curve')
                    self.has_stopped = False
                    self.actions.curve_left()

            elif cyberpi.smart_camera.detect_label(ACTION_CURVE_RIGHT):
                if self.actions.can_perform(ACTION_CURVE_RIGHT):
                    self.display_and_log('Right Curve')
                    self.has_stopped = False
                    self.actions.curve_right()

            elif cyberpi.smart_camera.detect_label(ACTION_RECORD_A):
                if self.actions.can_perform(ACTION_RECORD_A):
                    self.display_and_log('Record A')
                    self.has_stopped = False
                    self.actions.record_a()

            elif cyberpi.smart_camera.detect_label(ACTION_RECORD_B):
                if self.actions.can_perform(ACTION_RECORD_B):
                    self.display_and_log('Record B')
                    self.has_stopped = False
                    self.actions.record_b()

            elif cyberpi.smart_camera.detect_label(ACTION_PLAY_A):
                if self.actions.can_perform(ACTION_PLAY_A):
                    self.display_and_log('Play A')
                    self.has_stopped = False
                    self.actions.play_a()

            elif cyberpi.smart_camera.detect_label(ACTION_PLAY_B):
                if self.actions.can_perform(ACTION_PLAY_B):
                    self.display_and_log('Play B')
                    self.has_stopped = False
                    self.actions.play_b()

            elif cyberpi.smart_camera.detect_label(ACTION_REPEAT):
                if self.actions.can_perform(ACTION_REPEAT):
                    self.display_and_log('Repeat')
                    self.has_stopped = False
                    self.actions.repeat()

    def send_to_server(self):
        # Check if we are sending: if the last send is still running we
        # don't want to start a new send (yet)
        if self.is_sending or not self.is_connected or not cyberpi.wifi.is_connect():
            return

        self.is_sending = True
        if self.send_pos == self.write_pos:
            self.is_sending = False
            return

        messages = []
        while self.send_pos < self.write_pos:
            self.send_pos += 1
            messages.append(self.messages[self.send_pos & 127])

        if len(messages) > 0:
            data = {
                "action": "log",
                "messages": messages,
                "time": cyberpi.timer.get()
            }
            try:
                self.display('Sending')
                requests.post(self.logging_url, json=data)
            except:
                # Should do something if the sending fails, but for now we will ignore it
                self.display('...failed')

        self.is_sending = False

    # Stop is a special command - we always want it to stop anything that is happening
    def stop(self, message = 'Stop', restart = True):
        cyberpi.mbot2.EM_stop('all')
        if not self.has_stopped:
            self.display_and_log(message)
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
            self.display_and_log('Mode change', clear_screen=True)

        self.actions = self.modes[self.mode]
        self.display_and_log(self.actions.name, 'mode')

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

# Pushing the joystick up changes mode
@cyberpi.event.is_press('up')
def button_up_callback():
    if r.is_running:
        return
    r.toggle_mode(1)
        
# Pushing the joystick down changes mode
@cyberpi.event.is_press('down')
def button_down_callback():
    if r.is_running:
        return
    r.toggle_mode(-1)

# Periodically send the logs to the server
@cyberpi.event.greater_than(5, 'timer')
def timer_callback():
    r.send_to_server()
    cyberpi.timer.reset()
