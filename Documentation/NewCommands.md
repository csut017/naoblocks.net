# New Commands

This page contains the instructions for how to add a new command to the system.

Adding a new command is a two step process:
1. Add the command to the robot
1. Define the command on the server

## Robot

The robot is responsible for executing the command. If the command is defined on the server, but not the robot, you will get an execution error (and crash the executing program!)

The actual change will depend on the type of robot, as each robot may have its own execution engine.

### Nao Robot

The execution engine for the Nao robot is defined in [engine.py](../Robots/nao/engine.py).

Adding a new command requires two steps:
1. Define the command function.
1. Register the function and associate it with a function name.

#### Command Functions

A command function is a Python method function. It requires two arguments: `self` and `state`. `self` is a reference to the calling object, while `state` exposes the execution engine's current state. This argument can be used for retrieving information like method arguments.

The following is a simple function that outputs Hello world to the log:

```python
    def _helloWorld(self, state):
        logger.log('[Engine] Hello world')
```

A more complete example that uses method arguments:

```python
    def _helloPerson(self, state):
        name = self._evaluate(state.ast['arguments'][0], state)
        logger.log('[Engine] Hello ' + name)
```

#### Registering a Function

Functions are registered as part of the [`_reset()`](../Robots/nao/engine.py#L221) function. This method defines a dictionary with all the functions and their associated names.

To add a new function, add a new entry to the dictionary. For example, to add the above two functions:

```python
        # New custom functions
        'helloWorld': EngineFunction(self._helloWorld),
        'logPerson': EngineFunction(self._helloPerson),
```

The `EngineFunction` class is required. It provides a wrapper around your function with additional metadata for the engine.

#### Behaviour-only Functions

One common design approach is to build a behaviour in Choregraphe and download it to the robot. Executing the behaviour involves starting it and waiting for it to complete.

There is a helper method that allows calling the behaviour directly, rather than writing a command function for every behaviour. This helper will register the behaviour as a function directly.

To call it, add a function registration the [`_reset()`](../Robots/nao/engine.py#L221) function:
``` python
        # Behaviour functions
        'doSomething': EngineFunction(self._generate_behaviour('behaviour-name', 'behaviour-id')),
```

Where behaviour-id is the identifier of the behaviour (from Choregraphe) and behaviour-name is a human-readble name (it is only used in the logs.)

#### Deploying the Changes

You will need to deploy the changes once you have modified [engine.py](../Robots/nao/engine.py).

1. Stop the client code on the robot (if running)
1. Copy the code to the robot
  - You can use  `scp engine.py nao@ipaddress://home/nao/naoblocks/` to copy to the robot
1. Start the code

**Don't forget to test your code! Assume it does NOT work until tested.**

## Server

Now that the command is defined on the robot, we need to add the server configuration so a user can include it in their programs.

### Command Definition File

Currently, there are two formats for defining a command on the server. There is a robot-specific format and a unified, general format. Moving forward, we will only be using the unified format, so, I won't be including any details on the robot-specific format. If you want to see these, there are some examples in the Data folder.

The unified data format uses JSON to define three components:
* Blocks
* Images
* Converters

**Blocks** are used to define the individual commands in the system. These will appear as blocks in the Angular/Blockly interface and defines the codes for the tangible interface. They map to the commands on the robot.

Each block has the following attributes:
* **name**: this is the internal name of the block. Each block must have a unique name.
* **topCodes**: TopCodes specific attributes:
  - **numbers**: an aray of integers for mapping the TopCodes icon to a block. These must be unique within each UI definition.
* **generator**: generates the NaoLang code for the block. This is JavaScript that will be embedded directly into the application. ##PREFIX## us a special prefix that refers to the underlying application.

TODO: include details on the other definition attributes

**Images** are used to define the icons that appear in the tangible interface. These are data-encoded images that will be directly loaded into the interface. Currently, the only format that the UI handles is base-64 encoded PNG images.

**Converters** are currently not used by the system. They are needed to handle the conversion from NaoLang code to a Blockly XML definition.

### Uploading from Command-line

There is a python script that will split the unified definition into the robot specific definitions. In addition, this script can be used for uploading the split scripts directly to a running NaoBlocks instance.

This script requires Python 3.11 or later, and the requests package. Python can be downloaded from the [Python Downloads](https://www.python.org/downloads/) page. Once Python is installed, requests can be installed by running the following command:
```
py -3 -m pip install requests
```

The split and upload script is called generate_ui_definitions.py and can be found in the Utilities\Scripts folder. 

To upload a blocks definition, open a new command prompt and navigate to this location. Then, run the following command:

```
py -3 generate_ui_definitions.py ../../Data/unified-all.json ../../Data/From_Unified --server http://localhost:5000 --user user:password
```

Where
* *unified-all.json* is the name of the definition file to split and upload. You will need to replace this name with the name of your own file.
* *http://localhost:5000* is the target web server to upload to. If you are running the server on the same machine, you can leave this unchanged.
* *user:password* is the user name and password for an admin user on the server. You will need to change this before attempting to upload.


### Uploading via the Web Interface

TODO: How to use the web interface for uploading a command definition file