"""
UI Definition Generator
=======================

This utility will split a unified UI definition into the definitions for Angular and Tangible interfaces
(maybe more in future.)

Having a single unified definition means we can define blocks for all interfaces at once, allowing better
cross-interface comparisions.

There are two arguments:
* --input - the input file containing the unified definition.
* --output - the output folder that will contain the split definitions. The folder will be added if it
             does not already exist

Example command line:
py -3 generate_ui_definitions.py --input ../Data/unified-ui.json --output ../Data/From_Unified

"""

import argparse
import json
import os

def parse_args():
    parser = argparse.ArgumentParser(description='Unified Interface Definition Splitter.')
    parser.add_argument('--input', help='The input unified definition', required=True)
    parser.add_argument('--output', help='The output folder', required=True)
    return parser.parse_args()

def save_definition(output, definition, name):
    filename = os.path.join(output, name + '.json')
    with open(filename, 'w') as json_file:
        json.dump(definition, json_file, indent=4)
    print('Saved ' + name +' definition to ' + filename)

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
            skip = block['angular']['skip']
        except KeyError:
            pass
        if skip:
            continue

        name = block['name']
        print('-> Converting block ' + name)
        new_block = {
            'name': name
        }

        new_block['text'] = block['text']
        new_block['category'] = block['category']
        new_block['definition'] = block['angular']['definition']
        try:
            generator = block['angular']['generator']
        except KeyError:
            generator = block['generator']
        new_block['generator'] = generator.replace('##PREFIX##', '')

        blocks.append(new_block)
        count += 1
    print('Added ' + str(count) + ' block(s)')

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
        print('-> Converting node ' + name)
        new_node = {
            'name': name
        }

        new_node['converter'] = converter['angular']['converter']
        nodes.append(new_node)
        count += 1
    print('Added ' + str(count) + ' node(s)')

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
            skip = block['topCodes']['skip']
        except KeyError:
            pass
        if skip:
            continue

        name = block['name']
        print('-> Converting block ' + name)
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
    print('Added ' + str(count) + ' block(s)')

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
        print('-> Converting image ' + name)
        new_image = {
            'name': name
        }

        new_image['image'] = image['image']
        images.append(new_image)
        count += 1
    print('Added ' + str(count) + ' image(s)')

    save_definition(output, definition, 'topCodes')

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

if __name__ == "__main__":
    main()
