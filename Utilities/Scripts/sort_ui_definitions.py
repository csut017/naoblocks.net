"""
UI Definition Sorter
=======================

This utility will sort all the blocks in a definition file. This makes it easier to find the blocks.

The first argument is the path to the unified definition.

Example command line (basic):
    py -3 sort_ui_definitions.py ../../Data/unified-all.json

"""

import argparse
import json

def parse_args():
    parser = argparse.ArgumentParser(description='Unified Interface Definition Splitter.')
    parser.add_argument('input', help='The input definition.')
    return parser.parse_args()

def get_key(item):
    try:
        group_name = item["group"]
    except KeyError:
        group_name = ''
    return group_name + '->' + item['name']

def sort_definitions(json_definition):
    json_definition["blocks"].sort(key=get_key)

def main():
    args = parse_args()
    print('Sorting definition from ' + args.input)

    print('Reading json file')
    with open(args.input, 'r') as input_file:
        json_definition = json.load(input_file)

    print('Sorting definitions')
    sort_definitions(json_definition)

    print('Writing json file')
    with open(args.input, 'w') as output_file:
        json.dump(json_definition, output_file)

if __name__ == "__main__":
    main()
