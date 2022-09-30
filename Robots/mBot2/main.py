""" mBot2 client

This client runs directly on an mBot2 client. It handles communicates with the
server and execution of the commands.

This file needs to be deployed via mBlock. All the code must be in a single file,
otherwise mBlock won't compile it correctly.
"""

import cyberpi
import socket
import time

import urequests as requests

### Configuration settings ###

HOST = '192.168.0.5'
SERVER = 'http://' + HOST + ':5000'
PORT = 5010
WIFI_SSID= 'robotics'
WIFI_PASSWORD= 'letmein1'
NAME = cyberpi.get_name()

### Common code ###

class Message(object):
    def __init__(self, type, seq):
        self.type = type
        self.seq = seq
        self.values = {}

class Connection(object):
    def __init__(self, address, port):
        self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._socket.connect((address, port))
        self._seq = 0
        self._data = bytearray(1024)
        self._next_pos = 0
        self._last_pos = 0
        self._pending = b''

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

### mBot2 code ###

class robot():
    def __init__(self):
        self.lines = 100
        self.is_connected = False
        self.conn = None
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

    def initialise_wifi(self):
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

        self.display('Host connect')
        try:
            self.conn = Connection(HOST, PORT)
        except:
            self.display('...failed')
            return False

        self.display('...connected')
        self.is_connected = True
        return True

    def run(self):
        while True:
            msg = self.conn.receive_next_message()
            if msg is None:
                continue

            self.display('Received ' + str(msg.type))

r = robot()

@cyberpi.event.start
def on_start():
    cyberpi.speaker.set_vol(100)
    if r.initialise_wifi():
        r.run()
