#!/usr/bin/env python

""" Aruco client application

This application provides a tangible-based application using Aruco markers
as the input blocks.
"""

import argparse
import logging
import socket

import aruco_client
import naoblocks
import requests

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

def init_argparse() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description='NaoBlocks Remote client using Aruco code recognition.'
    )
    parser.add_argument("--connections", help="The connections file to use.", default='connect.txt')
    parser.add_argument("-s", "--server", help="The address of the NaoBlocks server.")
    parser.add_argument("-u", "--user", help="The user for connecting to the server. If not set, it will default to the machine name.")
    parser.add_argument("-p", "--password", help="The password for connecting to the server.")
    return parser

def on_program_received(scanner: aruco_client.Scanner):
    def on_received(program):
        logger.info('Received program')
        scanner.stop()

    return on_received

def main():
    parser = init_argparse()
    args = parser.parse_args()
    user = socket.gethostname() if args.user is None else args.user
    logger.info('Starting Aruco remote client')
    connected = False
    if not args.server is None:
        logger.info('Attempting connection to server using arguments:')
        logger.info('--> Server:      {}'.format(args.server))
        logger.info('--> User:        {}'.format(user))

        conn = naoblocks.Connection('http://{}/api/'.format(args.server))
        try:
            logger.info('Attempting to connect')
            conn.login(user, args.password)
            logger.info('Connected')
            connected = True
        except requests.exceptions.HTTPError as ex:
            logger.info('Connection failed')
            logger.debug(ex)

    if not connected:
        logger.info('Scanning for connections using {}'.format(args.connections))

    if connected:
        scanner = aruco_client.Scanner()
        scanner.build_dictionary(
            aruco_client.BlockDefinition(10, 'Turn Left', 'turn_left', 'turn(LEFT, 3'),
            aruco_client.BlockDefinition(20, 'Turn Right', 'turn_right', 'turn(RIGHT, 3'),
            aruco_client.BlockDefinition(30, 'Rest', 'rest', 'rest()'),
        )
        scanner.run(show_window=True).subscribe(
            on_next = on_program_received(scanner)
        )
        scanner.wait()

if __name__ == "__main__":
    main()
