""" mBot2 Tangibles client

This client runs directly on an mBot2 client. It scans for barcodes on paper cards
and directly executes the commands. The details are then logged to the server.

This file needs to be deployed via mBlock. All the code must be in a single file,
otherwise mBlock won't compile it correctly.
"""

import cyberpi
import random
import time
import urequests as requests

VERSION = '1.01'

BLINK_DELAY = 5
MAX_MESSAGES = 128
NAME = cyberpi.get_name()       # This lines means we don't need to hardcode the robot name
SPEED = 15

# Wifi settings - these will need to change to match the network
SERVER_BASE_ADDRESS = 'http://192.168.0.151:5000/'
LOGGING_URL = SERVER_BASE_ADDRESS + 'api/v1/robots/' + NAME + '/logs'
WIFI_SSID= 'NaoBlocks'
WIFI_PASSWORD= 'letmein1'

#Modes
MODE_CONTINUOUS = 1
MODE_DISCRETE = 2
MODE_DISCRETE_NO_REPEAT = 3
MODE_FIRST = MODE_CONTINUOUS
MODE_LAST = MODE_DISCRETE_NO_REPEAT

class robot():
    def __init__(self):
        self.lines = 100
        self.last_command = -1
        self.last_location = -1
        self.has_stopped = True
        self.count = 32000
        self.is_running = False
        self.is_connected = False
        self.messages = [''] * MAX_MESSAGES
        self.write_pos = -1
        self.send_pos = -1
        self.mode = MODE_CONTINUOUS
        self.map = 0
        self.change_map = False
        cyberpi.smart_camera.set_mode(mode = "line")
        cyberpi.quad_rgb_sensor.set_led('w')
        
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
        self.lines += 1
        if clear_screen or (self.lines > 6):
            cyberpi.console.clear()
            self.lines = 1
            cyberpi.console.print('>')
            cyberpi.console.print(NAME)
            
        cyberpi.console.println(' ')
        cyberpi.console.print(message)
        next_pos = self.write_pos + 1
        self.messages[next_pos & 127] = str(cyberpi.timer.get()) + ':' + type + ':' + message
        self.write_pos = next_pos
            
    def toggle_mode(self, direction):
        self.mode += direction
        if self.mode > MODE_LAST:
            self.mode = MODE_FIRST
        elif self.mode < MODE_FIRST:
            self.mode = MODE_LAST
            
        self.display('Version ' + VERSION)
        self.display_and_log('Mode change', clear_screen=True)
        if self.mode == MODE_CONTINUOUS:
            self.display_and_log('Continuous', 'mode')
        elif self.mode == MODE_DISCRETE:
            self.display_and_log('Discrete', 'mode')
        elif self.mode == MODE_DISCRETE_NO_REPEAT:
            self.display_and_log('No Repeat', 'mode')
        self.last_command = -1

    def stop(self, message = 'Stop', restart = True):
        cyberpi.mbot2.EM_stop('all')
        if not self.has_stopped:
            self.display_and_log(message)
            self.has_stopped = True
        cyberpi.speaker.play_music('stop')      # This file should not exist, therefore it will stop the sound
        self.count = 0
        if not restart:
            self.is_running = False
            cyberpi.led.on('y')
        else:
            self.is_running = True
            cyberpi.led.on('r')
               
    def run(self):
        blink_bottom = True
        if self.mode == MODE_CONTINUOUS:
            self.display_and_log('Ready [C]', 'status')
        elif self.mode == MODE_DISCRETE:
            self.display_and_log('Ready [D]', 'status')
        elif self.mode == MODE_DISCRETE_NO_REPEAT:
            self.display_and_log('Ready [N]', 'status')
        else:
            self.display_and_log('Unknown', 'status')
            return

        cyberpi.smart_camera.open_light()
        cyberpi.led.play('flash_red')
        self.is_running = True
        while self.is_running:
            self.count -= 1
            if self.count < 0:
                if blink_bottom:
                    cyberpi.quad_rgb_sensor.set_led('k')
                    self.count = BLINK_DELAY
                else:
                    cyberpi.quad_rgb_sensor.set_led('w')
                    self.count = 32000
                blink_bottom = not blink_bottom

            if cyberpi.smart_camera.detect_label(12):
                self.stop()
                continue
    
            if cyberpi.smart_camera.detect_label(3):
                self.stop('Ouch')
                angle = random.randint(-45, 45)
                cyberpi.mbot2.turn(angle, 2 * SPEED)
                cyberpi.mbot2.forward(-SPEED, 2)
                self.last_command = -1
                continue
    
            elif self.last_command != 1 and cyberpi.smart_camera.detect_label(1):
                # Forwards
                self.display_and_log('Forward')
                if self.mode == MODE_CONTINUOUS:
                    cyberpi.mbot2.forward(SPEED)
                elif self.mode == MODE_DISCRETE:
                    cyberpi.mbot2.forward(SPEED, 2)
                elif self.mode == MODE_DISCRETE_NO_REPEAT:
                    cyberpi.mbot2.forward(SPEED, 2)
                cyberpi.led.on('b')
                self.has_stopped = False

                self.count = 0
                if self.mode != MODE_DISCRETE:
                    self.last_command = 1
                continue
            
            elif self.last_command != 2 and cyberpi.smart_camera.detect_label(2):
                # Turn left
                self.display_and_log('Left')
                if self.mode == MODE_CONTINUOUS:
                    cyberpi.mbot2.turn_left(SPEED)
                elif self.mode == MODE_DISCRETE:
                    cyberpi.mbot2.turn(-45, SPEED)
                elif self.mode == MODE_DISCRETE_NO_REPEAT:
                    cyberpi.mbot2.turn(-45, SPEED)
                cyberpi.led.show('k k k w b')
                self.has_stopped = False

                self.count = 0
                if self.mode != MODE_DISCRETE:
                    self.last_command = 2
                continue
            
            elif self.last_command != 5 and cyberpi.smart_camera.detect_label(5):
                # Turn right
                self.display_and_log('Right')
                if self.mode == MODE_CONTINUOUS:
                    cyberpi.mbot2.turn_right(SPEED)
                elif self.mode == MODE_DISCRETE:
                    cyberpi.mbot2.turn(45, SPEED)
                elif self.mode == MODE_DISCRETE_NO_REPEAT:
                    cyberpi.mbot2.turn(45, SPEED)
                cyberpi.led.show('b w k k k')
                self.has_stopped = False

                self.count = 0
                if self.mode != MODE_DISCRETE:
                    self.last_command = 5
                continue

            elif self.last_location != 1 and cyberpi.smart_camera.detect_label(7):
                self.display_and_log('Location 1', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '1')
                self.last_command = -1
                self.last_location = 1
                continue

            elif self.last_location != 2 and cyberpi.smart_camera.detect_label(8):
                self.display_and_log('Location 2', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '2')
                self.last_command = -1
                self.last_location = 2
                continue

            elif self.last_location != 3 and cyberpi.smart_camera.detect_label(9):
                self.display_and_log('Location 3', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '3')
                self.last_command = -1
                self.last_location = 3
                continue

            elif self.last_location != 4 and cyberpi.smart_camera.detect_label(10):
                self.display_and_log('Location 4', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '4')
                self.last_command = -1
                self.last_location = 4
                continue

            elif self.last_location != 5 and cyberpi.smart_camera.detect_label(11):
                self.display_and_log('Location 5', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '5')
                self.last_command = -1
                self.last_location = 5
                continue

            elif self.last_location != 6 and cyberpi.smart_camera.detect_label(12):
                self.display_and_log('Location 6', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '6')
                self.last_command = -1
                self.last_location = 6
                continue

            elif self.last_location != 7 and cyberpi.smart_camera.detect_label(13):
                self.display_and_log('Location 7', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '7')
                self.last_command = -1
                self.last_location = 7
                continue

            elif self.last_location != 8 and cyberpi.smart_camera.detect_label(14):
                self.display_and_log('Location 8', 'location')
                if self.map > 0:
                    cyberpi.speaker.play_music('kj' + str(self.map) + '8')
                self.last_command = -1
                self.last_location = 8
                continue

        self.display_and_log('Stopped', 'status')

    def send_to_server(self):
        # Check if we are sending: if the last send is still running we
        # don't want to start a new send (yet)
        if not self.is_connected or not cyberpi.wifi.is_connect():
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
                requests.post(LOGGING_URL, json=data)
            except:
                # Should do something if the sending fails, but for now we will ignore it
                self.display('...failed')

        self.is_sending = False

    def initialise(self):
        cyberpi.wifi.connect(WIFI_SSID, WIFI_PASSWORD)
        count = 20
        self.display('Version ' + VERSION)
        self.display('Wifi connect')
        while (count > 0) and not cyberpi.wifi.is_connect():
            time.sleep(0.5)
            count -= 1

        if not cyberpi.wifi.is_connect():
            self.display('...failed')
            return False

        self.display('...connected')
        self.display('Get config')
        data = {
            "action": "init",
            "version": VERSION
        }
        try:
            resp = requests.post(LOGGING_URL, json=data)
            if resp.status_code != 200:
                self.display('...failed')
                return False
        except:
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
            self.mode = MODE_CONTINUOUS

        try:
            self.map = int(values['map'])
        except:
            self.map = 0
            
        return True

    def record_move(self):
        if not self.is_running:
            return

        self.display_and_log('...moved...', 'external')

r = robot()

@cyberpi.event.is_press('a')
def button_a_callback():
    cyberpi.smart_camera.close_light()
    r.stop('Halt', False)           # Don't resume execution

@cyberpi.event.is_press('b')
def button_b_callback():
    if r.is_running:
        r.stop('Reset')
    else:
        cyberpi.broadcast('reset')

@cyberpi.event.is_press('up')
def button_up_callback():
    if r.is_running:
        return
    r.toggle_mode(1)
        
@cyberpi.event.is_press('down')
def button_down_callback():
    if r.is_running:
        return
    r.toggle_mode(-1)

@cyberpi.event.greater_than(5, 'timer')
def timer_callback():
    r.send_to_server()
    cyberpi.timer.reset()
        
@cyberpi.event.receive('reset')
def reset_callback():
    r.run()

@cyberpi.event.start
def on_start():
    cyberpi.speaker.set_vol(100)
    if r.initialise():
        r.run()

@cyberpi.event.is_tiltforward
def on_tilt_forward():
    r.record_move()

@cyberpi.event.is_tiltback
def on_tilt_back():
    r.record_move()

@cyberpi.event.is_tiltleft
def on_tilt_left():
    r.record_move()

@cyberpi.event.is_tiltright
def on_tilt_right():
    r.record_move()
