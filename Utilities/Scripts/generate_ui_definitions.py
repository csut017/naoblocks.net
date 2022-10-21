"""
UI Definition Generator
=======================

This utility will split a unified UI definition into the definitions for Angular and Tangible interfaces
(maybe more in future.)

Having a single unified definition means we can define blocks for all interfaces at once, allowing better
cross-interface comparisions.

The first argument is the path to the unified definition.
The second argument is the path to the output folder.

There are the following optional arguments:
* --server : automatically upload to a NaoBlocks server. This argument must contain the address of the server (server and protocol only.)
* --user   : a username and password pair for logging onto the server. Seperate the two components with a colon.

Example command line (basic):
    py -3 generate_ui_definitions.py ../../Data/unified-ballet.json ../../Data/From_Unified

Example command line (basic):
    py -3 generate_ui_definitions.py ../../Data/unified-ballet.json ../../Data/From_Unified --server http://192.168.0.5:5000 --user user:password

"""

import argparse
import json
import os
import requests

def parse_args():
    parser = argparse.ArgumentParser(description='Unified Interface Definition Splitter.')
    parser.add_argument('input', help='The input definition.')
    parser.add_argument('output', help='The output folder.')
    parser.add_argument('-s', '--server', help='The NaoBlocks server to upload to. If this argument is omitted, the files will not be uploaded.')
    parser.add_argument('-u', '--user', help='The user name and password to use for authentication. Seperate by a colon.')
    return parser.parse_args()

def save_definition(output, definition, name):
    filename = os.path.join(output, name + '.json')
    with open(filename, 'w') as json_file:
        json.dump(definition, json_file, indent=4)
    print(f'Saved {name} definition to {filename}')

def generate_angular(json_definition, output):
    print('Generating angular definition')
    blocks = []
    nodes = []
    definition = {
        'blocks': blocks,
        'nodes': nodes
    }

    # Convert the blocks
    count = 0
    for block in json_definition['blocks']:
        skip = False
        try:
            if 'angular' in block:
                skip = block['angular']['skip']
            else:
                skip = True
        except KeyError:
            pass
        if skip:
            continue

        name = block['name']
        print(f'-> Converting block {name}')
        new_block = {
            'name': name
        }

        text = block['text']
        new_block['text'] = text
        try:
            new_block['category'] = block['category']
        except KeyError:
            new_block['category'] = 'Unknown'
        def_json = block['angular']['definition']
        def_json_parsed = json.loads(def_json)
        update_json = False
        if not 'type' in def_json_parsed:
            def_json_parsed['type'] = name
            update_json = True

        if not 'message0' in def_json_parsed:
            def_json_parsed['message0'] = text
            update_json = True

        if not 'inputsInline' in def_json_parsed:
            def_json_parsed['inputsInline'] = True
            update_json = True

        if not 'nextStatement' in def_json_parsed:
            try:
                add_next = not block['angular']['end']
            except KeyError:
                add_next = True

            if add_next:
                def_json_parsed['nextStatement'] = None
                update_json = True

        if not 'previousStatement' in def_json_parsed:
            try:
                add_prev = not block['angular']['start']
            except KeyError:
                add_prev = True

            if add_prev:
                def_json_parsed['previousStatement'] = None
                update_json = True

        if update_json:
            def_json = json.dumps(def_json_parsed)

        new_block['definition'] = def_json
        try:
            generator = block['angular']['generator']
        except KeyError:
            generator = block['generator']
        new_block['generator'] = generator.replace('##PREFIX##', '')

        blocks.append(new_block)
        count += 1
    print(f'Added {count} block(s)')
    blocks.sort(key=lambda i: i['name'])

    # Convert the nodes (converters)
    count = 0
    for converter in json_definition['converters']:
        skip = False
        try:
            skip = converter['angular']['skip']
        except KeyError:
            pass
        if skip:
            continue

        name = converter['name']
        print(f'-> Converting node {name}')
        new_node = {
            'name': name
        }

        new_node['converter'] = converter['angular']['converter']
        nodes.append(new_node)
        count += 1
    print(f'Added {count} node(s)')
    nodes.sort(key=lambda i: i['name'])

    save_definition(output, definition, 'angular')

def generate_topCodes(json_definition, output):
    print('Generating tangible definition')
    blocks = []
    images = []
    definition = {
        'blocks': blocks,
        'images': images
    }

    # Convert the blocks
    count = 0
    for block in json_definition['blocks']:
        skip = False
        try:
            if 'topCodes' in block:
                skip = block['topCodes']['skip']
            else:
                skip = True
        except KeyError:
            pass
        if skip:
            continue

        name = block['name']
        print(f'-> Converting block {name}')
        new_block = {
            'name': name
        }

        new_block['text'] = block['text']
        new_block['image'] = block['image']
        new_block['numbers'] = block['topCodes']['numbers']
        try:
            generator = block['topCodes']['generator']
        except KeyError:
            generator = block['generator']
        new_block['generator'] = generator.replace('##PREFIX##', 'Tangibles.NaoLang.generatePrefix() + ')

        blocks.append(new_block)
        count += 1
    print(f'Added {count} block(s)')
    blocks.sort(key=lambda i: i['name'])

    # Convert the images
    count = 0
    for image in json_definition['images']:
        skip = False
        try:
            skip = image['topCodes']['skip']
        except KeyError:
            pass
        if skip:
            continue

        name = image['name']
        print(f'-> Converting image {name}')
        new_image = {
            'name': name
        }

        new_image['image'] = image['image']
        images.append(new_image)
        count += 1
    print(f'Added {count} image(s)')
    images.sort(key=lambda i: i['name'])

    save_definition(output, definition, 'topCodes')

def upload_definition(server, type, headers, output, definition):
    filename = os.path.join(output, definition + '.json')
    print(f'-> Uploading "{type}" definition from {filename}...')
    with open(filename, 'r') as input_file:
        json_definition = json.load(input_file)
    
    resp = requests.post(f'{server}api/v1/ui/{type}?replace=yes', headers=headers, json=json_definition)
    resp.raise_for_status()
    print('-> ...done')

def upload_to_server(server, user, output):
    if not server.endswith('/'):
        server += '/'
    print(f'Attempting to upload to {server}')

    print('-> Pinging server')
    resp = requests.get(f'{server}api/v1/version')
    resp.raise_for_status()
    version = resp.json()['version']
    print(f'-> Server version is {version}')

    headers = {}
    if user is None:
        print('-> User has not been set - cannot upload')
        return 

    parts = user.split(':')
    print(f'-> Authenticating "{parts[0]}"')
    resp = requests.post(f'{server}api/v1/session', json={
        'name': parts[0],
        'password': parts[1]
    })
    resp.raise_for_status()
    token = resp.json()['output']['token']
    headers['Authorization'] = f'Bearer {token}'

    print('-> Verifying role')
    resp = requests.get(f'{server}api/v1/session', headers=headers)
    resp.raise_for_status()
    if resp.json()['role'] != 'Administrator':
        print('-> User must have administrator role')
        return

    print('-> Role is valid')
    upload_definition(server, 'angular', headers, output, 'angular')
    upload_definition(server, 'tangibles', headers, output, 'topCodes')
    print('Upload completed')

def main():
    args = parse_args()
    print('Reading definition from ' + args.input)
    print('Writing split definitions to ' + args.output)

    if not os.path.exists(args.output):
        print('Adding output directory')
        os.makedirs(args.output)
    else:
        print('Output directory already exists')

    print('Reading json file')
    with open(args.input, 'r') as input_file:
        json_definition = json.load(input_file)

    generate_angular(json_definition, args.output)
    generate_topCodes(json_definition, args.output)

    if not args.server is None:
        upload_to_server(args.server, args.user, args.output)


if __name__ == "__main__":
    main()
