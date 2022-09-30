""" mBot2 test client

This client simulates running an mBot2 client.

Unfortunately, due to how mBlock deploys, this is a copy-paste job.
"""

import requests
import socket

### Configuration settings ###

HOST = '192.168.0.5'
SERVER = 'http://' + HOST + ':5000'
PORT = 5010

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

    def set_state(self, state):
        self.send_message(501, {'state': state})
        self._conversation = 0

### Test code ###

if __name__ == "__main__":
    conn = Connection(HOST, PORT)
    hostname = socket.gethostname()
    try:
        print('Logging in...')
        conn.login(hostname, 'one')
        print('...done')
    except LoginException:
        try:
            print('...registering...')
            conn.register(hostname)
            print('...done')
        except Exception:
            print('...failed')

    while True:
        msg = conn.receive_next_message()
        if msg.type == 2:       # Authenticate
            print('Authenticated')
            conn.send_message(501, {'state': 'Waiting'})

        elif msg.type == 22:    # Download program
            conn.send_message(501, {'state': 'Downloading'})
            print('Downloading code...')
            try:
                ast = conn.download_code(msg)
                print('...done')
            except:
                print('...failed')

        elif msg.type == 101:   # Start execution
            pass

        elif msg.type == 201:   # Cancel execution
            pass
