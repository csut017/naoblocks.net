import logging
import json
import socket
from threading import Thread
import time
import traceback

import numpy
import requests
import ssl
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
        self._token = None
        self._verify = True
        self._base_address = None
        self._ast = None

    def start(self, address, pwd=None, verify=True):
        self._verify = verify
        self._base_address = address
        start_address = 'https://' + self._base_address + '/api/v1/version'
        print '[Comms] Checking server version (%s)' % (start_address,)
        try:
            response = requests.get(start_address, timeout=10, verify=self._verify)
            print '[Comms] -> Received response %s' % (response.text,)
        except requests.exceptions.ConnectionError as e:
            print '[Comms] Server not responding: ' + str(e) + '!'
            return False
        except requests.exceptions.Timeout:
            print '[Comms] Connection attempt timed out!'
            return False
        except Exception as e:
            print '[Comms] unknown error: ' + str(e) + '!'
            return False

        start_address = 'https://' + self._base_address + '/api/v1/session'
        print '[Comms] Authenticating (%s)' % (start_address,)
        hostname = socket.gethostname()
        print '[Comms] -> user name %s' % (hostname,)
        start_json = json.dumps({'name': hostname, 'password': pwd, 'role': 'robot'})
        headers = {'Content-type': 'application/json'}
        try:
            req = requests.post(start_address, data=start_json, verify=self._verify, headers=headers)
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
            print '[Comms] Login failed [' + str(req.status_code) + ']!'
            print '[Comms] -> ' + req.text

            start_address = 'https://' + self._base_address + '/api/v1/robots/register'
            print '[Comms] Registering robot %s (%s)' % (hostname, start_address)
            start_json = json.dumps({'machineName': hostname})
            try:
                req = requests.post(start_address, data=start_json, timeout=10, verify=self._verify, headers=headers)
                req.raise_for_status()
                print '[Comms] -> robot registered'
            except Exception as e:
                print '[Comms] registration failed: ' + str(e)
                return False


            return False

        authResp = json.loads(req.text)
        self._token = authResp['output']['token']
        ws_address = 'wss://' + self._base_address + '/api/v1/connections/robot'
        print '[Comms] Connecting to %s' % (ws_address)
        self._ws = websocket.WebSocketApp(ws_address,
                                          on_message=self._message,
                                          on_error=self._error,
                                          on_close=self._close)
        self._ws.on_open = self._open
        connectAgain = True
        while connectAgain:
            if self._connectionCount > 0:
                delayTime = 2 ** self._connectionCount
                print '[Comms] Pausing for %ds' % (delayTime)
                for _ in range(delayTime):
                    time.sleep(1)
            print '[Comms] Connection attempt #%d' % (self._connectionCount)
            if not self._verify:
                self._ws.run_forever(sslopt={"cert_reqs": ssl.CERT_NONE})
            else:
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
        self.send(501, {'state': 'Initialising'})
        try:
            opts = json.loads(data['values']['opts'])
        except KeyError:
            opts = {}
        self._engine.configure(opts)
        self.send(102, {})
        self.send(501, {'state': 'Running'})
        self._engine.run(self._ast)
        self.send(202 if self._engine.is_cancelled else 103, {})
        self.send(501, {'state': 'Waiting'})

    def _message(self, message):
        print '[Comms] Received %s' % (message)
        data = json.loads(message)
        if data['type'] == 22:
            self.send(501, {'state': 'Downloading'})
            program_address = 'https://' + self._base_address + '/api/v1/code/' + data['values']['user'] + '/' + data['values']['program']
            print '[Comms] Downloading program from ' + program_address
            headers = {'Authorization': 'Bearer ' + self._token}
            try:
                req = requests.get(program_address, verify=self._verify, headers=headers)
                req.raise_for_status()
                print '[Comms] Program downloaded'
                print '[Comms] -> ' + req.text

                result = json.loads(req.text)
                self._ast = result['output']['nodes']

                self.send(23, {})
                self.send(501, {'state': 'Prepared'})
            except Exception as e:
                print '[Comms] unknown error: ' + str(e) + '!'
                self.send(24, { 'error': str(e) } )
                self.send(501, {'state': 'Waiting'})

        elif data['type'] == 101:
            thrd = Thread(target=self._execute_code, args=(data,))
            thrd.start()

        elif data['type'] == 201:
            print '[Comms] Cancelling current run'
            self.send(501, {'state': 'Cancelling'})
            self._engine.cancel()

        elif data['type'] == 2:
            print '[Comms] Robot has been authenticated'
            self.send(501, {'state': 'Waiting'})

        else:
            print '[Comms] Unknown or missing message type "%s"' % (
                data['type'])

    def _error(self, error):
        if isinstance(error, KeyboardInterrupt):
            self._serverDisconnected = False
            return
        self._serverDisconnected = True
        print '[Comms] Lost connection: %s' % (error,)

    def _close(self):
        print '[Comms] Closed'

    def _open(self):
        print '[Comms] Opened'
        self._serverDisconnected = False
        self._connectionCount = 0
        self.send(1, { 'token': self._token })
        try:
            self._engine = Engine(self._ws, self._use_robot)
        except:
            traceback.print_exc()
        
    def send(self, msg_type, data):
        print '[Comms] Sending `' + str(msg_type) + '` message'
        msg = json.dumps({
            'type': msg_type,
            'values': data
        })
        print '[Comms] ->  ' + msg
        self._ws.send(msg)
