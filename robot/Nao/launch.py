import base64
import hashlib
import os
import sys

import requests

BUF_SIZE = 65536

def log(message, *args):
    print('[LAUNCH] ' + message % args)

def generateFileHash(filepath):
    sha = hashlib.sha256()
    with open(filepath, 'rb') as f:
        while True:
            data = f.read(BUF_SIZE)
            if not data:
                break
            sha.update(data)
    return base64.b64encode(sha.digest())

def retrieveFileList(server, is_secure, verify = True):
    address = ('https' if is_secure else 'http') + '://' + server + '/api/v1/robots/types/Nao/package.txt'
    log('Downloading file list (%s)', address)
    try:
        response = requests.get(address, timeout=10, verify=verify)
        log('Received response')
        lines = response.text.splitlines(False)
        log('%d files received', len(lines))
        return lines
    except requests.exceptions.ConnectionError as e:
        log('ERROR: Server not responding: ' + str(e) + '!')
        return False
    except requests.exceptions.Timeout:
        log('ERROR: Connection attempt timed out!')
        return False
    except Exception as e:
        log('ERROR: %s', str(e))
        return False

def generateFolder():
    path = os.getcwd()
    target_path = os.path.join(path, 'launch')
    if os.path.exists(target_path):
        log('Folder %s already exists, skipping make', target_path)
        return target_path

    log('Making folder %s', target_path)
    try:
        os.mkdir(target_path)
    except OSError:
        log('ERROR: Unable to make folder %s', target_path)
        return False

    log('Made folder %s', target_path)
    return target_path

def downloadFile(file, target, server, is_secure, prefix = '/api/v1/robots/types/Nao/package/', verify = True):
    address = ('https' if is_secure else 'http') + '://' + server + prefix + file
    log('Downloading file from %s', address)
    try:
        response = requests.get(address, timeout=10, verify=verify)
        log('Received response')
        with open(target, 'wb') as f:
            f.write(response.content)
        log('Successfully downloaded %s', file)
        return True
    except requests.exceptions.ConnectionError as e:
        log('ERROR: Server not responding: ' + str(e) + '!')
        return False
    except requests.exceptions.Timeout:
        log('ERROR: Connection attempt timed out!')
        return False
    except Exception as e:
        log('ERROR: %s', str(e))
        return False

def downloadAllFiles(path, server, is_secure, verify = True):
    files = retrieveFileList(server, is_secure, verify)
    if files == False:
        return False

    count = 0
    for f in files:
        parts = f.split(',')
        file_path = os.path.join(path, parts[0])
        download = False

        if os.path.exists(file_path):
            log('File %s exists, checking hash', file_path)
            hash = generateFileHash(file_path)
            if hash != parts[1]:
                log('Hashes for %s are different', file_path)
                download = True
            else:
                log('Hashes for %s are the same', file_path)
        else:
            log('File %s does not exist', file_path)
            download = True

        if download:
            if not downloadFile(parts[0], file_path, server, is_secure, verify = verify):
                return False
            else:
                count += 1

    log('Downloaded %d file(s)', count)
    return True

def downloadConnections(path, server, is_secure, verify = True):
    connect_file = 'connect.txt'
    file_path = os.path.join(path, connect_file)
    if not downloadFile(connect_file, file_path, server, is_secure, prefix = '/api/v1/system/addresses/', verify = verify):
        return False
    return True

server = 'localhost:5000'
path = generateFolder()
if path != False:
    if downloadAllFiles(path, server, False) and downloadConnections(path, server, False):
        log('Successfully downloaded all required files')
    else:
        log('!! Unable to download all required files !!')