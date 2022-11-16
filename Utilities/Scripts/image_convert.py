"""
Image converter
===============

This utility will convert a SVG image to a base64 encoded PNG.

The conversion is needed to upload images for the tangibles interface. Each image needs to be 48x48 
(you can download them from https://fonts.google.com/icons?icon.set=Material+Icons, just make sure
you set the size download as SVG.)

There are two arguments:
* --input - the input folder containing the SVG images
* --output -- the output folder that will contain the base64 encoded PNGs

For this utility to work, you will need to install svglib:
* py -3 -m pip install svglib

Example command line:
py -3 image_convert.py --input ..\Data\svg --output ..\Data\png

"""

import argparse
import base64
import os

from PIL import Image
from reportlab.graphics import renderPM
from svglib.svglib import svg2rlg

def convert_image(filename):
    img = Image.open(filename)
    img = img.convert("RGBA")
 
    previous_image = img.getdata() 
    new_image = [] 
    for item in previous_image:
        if item[0] == 255 and item[1] == 255 and item[2] == 255:
            new_image.append((255, 255, 255, 0))
        else:
            new_image.append(item)
 
    img.putdata(new_image)
    img.save(filename, "PNG")    

def parse_args():
    parser = argparse.ArgumentParser(description='Image converter.')
    parser.add_argument('--input', help='The input folder', required=True)
    parser.add_argument('--output', help='The output folder', required=True)
    return parser.parse_args()

def main():
    args = parse_args()
    print('Reading images from ' + args.input)
    print('Writing images to ' + args.output)
    files = os.listdir(args.input)
    for filename in files:
        full_path = os.path.join(args.input, filename)
        print('Converting ' + full_path)
        drawing = svg2rlg(full_path)
        short_name, _ = os.path.splitext(filename)
        png_path = os.path.join(args.output, short_name + '.png')
        print('-> Saving to ' + png_path)
        renderPM.drawToFile(drawing, png_path, fmt='PNG')
        convert_image(png_path)

        with open(png_path, "rb") as img_file:
            b64_string = base64.b64encode(img_file.read())
        with open(png_path + ".encoded", "wb") as img_file:
            img_file.write(b64_string)

if __name__ == "__main__":
    main()
