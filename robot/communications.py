import logging
import json
import socket
from threading import Thread
import time
import traceback

import numpy
import requests
import websocket

from engine import Engine

logging.basicConfig()

class Communications(object):
    '''The communications interface. '''

    _engine = None
    _ws = None

    def __init__(self,  use_robot=True, reconnectAttempts = None):
        ''' Initialises the communications. '''
        self._use_robot = use_robot
        self._reconnect = reconnectAttempts
        self._serverDisconnected = False
        self._connectionCount = 0

    def start(self, address, pwd=None):
        start_address = 'https://' + address + '/api/v1/connections/robot'
        print '[Comms] Authenticating to %s' % (start_address)
        hostname = socket.gethostname()
        start_json = json.dumps({'name': hostname, 'password': pwd})
        try:
            req = requests.post(start_address, data=start_json, timeout=10)
        except requests.exceptions.ConnectionError:
            print '[Comms] Server not responding!'
            return False
        except requests.exceptions.Timeout:
            print '[Comms] Connection attempt timed out!'
            return False
        except Exception as e:
            print '[Comms] unknown error: ' + str(e) + '!'
            return False

        if req.status_code != 200:
            print '[Comms] Login failed!'
            return False

        token = requests.utils.dict_from_cookiejar(req.cookies)[
            'session-security']
        ws_address = 'ws://' + address + '/api/socket'
        print '[Comms] Connecting to %s' % (ws_address)
        self._ws = websocket.WebSocketApp(ws_address,
                                          on_message=self._message,
                                          on_error=self._error,
                                          on_close=self._close,
                                          cookie='session-security=' + token)
        self._ws.on_open = self._open
        connectAgain = True
        while connectAgain:
            if self._connectionCount > 0:
                delayTime = 2 ** self._connectionCount
                print '[Comms] Pausing for %ds' % (delayTime)
                for _ in range(delayTime):
                    time.sleep(1)
            print '[Comms] Connection attempt #%d' % (self._connectionCount)
            self._ws.run_forever()
            if self._serverDisconnected:
                self._connectionCount += 1
                connectAgain = self._connectionCount <= self._reconnect
            else:
                connectAgain = False
        return True

    def trigger(self, block_name, value=None):
        ''' Triggers a block in the engine. '''
        self._engine.trigger(block_name, value)

    def _execute_code(self, data):
        print '[Comms] Running code'
        self.send('state', {'state': 'Initialising'})
        self._engine.configure(data['data']['opts'])
        self.send('state', {'state': 'Running'})
        self._engine.run(data['data']['ast'])
        self.send('completed', {
            'cancelled': self._engine.is_cancelled
        })
        self.send('state', {'state': 'Waiting'})

    def _message(self, message):
        print '[Comms] Received %s' % (message)
        data = json.loads(message)
        if data['type'] == 'code':
            thrd = Thread(target=self._execute_code, args=(data,))
            thrd.start()
        elif data['type'] == 'cancel':
            print '[Comms] Cancelling current run'
            self.send('state', {'state': 'Cancelling'})
            self._engine.cancel()
        else:
            print '[Comms] Unknown or missing message type "%s"' % (
                data['type'])

    def _error(self, error):
        if isinstance(error, KeyboardInterrupt):
            self._serverDisconnected = False
            return
        try:
            errno = error.errno
        except KeyError:
            errno = 0
        if errno == 10054:
            self._serverDisconnected = True
            print '[Comms] Lost connection to server'
        elif errno == 10061:
            print '[Comms] Unable to connect to server'
        else:
            print '[Comms] Unknown error: %s' % (error)

    def _close(self):
        print '[Comms] Closed'

    def _open(self):
        print '[Comms] Opened'
        self._serverDisconnected = False
        self._connectionCount = 0
        hostname = socket.gethostname()
        msg = json.dumps({
            'type': 'register',
            'data': {
                'type': 'robot',
                'robot': 'nao',
                'name': hostname
            }
        })
        self._ws.send(msg)
        try:
            self._engine = Engine(self._ws, self._use_robot)
        except:
            traceback.print_exc()
        
    def send(self, msg_type, data):
        print '[Comms] Sending `' + msg_type + '` message'
        msg = json.dumps({
            'type': msg_type,
            'data': data
        })
        self._ws.send(msg)
