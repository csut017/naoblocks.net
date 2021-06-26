import json
import logging
import requests
import threading
import websocket

from common import ClassLogger
from enum import IntEnum
from rx.subject import Subject

logger = logging.getLogger(__name__)

class Connection(object):

    def __init__(self, url_base) -> None:
        super().__init__()
        self.url_base = url_base
        if not self.url_base.endswith('/'):
            self.url_base + '/'
        self.token = None
        self._logger = ClassLogger(logger, 'Connection')

    def login(self, username, password, role='Student'):
        url = '{}v1/session'.format(self.url_base)
        self._logger.info('Logging in using {}'.format(url))
        request = {
            'name': username,
            'password': password,
            'role': role
        }
        response = requests.post(url, json=request)
        response.raise_for_status()
        data = response.json()        
        if data['successful']:
            self._logger.info('Login successful')
            self.token = data['output']['token']
            return True
        else:
            self._logger.info('Login failed')
            return False

    def compile(self, code, store=True):
        if self.token is None:
            raise Exception('Not connected to server')

        url = '{}v1/code/compile'.format(self.url_base)
        self._logger.info('Compiling code using {}'.format(url))
        self._logger.debug(code.replace('\n', '\\n').replace('\r', '\\r'))
        request = {
            'code': code,
            'store': store
        }
        response = requests.post(url, json=request, headers=self.generate_headers())
        response.raise_for_status()
        data = response.json()
        if data['successful']:
            self._logger.info('Code compiled')
            return True, data['output']
        else:
            self._logger.info('Code failed to compile')
            return False, data['output']

    def generate_headers(self):
        headers = {
            'Authorization': 'Bearer {}'.format(self.token)
        }
        return headers

class ClientMessageType(IntEnum):
    AUTHENTICATE = 1                  # Send the client credentials to the server
    AUTHENTICATED = 2                 # Credentials are valid
    REQUEST_ROBOT = 11                # Request a robot to run a program on
    ROBOT_ALLOCATED = 12              # Allocate a robot to the client
    NO_ROBOT_AVAILABLE = 13           # There are no clients available
    TRANSFER_PROGRAM = 20             # Request the server to inform the robot to download a program
    PROGRAM_TRANSFERRED = 21          # Reply from the server when the robot has finished downloading
    DOWNLOAD_PROGRAM = 22             # Request the robot to download a program
    PROGRAM_DOWNLOADED = 23           # The robot has finished downloading the program
    UNABLE_TO_DOWNLOAD_PROGRAM = 24   # The program cannot be downloaded to the robot
    START_PROGRAM = 101               # Start execution of a program
    PROGRAM_STARTED = 102             # Program execution has started
    PROGRAM_FINISHED = 103            # Program execution has finished
    STOP_PROGRAM = 201                # Request cancellation of a program
    PROGRAM_STOPPED = 202             # Program has been cancelled
    ROBOT_STATE_UPDATE = 501          # An update from the robot about its state
    ROBOT_DEBUG_MESSAGE = 502         # A debug message from the robot (normally a step has started)
    ROBOT_ERROR = 503                 # An error that occurred during execution of a program
    ERROR = 1000                      # A general error (e.g. message type not recognised)
    NOT_AUTHENTICATED = 1001          # The client has not been authenticated
    FORBIDDEN = 1002                  # The client is not allowed to call the functionality
    START_MONITORING = 1100           # Start monitoring all client changes
    STOP_MONITORING = 1101            # Stop monitoring all client changes
    CLIENT_ADDED = 1102               # A new client has connected to the system
    CLIENT_REMOVED = 1103             # An existing client has disconnected
    ALERT_REQUEST = 1200              # Requests all current notifications on the robot
    ALERT_BROADCAST = 1201            # An alert is being broadcast to all listeners

    CLOSED = 5001                     # The connection has been closed 

class Exector(object):

    def __init__(self, connection: Connection, url_base) -> None:
        super().__init__()
        self._connection = connection
        self._logger = ClassLogger(logger, 'Executor')
        self._websocket = None
        self._subject = None
        self._url_base = url_base

        self._assigned_robot = None
        self._program = None

        self._message_processors = {
            ClientMessageType.AUTHENTICATED: self._on_authenticated,
            ClientMessageType.ROBOT_ALLOCATED: self._on_robot_allocated,
            ClientMessageType.NO_ROBOT_AVAILABLE: self._on_no_robot,
            ClientMessageType.PROGRAM_TRANSFERRED: self._on_program_transferred,
            ClientMessageType.UNABLE_TO_DOWNLOAD_PROGRAM: self._on_program_transfer_failed,
            ClientMessageType.PROGRAM_STARTED: self._on_program_started,
            ClientMessageType.PROGRAM_FINISHED: self._on_program_ended('finished'),
            ClientMessageType.PROGRAM_FINISHED: self._on_program_ended('stopped'),
            ClientMessageType.ROBOT_STATE_UPDATE: self._on_state_update,
            ClientMessageType.ROBOT_DEBUG_MESSAGE: self._on_debug_message,
            ClientMessageType.ROBOT_ERROR: self._on_error('robot'),
            ClientMessageType.ERROR: self._on_error('general'),
        }

    def execute(self, code):
        self._logger.info('Compiling code')
        success, prog = self._connection.compile(code)
        if not success:
            self._logger.warning('Compilation failed')
            return None

        self._program = prog['programId']
        self._subject = Subject()
        self._thread = threading.Thread(target=self._message_processor, daemon=True)
        self._thread.start()

        return self._subject

    def wait(self) -> None:
        self._thread.join()

    def _message_processor(self):
        self._logger.debug('Starting message processing loop')

        url = '{}v1/connections/user'.format(self._url_base)
        self._logger.info('Opening websocket to {}'.format(url))
        self._websocket = websocket.WebSocketApp(url, 
            on_open=self._on_open, 
            on_message=self._on_message, 
            on_close=self._on_close)
        self._websocket.run_forever()

    def _on_open(self, socket):
        self._logger.info('Socket open')

        self._logger.info('Authenticating')
        self.send_message(self.generate_message(ClientMessageType.AUTHENTICATE, {'token': self._connection.token}))

    def _on_message(self, socket, message):
        self._logger.debug('Received message {}'.format(message))
        data = json.loads(message)
        type = int(data['type'])
        processor = None
        try:
            processor = self._message_processors[type]
        except KeyError:
            self._logger.warning('Unable to find processor for {}'.format(type))

        if processor is not None:
            processor(data)        

    def _on_close(self, socket, close_status_code, close_msg):
        self._logger.info('Socket closed')
        self.closeConnection()

    def send_message(self, message):
        data = json.dumps(message)
        self._logger.debug('Sending message {}'.format(data))
        self._websocket.send(data)

    def generate_message(self, type, values = None, original = None):
        msg = {
            'type': int(type)
        }
        if not values is None:
            msg['values'] = values

        if not original is None:
            msg['conversationId'] = original['conversationId']

        return msg

    def _on_authenticated(self, data):
        self._logger.info('Authenticated')
        self.send_message(self.generate_message(ClientMessageType.REQUEST_ROBOT, original= data))

    def _on_robot_allocated(self, data):
        self._logger.info('Robot allocated')
        self._assigned_robot = data['values']['robot']
        msg = self.generate_message(ClientMessageType.TRANSFER_PROGRAM, {
            'robot': self._assigned_robot,
            'program': self._program
        }, original= data)
        self.send_message(msg)

    def _on_no_robot(self, data):
        self._logger.info('No robot available')
        self.closeConnection()

    def _on_program_transferred(self, data):
        self._logger.info('Program transferred')
        msg = self.generate_message(ClientMessageType.START_PROGRAM, {
            'robot': self._assigned_robot,
            'program': self._program,
            'opts': json.dumps({})
        }, original= data)
        self.send_message(msg)

    def _on_program_transfer_failed(self, data):
        self._logger.info('Unable to transfer program')
        self.closeConnection()

    def _on_program_started(self, data):
        self._logger.info('Program started')

    def _on_program_ended(self, type):
        def inner(data):
            self._logger.info('Program {}'.format(type))
            self.closeConnection()
        return inner

    def _on_state_update(self, data):
        state = data['values']['state']
        self._logger.info('Robot state changed to {}'.format(state))
        self._subject.on_next((ClientMessageType.ROBOT_STATE_UPDATE, data))

    def _on_debug_message(self, data):
        source = data['values']['sourceID']
        state = data['values']['status']
        func = data['values']['function']
        self._logger.info('Function {} has {}ed [{}]'.format(func, state, source))
        self._subject.on_next((ClientMessageType.ROBOT_STATE_UPDATE, data))

    def _on_error(self, type):
        def inner(data):
            try:
                err = data['values']['error']
            except KeyError:
                err = 'Unknown'
            self._logger.info('A {} error has occurred: {}'.format(type, err))
            self.closeConnection()
        return inner

    def closeConnection(self):
        self._logger.debug('Closing connection')
        self._websocket.close()
        self._subject.on_completed()
