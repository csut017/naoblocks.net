import argparse
import base64
import csv
import hashlib
import logger
import os
import requests
import subprocess
import sys

BUFFER_SIZE = 65536

def generateFileHash(filepath):
    sha = hashlib.sha256()
    with open(filepath, 'rb') as f:
        while True:
            data = f.read(BUFFER_SIZE)
            if not data:
                break
            sha.update(data)
    return base64.b64encode(sha.digest())

def download_file(filename, base_address, verifySSL, hash):
    full_address = base_address + 'package/' + filename
    logger.log('[AutoLoad] -> Downloading %s from %s', filename, full_address)
    headers = {
        "ETag": hash
    }
    response = requests.get(full_address, verify=verifySSL, headers=headers)
    response.raise_for_status()
    with open(filename, "wb") as f:
        f.write(response.content)
    logger.log('[AutoLoad] -> %s downloaded', filename)

def check_for_updates(server, verifySSL, secure):
    base_address = ('https' if secure else 'http') + '://' + server + '/api/v1/robots/types/nao/'
    list_address = base_address + 'package.txt'
    logger.log('[AutoLoad] Checking for package at (%s)', list_address)
    try:
        response = requests.get(list_address, timeout=10, verify=verifySSL)
        response.raise_for_status()

        logger.log('[AutoLoad] -> Received response')
        response_text = response.text.strip()
        logger.log(response_text)
        for line in response_text.split('\n'):
            cleaned = line.strip()
            if cleaned == '':
                continue

            parts = cleaned.split(',')
            filename = parts[0]
            hash = parts[1]
            logger.log('[AutoLoad] -> Checking file %s', filename)

            local_hash = generateFileHash(filename)
            is_match = local_hash == hash
            logger.log('[AutoLoad] -> %s:local=%s,remote=%s', ('Match' if is_match else 'Different'), local_hash, hash)
            if not is_match:
                download_file(filename, base_address, verifySSL, local_hash)
                
        return True
    except requests.exceptions.ConnectionError as e:
        logger.log('[AutoLoad] Server not responding: ' + str(e) + '!')
    except requests.exceptions.Timeout:
        logger.log('[AutoLoad] Connection attempt timed out!')
    except Exception as e:
        logger.log('[AutoLoad] unknown error: ' + str(e) + '!')
    return False

def run():
    args = sys.argv[1:]
    args.insert(0, 'main.py')
    args.insert(0, sys.executable)
    logger.log('[AutoLoad] Starting main')
    process = subprocess.Popen(args)
    try:
        process.wait()
    except KeyboardInterrupt:
        pass

def parse_args():
    parser = argparse.ArgumentParser(description='Nao robot autoloader.')
    parser.add_argument('--server', help='The address of the NaoBlocks server')
    parser.add_argument('--port', help='The address of the NaoBlocks server')
    parser.add_argument('--name', help='An alternate name to use as the robot name', default=None)
    parser.add_argument(
        '--password', help='The password to connect to the NaoBlocks server')
    parser.add_argument(
        '--pip', help='Parent broker port: the IP address or your robot', default='127.0.0.1')
    parser.add_argument(
        '--pport', help='Parent broker port: the port NAOqi is listening to', default=9559)
    parser.add_argument(
        '--test', help='Test mode: will not attempt to connect to the robot', action='store_true')
    parser.add_argument(
        '--ignoreSSL', help='Ignores any SSL errors - only recommended for test environments', action='store_true')
    parser.add_argument(
        '--useHttp', help='Uses plain HTTP for the server connection', action='store_true')
    parser.add_argument(
        '--updateOnly', help='Attempts to update the files without running the client', action='store_true')
    parser.add_argument(
        '--reconnect', help='The number of reconnect attempts to make if a connection is lost', default=25)
    return parser.parse_args()

def main():
    args = parse_args()

    server = args.server
    if (args.port is not None) and (args.port != ''):
        server = server + ':' + args.port

    verifySSL = not args.ignoreSSL
    if not verifySSL:
        logger.log('[AutoLoad] WARNING: ignoring SSL errors')

    connected = False
    if not args.server is None:
        logger.log('[AutoLoad] Connecting to %s', server)
        connected = check_for_updates(server, verifySSL, not args.useHttp)

    if not connected:
        logger.log('[AutoLoad] Attempting to connect using connect.txt')
        try:
            with open('connect.txt', 'r') as conn_file:
                rdr = csv.reader(conn_file)
                for row in rdr:
                    if not connected:
                        server = row[0]
                        secure = True if len(row) < 3 else row[2] != 'no'
                        logger.log('[AutoLoad] Connecting to %s %s', server, ('' if secure else ' [not secure]'))
                        connected = check_for_updates(server, verifySSL, secure)
        except IOError:
            logger.log('[AutoLoad] Cannot find connect.txt')

    if not args.updateOnly:
        run()

if __name__ == "__main__":
    main()
