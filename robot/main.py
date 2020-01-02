#!/usr/bin/env python

import argparse
import atexit
import csv
import sys
import time

import websocket
    
from noRobot import RobotMock
from communications import Communications

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
        print '[Module] Subscribed to all events'

    def _unregisterEvents(self):
        memory.unsubscribeToEvent("MiddleTactilTouched", "NaoRemote")
        memory.unsubscribeToEvent("FrontTactilTouched", "NaoRemote")
        memory.unsubscribeToEvent("RearTactilTouched", "NaoRemote")
        memory.unsubscribeToEvent("ChestButtonPressed", "NaoRemote")
        print '[Module] Unsubscribed from all events'

    def onMiddleTouchDetected(self, *args):
        """ This will be called each time the middle head button is pushed. """
        print '[Module] Middle head tactile touched'
        self._unregisterEvents()
        self._comms.trigger('middle')
        self._registerEvents()

    def onChestButtonDetected(self, *args):
        """ This will be called each time the middle head button is pushed. """
        print '[Module] Chest button tactile touched'
        self._unregisterEvents()
        self._comms.trigger('chest')
        self._registerEvents()

    def onFrontTouchDetected(self, *args):
        """ This will be called each time the front head button is pushed. """
        print '[Module] Front head tactile touched'
        self._unregisterEvents()
        self._comms.trigger('front')
        self._registerEvents()

    def onRearTouchDetected(self, *args):
        """ This will be called each time the rear head button is pushed. """
        print '[Module] Rear head tactile touched'
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
        '--reconnect', help='The number of reconnect attempts to make if a connection is lost', default=10)
    args = parser.parse_args()

    server = args.server
    pwd = args.password
    if (args.port is not None) and (args.port != ''):
        server = server + ':' + args.port

    args.reconnect = int(args.reconnect)
    print '[Main] Starting communications'
    print '[Main] Environment'
    print '[Main] -- Test robot              : %r' % (args.test)
    print '[Main] -- Number of reconnections : %r' % (args.reconnect)
    comms = Communications(not args.test, args.reconnect)
    if not args.test:
        if has_nao:
            myBroker = ALBroker("myBroker", "0.0.0.0", 0, args.pip, args.pport)
            global NaoRemote
            NaoRemote = NaoRemoteModule("NaoRemote", comms)
        else:
            print '[Main] !!NaoQI not installed!!'
            return

    connected = False
    if not args.server is None:
        print '[Main] Connecting to ' + server
        comms.start(server, pwd)
        connected = True

    if not connected:
        print '[Main] Attempting to connect using connect.txt'
        with open('connect.txt', 'rb') as conn_file:
            rdr = csv.reader(conn_file)
            for row in rdr:
                if not connected:
                    server = row[0]
                    pwd = row[1]
                    print '[Main] Connecting to ' + server
                    if comms.start(server, pwd):
                        connected = True

    if not connected:
        print '[Main] Unable to connect'
        robot = RobotMock()
        if not args.test and has_nao:
            robot = Robot('127.0.0.1')
        robot.say('I cannot connect to a server')


def shutdown():
    print '[Main] Shutting down'

if __name__ == "__main__":
    atexit.register(shutdown)
    main()
