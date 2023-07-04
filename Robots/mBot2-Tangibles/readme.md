# mBot2 (Tangibles)

This folder contains the mBot2 robot client for tangibles.

This code runs directly on the robot. It scans for command cards and executes them directly. The only server interaction is via logging.

This code was originally written as part of Te haerenga a Kupe, Kupe's Journey project.

## Barcodes

This system uses the barcodes from the PixyMon smart camera.

### Current Mapping

The following codes are currently mapped:
* 1 -> Forward
* 2 -> Backward
* 3 -> Turn left
* 4 -> Turn right
* 5 -> Curve left
* 6 -> *DO NOT USE*
* 7 -> Record A
* 8 -> Record B
* 9 -> Repeat
* 10-> *DO NOT USE*
* 11-> Play B
* 12-> *DO NOT USE*
* 13->Curve right
* 14->Play B
* 15-> Stop

### Problem Codes

However, some of the barcodes are inverted images of others. The following are inverted pairs:
* 2 and 6
* 4 and 10
* 8 and 12

In addition, 1 and 9 are sometimes detected as 14.

15 is the only code that is the same when inverted.
