""" mBot2 client

This client runs directly on an mBot2 client. It handles communicates with the
server and execution of the commands.
"""

import socket

HOST = '127.0.0.1'
PORT = 5010

class connection(object):
    def __init__(self, address, port):
        self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._socket.connect((address, port))
        self._seq = 0

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

if __name__ == "__main__":
    conn = connection(HOST, PORT)
    conn.send_message(1, {'name': 'me'})
    conn.send_message(102)
    conn.send_message(103)
