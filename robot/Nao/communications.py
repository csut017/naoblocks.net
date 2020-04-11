import logging
import json
import socket
from threading import Thread
import time
import traceback

import requests
import ssl
import websocket

from engine import Engine
import logger

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
        self._secure = True
        self._conversationId = 0
        self._closing = False

    def start(self, address, pwd=None, verify=True, secure=True):
        self._verify = verify
        self._secure = secure
        self._base_address = address
        start_address = ('https' if self._secure else 'http') + '://' + self._base_address + '/api/v1/version'
        logger.log('[Comms] Checking server version (%s)', start_address)
        try:
            response = requests.get(start_address, timeout=10, verify=self._verify)
            logger.log('[Comms] -> Received response %s', response.text)
        except requests.exceptions.ConnectionError as e:
            logger.log('[Comms] Server not responding: ' + str(e) + '!')
            return False
        except requests.exceptions.Timeout:
            logger.log('[Comms] Connection attempt timed out!')
            return False
        except Exception as e:
            logger.log('[Comms] unknown error: ' + str(e) + '!')
            return False

        start_address = ('https' if self._secure else 'http') + '://' + self._base_address + '/api/v1/session'
        logger.log('[Comms] Authenticating (%s)', start_address)
        hostname = socket.gethostname()
        logger.log('[Comms] -> user name %s', hostname)
        start_json = json.dumps({'name': hostname, 'password': pwd, 'role': 'robot'})
        headers = {'Content-type': 'application/json'}
        try:
            req = requests.post(start_address, data=start_json, verify=self._verify, headers=headers)
        except requests.exceptions.ConnectionError:
            logger.log('[Comms] Server not responding!')
            return False
        except requests.exceptions.Timeout:
            logger.log('[Comms] Connection attempt timed out!')
            return False
        except Exception as e:
            logger.log('[Comms] unknown error: ' + str(e) + '!')
            return False

        if req.status_code != 200:
            logger.log('[Comms] Login failed [' + str(req.status_code) + ']!')
            logger.log('[Comms] -> ' + req.text)

            start_address = ('https' if self._secure else 'http') + '://' + self._base_address + '/api/v1/robots/register'
            logger.log('[Comms] Registering robot %s (%s)', hostname, start_address)
            start_json = json.dumps({'machineName': hostname})
            try:
                req = requests.post(start_address, data=start_json, timeout=1, verify=self._verify, headers=headers)
                req.raise_for_status()
                logger.log('[Comms] -> robot registered')
            except Exception as e:
                logger.log('[Comms] registration failed: ' + str(e))
                return False


            return False

        authResp = json.loads(req.text)
        self._token = authResp['output']['token']
        ws_address = ('wss' if self._secure else 'ws') + '://' + self._base_address + '/api/v1/connections/robot'
        logger.log('[Comms] Connecting to %s', ws_address)
        self._ws = websocket.WebSocketApp(ws_address,
                                          on_message=self._message,
                                          on_error=self._error,
                                          on_close=self._close)
        self._ws.on_open = self._open
        connectAgain = True
        while connectAgain:
            if self._closing:
                connectAgain = False
                continue

            # Attempt to reconnect
            if self._connectionCount > 0:
                delayTime = 2 ** self._connectionCount
                logger.log('[Comms] Pausing for %ds', delayTime)
                for _ in range(delayTime):
                    time.sleep(1)
            logger.log('[Comms] Connection attempt #%d', self._connectionCount)
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
        logger.log('[Comms] Running code')
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
        self._conversationId = 0

    def _message(self, *args):
        message = args[-1]
        logger.log('[Comms] Received %s', message)
        data = json.loads(message)
        self._conversationId = data['conversationId']
        if data['type'] == 22:
            self.send(501, {'state': 'Downloading'})
            program_address = ('https' if self._secure else 'http') + '://' + self._base_address + '/api/v1/code/' + data['values']['user'] + '/' + data['values']['program']
            logger.log('[Comms] Downloading program from %s', program_address)
            headers = {'Authorization': 'Bearer ' + self._token}
            try:
                req = requests.get(program_address, verify=self._verify, headers=headers)
                req.raise_for_status()
                logger.log('[Comms] Program downloaded')
                logger.log('[Comms] -> %s', req.text)

                result = json.loads(req.text)
                self._ast = result['output']['nodes']

                self.send(23, {})
                self.send(501, {'state': 'Prepared'})
            except Exception as e:
                logger.log('[Comms] unknown error: %s!', e)
                self.send(24, { 'error': str(e) } )
                self.send(501, {'state': 'Waiting'})
                self._conversationId = 0

        elif data['type'] == 101:
            thrd = Thread(target=self._execute_code, args=(data,))
            thrd.start()

        elif data['type'] == 201:
            logger.log('[Comms] Cancelling current run')
            self.send(501, {'state': 'Cancelling'})
            self._engine.cancel()

        elif data['type'] == 2:
            logger.log('[Comms] Robot has been authenticated')
            self.send(501, {'state': 'Waiting'})
            self._conversationId = 0

        else:
            logger.log('[Comms] Unknown or missing message type "%s"', data['type'])

    def _error(self, *args):
        error = args[-1]
        if isinstance(error, KeyboardInterrupt):
            self._serverDisconnected = False
            return
        self._serverDisconnected = True
        logger.log('[Comms] Lost connection: %s', error)

    def _close(self, *args):
        logger.log('[Comms] Closed')

    def _open(self, *args):
        logger.log('[Comms] Opened')
        self._serverDisconnected = False
        self._connectionCount = 0
        self.send(1, { 'token': self._token })
        try:
            self._engine = Engine(self, self._use_robot)
        except:
            traceback.print_exc()
        
    def send(self, msg_type, data):
        logger.log('[Comms] Sending message of type %s', str(msg_type))
        msg = json.dumps({
            'type': msg_type,
            'conversationId': self._conversationId,
            'values': data
        })
        logger.log('[Comms] -> %s', msg)
        self._ws.send(msg)

    def close(self):
        logger.log('[Comms] Closing down communications')
        self._closing = True
