#!/usr/bin/env python

""" Remote robot client

This client acts as a middleman between the server and a robot or drone. It will
handle all the server communications and translate them to the robot or drone
instructions.

This client is typically used for robots or drones that do not provide sufficient
onboard capacity to run a client. Instead, this client will run on a "server" device
and connect directly to the robot or drone. 
"""

import argparse
import atexit
import logging

logger = logging.getLogger(__name__)

def main():
    """ Main entry point. """
    parser = argparse.ArgumentParser(description='Start remote client.')
    parser.add_argument('--server', help='The address of the NaoBlocks server')
    parser.add_argument('--port', help='The address of the NaoBlocks server')
    parser.add_argument(
        '--password', help='The password to connect to the NaoBlocks server')
    parser.add_argument(
        '--ignoreSSL', help='Ignores any SSL errors - only recommended for test environments', action='store_true')
    parser.add_argument(
        '--reconnect', help='The number of reconnect attempts to make if a connection is lost', default=25)
    args = parser.parse_args()

    server = args.server
    pwd = args.password
    if (args.port is not None) and (args.port != ''):
        server = server + ':' + args.port

    args.reconnect = int(args.reconnect)
    logger.info('[Main] Starting communications')
    logger.info('[Main] Environment')
    logger.info('[Main] -- Number of reconnections : %r', args.reconnect)

    verifySSL = not args.ignoreSSL
    if not verifySSL:
        logger.warning('[Main] WARNING: ignoring SSL errors')

    connected = False
    logger.info('TODO: implement everything')


def shutdown():
    print('[Main] Shutting down')

if __name__ == "__main__":
    atexit.register(shutdown)
    main()
