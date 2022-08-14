# New Commands

This page contains the instructions for how to add a new command to the system.

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

Functions are registered as part of the `_reset()` function. This method defines a dictionary with all the functions and their associated names.

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

To call it, add a function registration the `_reset()` function:
``` python
        # Behaviour functions
        'doSomething': EngineFunction(self._generate_behaviour('behaviour-name', 'behaviour-id')),
```

Where behaviour-id is the identifier of the behaviour (from Choregraphe) and behaviour-name is a human-readble name (it is only used in the logs.)
