""" mBot2 Test client

This client runs directly on an mBot2 client. It contains the server interaction code: it allows for testing the
communications between an mBot robot and the NaoBlocks server.
"""

import cyberpi
import urequests as requests
import time

# Base settings
MAX_MESSAGES = 128
NAME = cyberpi.get_name()       # This lines means we don't need to hardcode the robot name
SPEED = 15
VERSION = '1.04'

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
            return False

        self.is_connected = True
        self.display('...retrieved')
        options = resp.json()
        try:
            values = options['values']
        except KeyError:
            # No options, we can stop parsing here
            return True

        try:
            self.mode = int(values['mode'])
        except:
            pass

        try:
            self.map = values['map']
        except:
            self.map = 0
            
        return True

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

class Dispatcher():
    def __init__(self, robot, prefix):
        self.robot = robot
        self.prefix = prefix

    def execute(self, action):
        if action == ACTION_BACKWARD:
            self.robot.display_and_log(self.prefix + 'Backward')
        elif action == ACTION_FORWARD:
            self.robot.display_and_log(self.prefix + 'Forward')
        elif action == ACTION_TURN_LEFT:
            self.robot.display_and_log(self.prefix + 'Left Turn')
        elif action == ACTION_TURN_RIGHT:
            self.robot.display_and_log(self.prefix + 'Right Turn')
        elif action == ACTION_CURVE_LEFT:
            self.robot.display_and_log(self.prefix + 'Left Curve')
        elif action == ACTION_CURVE_RIGHT:
            self.robot.display_and_log(self.prefix + 'Right Curve')

r = Robot()
send_dispatcher = Dispatcher(r, '')
process_dispatcher = Dispatcher(r, '>')

# Initialize the test harness on startup
@cyberpi.event.start
def on_start():
    r.initialise()

# Request program from server
@cyberpi.event.is_press('a')
def button_a_callback():
    r.display('Request')

# Send transaction batch to server
@cyberpi.event.is_press('b')
def button_b_callback():
    r.display('Send')
    r.send_to_server()

@cyberpi.event.is_press('left')
def button_left_callback():
    send_dispatcher.execute(ACTION_TURN_LEFT)
        
@cyberpi.event.is_press('right')
def button_right_callback():
    send_dispatcher.execute(ACTION_TURN_RIGHT)

@cyberpi.event.is_press('up')
def button_up_callback():
    send_dispatcher.execute(ACTION_FORWARD)
        
@cyberpi.event.is_press('down')
def button_down_callback():
    send_dispatcher.execute(ACTION_BACKWARD)
