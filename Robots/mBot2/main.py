""" mBot2 client

This client runs directly on an mBot2 client. It handles communicates with the
server and execution of the commands.
"""

import socket

HOST = '192.168.0.5'
PORT = 5010

class message(object):
    def __init__(self, type, seq):
        self.type = type
        self.seq = seq
        self.values = {}

class connection(object):
    def __init__(self, address, port):
        self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._socket.connect((address, port))
        self._seq = 0
        self._data = bytearray(1024);
        self._next_pos = 0
        self._last_pos = 0
        self._pending = b''

    def __del__(self):
        self._socket.close()

    def start_message(self, type):
        self._socket.sendall(type.to_bytes(2, 'little'))
        self._socket.sendall(self._seq.to_bytes(2, 'little'))

    def send_value(self, key, value):
        self._socket.sendall(f'{key}={value}'.encode('utf-8'))

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

            type = int.from_bytes(self._data[0:1], byteorder='little')
            seq = int.from_bytes(self._data[2:3], byteorder='little')
            msg = message(type, seq)
            values = self._data[4:self._next_pos-1].decode('utf-8')
            if len(values) > 0:
                for value in values.split(','):
                    index = value.index('=')
                    if index < 0:
                        msg.values[value] = ''
                    else:
                        msg.values[value[0:index]] = value[index+1:]
            self._last_pos = pos + 1
            return msg

if __name__ == "__main__":
    conn = connection(HOST, PORT)
    conn.send_message(1, {'name': 'me'})
    msg = conn.receive_next_message()
    conn.send_message(102)
    conn.send_message(103)
