# Running a Demo

These introductions explain how to setup and run a demo.

**Note:** Instructions in italics are specific to the UoA configuration, you will need to change them in other environments.

## Setup

1. Turn on the router
1. Unpack and turn on robots
1. Turn on server laptop and connect to correct network 
    * *`naoblocks` for UoA*
1. Start `NaoBlocks.Net` server 
    * Currently, this needs to be via Visual Studio as we don't have an installer for running it outside of Visual Studio
1. Start the client on each robot
    1. SSH to the robot
        * `ssh nao@ipaddress`
        * Where *ipaddress* is the address of the robot *(see [Topographies](Topography1.md))*
    1. Change to the correct directory
        * `cd naoblocks`
    1. Start the client
        * `python auto.py`
1. Start the client on each laptop
    * Start the laptop and connect to the correct network
    * Start a browser (tested in Chrome)
    * Navigate to the server web address
        * *If the hosts file has been configured, it will be `one|two|three|four.naoblocks.nz` (see [Topographies](Topography1.md))*

## Running the demo

For running the demo, all you need to do is login using one of the accounts and then `NaoBlocks.Net` is ready to go.

Currently, the default configuration contains blocks for the following combinations:
* Nao - Blockly (web-based)
* Nao - Tangibles
