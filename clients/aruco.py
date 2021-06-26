#!/usr/bin/env python

""" Aruco client application

This application provides a tangible-based application using Aruco markers
as the input blocks.
"""

import argparse
import logging
import socket
import threading

import aruco_client
import naoblocks
import requests

from common import ClassLogger, CodeBuilder
from rx import operators as op

logger = logging.getLogger(__name__)

class App(object):
    def __init__(self) -> None:
        self._logger = ClassLogger(logger, 'Scanner')
        self._conn = None
        self._server_name = None
        self._builder = CodeBuilder()
        self._program = None

    def run(self):
        parser = self._init_argparse()
        args = parser.parse_args()
        user = socket.gethostname() if args.user is None else args.user
        self._logger.info('Starting Aruco remote client')
        connected = False
        if not args.server is None:
            self._logger.info('Attempting connection to server using arguments:')
            self._logger.info('--> Server:      {}'.format(args.server))
            self._logger.info('--> User:        {}'.format(user))

            self._conn = naoblocks.Connection('http://{}/api/'.format(args.server))
            try:
                self._logger.info('Attempting to connect')
                self._conn.login(user, args.password)
                self._logger.info('Connected')
                self._server_name = args.server
                connected = True
            except requests.exceptions.HTTPError as ex:
                self._logger.info('Connection failed')
                self._logger.debug(ex)

        if not connected:
            self._logger.info('Scanning for connections using {}'.format(args.connections))

        if connected:
            scanner = aruco_client.Scanner()
            scanner.build_dictionary(
                aruco_client.BlockDefinition(10, 'Turn Left', 'turn_left', 'turn(LEFT, 3)'),
                aruco_client.BlockDefinition(20, 'Turn Right', 'turn_right', 'turn(RIGHT, 3)'),
                aruco_client.BlockDefinition(30, 'Rest', 'rest', 'rest()'),
            )
            scanner.run(show_window=True, show_id=True).pipe(
                op.distinct_until_changed(self._calculate_program_key)
            ).subscribe(
                on_next = self._on_program_received(),
                on_completed = self._on_completed()
            )
            scanner.wait()

    def _calculate_program_key(self, program: list[aruco_client.ScannedBlock]):
        pos = 1
        total = 0
        for block in program:
            total += block.aruco_id * pos
            pos += 1

        return total

    def _init_argparse(self) -> argparse.ArgumentParser:
        parser = argparse.ArgumentParser(
            description='NaoBlocks Remote client using Aruco code recognition.'
        )
        parser.add_argument("--connections", help="The connections file to use.", default='connect.txt')
        parser.add_argument("-s", "--server", help="The address of the NaoBlocks server.")
        parser.add_argument("-u", "--user", help="The user for connecting to the server. If not set, it will default to the machine name.")
        parser.add_argument("-p", "--password", help="The password for connecting to the server.")
        return parser

    def _on_program_received(self):
        def handler(blocks):
            if len(blocks) == 0:
                self._program = None
                return

            self._logger.info('Received program')
            self._program = self._builder.build(blocks, include_ids=True)
            self._logger.debug(self._program.replace('\n', '\\n'))

        return handler

    def _on_completed(self):
        def handler():
            if self._program is None:
                return

            self._logger.info('Starting program')
            base_url = 'ws://{}/api/'.format(self._server_name)
            executor = naoblocks.Exector(self._conn, base_url)
            executor.execute(self._program).subscribe(
                on_next=self._on_executor_message, 
                on_completed=self._on_executor_completed
            )
            executor.wait()

        return handler

    def _on_executor_message(self, message):
        self._logger.info('Received executor message')

    def _on_executor_completed(self):
        self._logger.info('Executor has completed')

if __name__ == "__main__":
    logging.basicConfig(level=logging.DEBUG)
    app = App()
    app.run()
