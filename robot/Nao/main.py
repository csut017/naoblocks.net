#!/usr/bin/env python

import argparse
import atexit
import csv
import sys
import time

import websocket
    
from noRobot import RobotMock
from communications import Communications
import logger

try:
    from naoqi import ALBroker, ALModule, ALProxy
    from robot import Robot
    has_nao = True
except ImportError:
    has_nao = False
    ALModule = object

NaoRemote = None
memory = None
asr = None
myBroker = None


class NaoRemoteModule(ALModule):
    """ Intercepts the Nao events and passes them onto the engine. """

    _right_side = False
    _left_side = False

    def __init__(self, name, comms):
        ALModule.__init__(self, name)
        global memory
        memory = ALProxy("ALMemory")

        self._comms = comms
        self._registerEvents()

    def _registerEvents(self):
        memory.subscribeToEvent("MiddleTactilTouched",
                                "NaoRemote", "onMiddleTouchDetected")
        memory.subscribeToEvent("FrontTactilTouched",
                                "NaoRemote", "onFrontTouchDetected")
        memory.subscribeToEvent("RearTactilTouched",
                                "NaoRemote", "onRearTouchDetected")
        memory.subscribeToEvent("ChestButtonPressed",
                                "NaoRemote", "onChestButtonDetected")
        logger.log('[Module] Subscribed to all events')

    def _unregisterEvents(self):
        memory.unsubscribeToEvent("MiddleTactilTouched", "NaoRemote")
        memory.unsubscribeToEvent("FrontTactilTouched", "NaoRemote")
        memory.unsubscribeToEvent("RearTactilTouched", "NaoRemote")
        memory.unsubscribeToEvent("ChestButtonPressed", "NaoRemote")
        logger.log('[Module] Unsubscribed from all events')

    def onMiddleTouchDetected(self, *args):
        """ This will be called each time the middle head button is pushed. """
        logger.log('[Module] Middle head tactile touched')
        self._unregisterEvents()
        self._comms.trigger('middle')
        self._registerEvents()

    def onChestButtonDetected(self, *args):
        """ This will be called each time the middle head button is pushed. """
        logger.log('[Module] Chest button tactile touched')
        self._unregisterEvents()
        self._comms.trigger('chest')
        self._registerEvents()

    def onFrontTouchDetected(self, *args):
        """ This will be called each time the front head button is pushed. """
        logger.log('[Module] Front head tactile touched')
        self._unregisterEvents()
        self._comms.trigger('front')
        self._registerEvents()

    def onRearTouchDetected(self, *args):
        """ This will be called each time the rear head button is pushed. """
        logger.log('[Module] Rear head tactile touched')
        self._unregisterEvents()
        self._comms.trigger('rear')
        self._registerEvents()


def main():
    """ Main entry point. """
    parser = argparse.ArgumentParser(description='Start Nao-remote client.')
    parser.add_argument('--server', help='The address of the Nao-remote server')
    parser.add_argument('--port', help='The address of the Nao-remote server')
    parser.add_argument(
        '--password', help='The password to connect to the Nao-remote server')
    parser.add_argument(
        '--pip', help='Parent broker port: the IP address or your robot', default='127.0.0.1')
    parser.add_argument(
        '--pport', help='Parent broker port: the port NAOqi is listening to', default=9559)
    parser.add_argument(
        '--test', help='Test mode: will not attempt to connect to the robot', action='store_true')
    parser.add_argument(
        '--ignoreSSL', help='Ignores any SSL errors - only recommended for test environments', action='store_true')
    parser.add_argument(
        '--reconnect', help='The number of reconnect attempts to make if a connection is lost', default=10)
    args = parser.parse_args()

    server = args.server
    pwd = args.password
    if (args.port is not None) and (args.port != ''):
        server = server + ':' + args.port

    args.reconnect = int(args.reconnect)
    logger.log('[Main] Starting communications')
    logger.log('[Main] Environment')
    logger.log('[Main] -- Test robot              : %r', args.test)
    logger.log('[Main] -- Number of reconnections : %r', args.reconnect)
    comms = Communications(not args.test, args.reconnect)
    if not args.test:
        if has_nao:
            myBroker = ALBroker("myBroker", "0.0.0.0", 0, args.pip, args.pport)
            global NaoRemote
            NaoRemote = NaoRemoteModule("NaoRemote", comms)
        else:
            logger.log('[Main] !!NaoQI not installed!!')
            return

    verifySSL = not args.ignoreSSL
    if not verifySSL:
        logger.log('[Main] WARNING: ignoring SSL errors')

    connected = False
    if not args.server is None:
        logger.log('[Main] Connecting to %s', server)
        comms.start(server, pwd, verifySSL)
        connected = True

    if not connected:
        logger.log('[Main] Attempting to connect using connect.txt')
        with open('connect.txt', 'r') as conn_file:
            rdr = csv.reader(conn_file)
            for row in rdr:
                if not connected:
                    server = row[0]
                    pwd = row[1]
                    secure = True if len(row) < 3 else row[2] != 'no'
                    logger.log('[Main] Connecting to %s %s', server, ('' if secure else ' [not secure]'))
                    if comms.start(server, pwd, verifySSL, secure):
                        connected = True

    if not connected:
        logger.log('[Main] Unable to connect')
        robot = RobotMock()
        if not args.test and has_nao:
            robot = Robot('127.0.0.1')
        robot.say('I cannot connect to a server')


def shutdown():
    logger.log('[Main] Shutting down')

if __name__ == "__main__":
    atexit.register(shutdown)
    main()
