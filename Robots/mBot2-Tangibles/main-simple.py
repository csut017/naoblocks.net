""" mBot2 Tangibles client (Simple)

This client runs directly on an mBot2 client. It scans for barcodes on paper cards
and directly executes the commands. The details are then logged to the server.

This file needs to be deployed via mBlock. All the code must be in a single file,
otherwise mBlock won't compile it correctly.

This version just has the code for executing tangibles - it does not log to the server
or execute any of the extended commands (e.g., music cards).
"""

import cyberpi
import random
import time

# Base settings
NAME = cyberpi.get_name()       # This lines means we don't need to hardcode the robot name
SPEED = 15
VERSION = '1.09s'

# ACTIONS
ACTION_NONE = -1
ACTION_BACKWARD = 2
ACTION_CURVE_LEFT = 5
ACTION_CURVE_RIGHT = 13
ACTION_FORWARD = 1
ACTION_STOP = 15
ACTION_TURN_LEFT = 3
ACTION_TURN_RIGHT = 4

# Programs
MOVEMENT_ACTIONS = [ACTION_FORWARD, ACTION_BACKWARD, ACTION_TURN_LEFT, ACTION_TURN_RIGHT, ACTION_CURVE_LEFT, ACTION_CURVE_RIGHT]

class Dispatcher():
    def __init__(self, executor, robot, prefix, can_repeat):
        self.actions = []
        self.executor = executor
        self.robot = robot
        self.prefix = prefix
        self.can_repeat = can_repeat

    def execute(self, action):
        """
        Execute action and log the action to the robot cyberpi console.
        """

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
        # Add an action to the action list 
        self.actions.append(action)
        cyberpi.led.play('meteor_green')

    def clear(self):
        # Clear all actions from action list
        self.actions.clear()

    def play(self):
        # Loop through all actions and execute them
        for action in self.actions:
            if self.robot.has_stopped:
                break
            
            # Execute the action (from dispatcher class)
            self.dispatcher.execute(action)

    def stop(self):
        pass

class Actions():
    """
    Base class for actions (super class extended by ContinuousActions and DiscreteActions).
    Will be overridden by the specific action classes.
    """
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

    def play_song(self):
        pass

    def play_sequence(self, key):
        pass

    def record_sequence(self, key):
        pass

    def clear_song(self):
        pass
    

class ContinuousActions(Actions):
    """
    Continuous Mode
    """
    def __init__(self, robot):
        # Identification details
        self.name = 'Continuous'
        self.code = 'C'

        # Configuration options
        self.robot = robot
        self.speed = SPEED

        # Internal state
        self.last_action = ACTION_NONE # used to record the last action performed (to prevent repeating the same action)

    def backward(self):        
        cyberpi.mbot2.backward(self.speed)
        self.last_action = ACTION_BACKWARD

    def can_perform(self, action):
        # Cannot perform the same action twice in a row (e.g. will not be able to scan forward when it is already moving forward)
        return self.last_action != action

    def curve_left(self):
        # left_power spins forward at "speed", right_power spins backward at "-speed * 2"
        cyberpi.mbot2.drive_power(self.speed, -self.speed * 2)
        self.last_action = ACTION_CURVE_LEFT

    def curve_right(self):
        # left_power spins forward at "speed * 2", right_power spins backward at "-speed"
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
    """
    Discrete Mode
    """

    def __init__(self, robot):
        # Identification details
        self.name = 'Discrete'
        self.code = 'D'

        # Configuration options
        self.robot = robot
        self.speed = SPEED
        self.distance = 2 # preset distance 
        self.angle = 45 # angle of the turns

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

    def stop(self):
        self.recording_program = None
        self.recording_program_id = None

    def turn_left(self):
        cyberpi.mbot2.turn(-self.angle, self.speed)
        self.last_action = ACTION_TURN_LEFT

    def turn_right(self):
        cyberpi.mbot2.turn(self.angle, self.speed)
        self.last_action = ACTION_TURN_RIGHT

class UniqueActions(DiscreteActions):
    """
    Unique Mode
    """

    def __init__(self, robot):
        super().__init__(robot)

        # Identification details
        self.name = 'Unique'
        self.code = 'U'

    def can_perform(self, action):
        # Can only perform an action if it is not the same as the last action
        can_perform = self.last_action != action
        return can_perform and super().can_perform(action)

class RandomValueActions(DiscreteActions):
    """
    Random Value Mode
    """
    
    def __init__(self, robot):
        super().__init__(robot)

        # Identification details
        self.name = 'RandVal'
        self.code = 'RV'

    def can_perform(self, action):
        if not action in MOVEMENT_ACTIONS:
            return False

        # Random values for speed, distance and angle
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

    def display(self, message):
        """
        Display a message on the console and clear the screen if needed.
        """

        self.lines += 1
        if self.lines > 6:
            cyberpi.console.clear()
            self.lines = 1
            cyberpi.console.print('>')
            cyberpi.console.print(NAME)
            
        cyberpi.console.println(' ')
        cyberpi.console.print(message)

    def initialise(self):
        """
        Initialise the robot by connecting to Wi-Fi and the logging server.
        """

        self.display('Version ' + VERSION)
        self.toggle_mode(0)
        return True

    def run(self):
        """
        Run the robot by scanning for barcodes and executing the actions.
        """

        cyberpi.smart_camera.open_light()
        cyberpi.led.play('flash_red')

        self.display('Ready [' + self.actions.code + ']')

        self.is_running = True
        self.has_stopped = True

        # self.actions refers to the current mode (Continuous, Discrete, etc.)
        # Each mode defines how actions behave and whether they can be performed
        while self.is_running:
            if cyberpi.ultrasonic2.get() < 6:
                self.stop()
                cyberpi.ultrasonic2.led_show([0, 0, 0, 0, 0, 0, 0, 0])
            else:
                cyberpi.ultrasonic2.led_show([100, 100, 100, 100, 100, 100, 100, 100])
                if cyberpi.smart_camera.detect_label(ACTION_STOP):
                    self.stop()
                
                elif cyberpi.smart_camera.detect_label(ACTION_FORWARD):
                    if self.actions.can_perform(ACTION_FORWARD):
                        self.display('Forward')
                        self.has_stopped = False
                        self.actions.forward()

                elif cyberpi.smart_camera.detect_label(ACTION_BACKWARD):
                    if self.actions.can_perform(ACTION_BACKWARD):
                        self.display('Backward')
                        self.has_stopped = False
                        self.actions.backward()

                elif cyberpi.smart_camera.detect_label(ACTION_TURN_LEFT):
                    if self.actions.can_perform(ACTION_TURN_LEFT):
                        self.display('Left Turn')
                        self.has_stopped = False
                        self.actions.turn_left()

                elif cyberpi.smart_camera.detect_label(ACTION_TURN_RIGHT):
                    if self.actions.can_perform(ACTION_TURN_RIGHT):
                        self.display('Right Turn')
                        self.has_stopped = False
                        self.actions.turn_right()

                elif cyberpi.smart_camera.detect_label(ACTION_CURVE_LEFT):
                    if self.actions.can_perform(ACTION_CURVE_LEFT):
                        self.display('Left Curve')
                        self.has_stopped = False
                        self.actions.curve_left()

                elif cyberpi.smart_camera.detect_label(ACTION_CURVE_RIGHT):
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

    def toggle_mode(self, direction):
        """
        Toggle the mode of the robot. The direction can be 1 (next) or -1 (previous).
        """
        
        self.mode += direction
        if self.mode >= len(self.modes):
            self.mode = 0
        elif self.mode < 0:
            self.mode = len(self.modes) - 1
            
        self.display('Mode change')

        self.actions = self.modes[self.mode]
        self.display(self.actions.name)

r = Robot()

# Start the robot when the CyberPi starts
@cyberpi.event.start
def on_start():
    cyberpi.speaker.set_vol(100)
    cyberpi.smart_camera.close_light()
    if r.initialise(): # Start the robot
        r.run() # Start barcode scanning and action execution loop

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
