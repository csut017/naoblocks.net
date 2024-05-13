# mBot2 (Simple)

This folder contains the mBot2 robot client that listens to the server.

This code runs directly on the robot. It polls the server for messages and then executes them.

## Process

This code will continuously poll the server for data. The data, when received, will be a stream of characters (see current mapping). It will then execute each code in sequence.

There is one special code: Parse control settings. This code will read the characters until the next instance of the code. Internally, the settings uses the following format:

    <setting>=<value>[,<next setting>]

There is no limit to the number of settings that can be contained in a control settings instance. The parser will read each setting and then apply each setting as it is finished.

## Current Mapping

The following codes are currently mapped:
* A -> Forward
* B -> Backward
* C -> Turn left
* D -> Turn right
* E -> Curve left
* F -> Curve right
* G -> Stop
* : -> Parse control settings

## Control Settings

There are the following settings:
* dur -> the duration of execution for each command (in ms), must be a number
* del -> the delay between command execution (in ms), must be a number