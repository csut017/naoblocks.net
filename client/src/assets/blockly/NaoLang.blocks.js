var blocks = [{
    "type": "drone_flip",
    "message0": "Flip %1",
    "args0": [{
        "type": "field_dropdown",
        "name": "DIRECTION",
        "options": Blockly.NaoLang.Directions2D
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Tells the drone to flip in a direction."
}, {
    "type": "drone_fly",
    "message0": "Fly %1 %2 cm",
    "args0": [{
        "type": "field_dropdown",
        "name": "DIRECTION",
        "options": Blockly.NaoLang.Directions3D
    }, {
        "type": "input_value",
        "check": "Number",
        "name": "DISTANCE"
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Tells the drone to fly in a direction for a certain distance."
}, {
    "type": "drone_land",
    "message0": "Land",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Tells the drone to land."
}, {
    "type": "drone_stop",
    "message0": "Stop",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Tells the drone to stop in position and hover."
}, {
    "type": "drone_takeoff",
    "message0": "Takeoff",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Tells the drone to take off."
}, {
    "type": "robot_action",
    "message0": "Do %1",
    "args0": [{
        "type": "field_dropdown",
        "name": "ACTION",
        "options": Blockly.NaoLang.Actions
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Performs an action and returns to the previous position."
}, {
    "type": "robot_action_and_say",
    "message0": "Do %1 and say %2",
    "args0": [{
        "type": "field_dropdown",
        "name": "ACTION",
        "options": Blockly.NaoLang.Actions
    },
    {
        "type": "input_value",
        "name": "TEXT"
    }
    ],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Performs an action and returns to the previous position."
}, {
    "type": "robot_change_chest",
    "message0": "Change chest to %1",
    "args0": [{
        "type": "input_value",
        "name": "COLOUR"
    }],
    "inputsInline": true,
    "previousStatement": null,
    "nextStatement": null,
    "colour": 65,
    "tooltip": "Changes the colour of the robot's chest."
}, {
    "type": "robot_change_eye",
    "message0": "Change %1 eye(s) to %2",
    "args0": [{
        "type": "field_dropdown",
        "name": "EYE",
        "options": [
            ["Both", "BOTH_EYES"],
            ["Left", "LEFT_EYE"],
            ["Right", "RIGHT_EYE"]
        ]
    },
    {
        "type": "input_value",
        "name": "COLOUR"
    }
    ],
    "inputsInline": true,
    "previousStatement": null,
    "nextStatement": null,
    "colour": 65,
    "tooltip": "Changes the colour of the robot's eyes."
}, {
    "type": "robot_sensor_head",
    "message0": "%1 head touched",
    "args0": [{
        "type": "field_dropdown",
        "name": "SENSOR",
        "options": [
            ["Front", "FRONT"],
            ["Middle", "MIDDLE"],
            ["Rear", "REAR"]
        ]
    }],
    "output": null,
    "colour": 85,
    "tooltip": "Retrieves whether a head sensor is touched"
}, {
    "type": "robot_sensor_sonar",
    "message0": "Distance to %1 front",
    "args0": [{
        "type": "field_dropdown",
        "name": "SENSOR",
        "options": [
            ["Left", "LEFT"],
            ["Right", "RIGHT"]
        ]
    }],
    "output": null,
    "colour": 85,
    "tooltip": "Retrieves the distance to the front of the root on a side"
}, {
    "type": "robot_sensor_battery",
    "message0": "Battery charge",
    "output": null,
    "colour": 85,
    "tooltip": "Retrieves the current charge level of the battery"
}, {
    "type": "robot_last_word",
    "message0": "Last word recognised",
    "output": null,
    "colour": 85,
    "tooltip": "Retrieves the last word recognised"
}, {
    "type": "robot_sensor_gyroscope",
    "message0": "%1 gyroscope",
    "args0": [{
        "type": "field_dropdown",
        "name": "SENSOR",
        "options": [
            ["x", "X"],
            ["y", "Y"],
            ["z", "Z"]
        ]
    }],
    "output": null,
    "colour": 85,
    "tooltip": "Retrieves the value of the gyroscope in an axis"
}, {
    "type": "robot_hand",
    "message0": "%1 %2 hand(s)",
    "args0": [{
        "type": "field_dropdown",
        "name": "ACTION",
        "options": [
            ["Open", "open"],
            ["Close", "close"],
        ]
    },
    {
        "type": "field_dropdown",
        "name": "HAND",
        "options": [
            ["Left", "LEFT"],
            ["Right", "RIGHT"],
            ["Both", "BOTH"],
        ]
    }
    ],
    "inputsInline": true,
    "previousStatement": null,
    "nextStatement": null,
    "colour": 65,
    "tooltip": "Changes the colour of the robot's eyes."
}, {
    "type": "robot_on_button",
    "message0": "When %1 pressed",
    "args0": [{
        "type": "field_dropdown",
        "name": "BUTTON",
        "options": [
            ["Front Head", "front"],
            ["Middle Head", "middle"],
            ["Read Head", "rear"],
            ["Chest", "chest"],
        ]
    }],
    "message1": "Do %1",
    "args1": [{
        "type": "input_statement",
        "name": "ACTION"
    }],
    "inputsInline": true,
    "colour": 290,
    "tooltip": "Starts the block when a button is touched."
}, {
    "type": "robot_on_start",
    "message0": "On start",
    "message1": "do %1",
    "args1": [{
        "type": "input_statement",
        "name": "ACTION"
    }],
    "colour": 290,
    "tooltip": "Starts the block when the script is downloaded.",
    "helpUrl": ""
}, {
    "type": "robot_on_recognised",
    "message0": "On word recognised",
    "message1": "do %1",
    "args1": [{
        "type": "input_statement",
        "name": "ACTION"
    }],
    "colour": 290,
    "tooltip": "Starts the block when a word is recognised.",
    "helpUrl": ""
}, {
    "type": "robot_play_audio",
    "message0": "Play audio %1",
    "args0": [{
        "type": "field_dropdown",
        "name": "AUDIO",
        "options": [
            ["Last Recorded", "record"],
        ]
    }],
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Puts the robot in a safe resting position."
}, {
    "type": "robot_posture",
    "message0": "Move to %1",
    "args0": [{
        "type": "field_dropdown",
        "name": "POSTURE",
        "options": Blockly.NaoLang.Postures
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Moves to the specified position."
}, {
    "type": "robot_posture_and_say",
    "message0": "Move to %1 and say %2",
    "args0": [{
        "type": "field_dropdown",
        "name": "POSTURE",
        "options": Blockly.NaoLang.Postures
    },
    {
        "type": "input_value",
        "name": "TEXT"
    }
    ],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Moves to the specified position."
}, {
    "type": "robot_look",
    "message0": "Look %1",
    "args0": [{
        "type": "field_dropdown",
        "name": "DIR",
        "options": [
            ["left", "LEFT"],
            ["ahead", "AHEAD"],
            ["right", "RIGHT"],
        ]
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Moves the robot's head to look in the direction."
}, {
    "type": "robot_point",
    "message0": "Point %1 arm %2",
    "args0": [{
        "type": "field_dropdown",
        "name": "ARM",
        "options": [
            ["left", "LEFT"],
            ["right", "RIGHT"],
        ]
    }, {
        "type": "field_dropdown",
        "name": "DIR",
        "options": [
            ["out", "OUT"],
            ["ahead", "AHEAD"],
            ["down", "DOWN"],
            ["up", "UP"],
        ]
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Moves one of the robot's arms to point in the direction."
}, {
    "type": "robot_point_left",
    "message0": "Point left",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Points the robot's left arm."
}, {
    "type": "robot_point_right",
    "message0": "Point right",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Points the robot's right arm."
}, {
    "type": "robot_gangnam",
    "message0": "Gangnam (music %1)",
    "args0": [{
        "type": "field_dropdown",
        "name": "MUSIC",
        "options": [
            ["on", "TRUE"],
            ["off", "FALSE"],
        ]
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Performs the gangnam dance."
}, {
    "type": "robot_dance",
    "message0": "Dance",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Performs the gangnam dance."
}, {
    "type": "robot_macaranna",
    "message0": "Macaranna (music %1)",
    "args0": [{
        "type": "field_dropdown",
        "name": "MUSIC",
        "options": [
            ["on", "TRUE"],
            ["off", "FALSE"],
        ]
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Performs the macaranna dance."
}, {
    "type": "robot_taichi",
    "message0": "Tai chi (music %1)",
    "args0": [{
        "type": "field_dropdown",
        "name": "MUSIC",
        "options": [
            ["on", "TRUE"],
            ["off", "FALSE"],
        ]
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Performs a tai chi dance."
}, {
    "type": "robot_record_audio",
    "message0": "Record audio for %1s",
    "args0": [{
        "type": "input_value",
        "check": "Number",
        "name": "TIME"
    }],
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Records some audio."
}, {
    "type": "robot_rest",
    "message0": "Rest",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Puts the robot in a safe resting position."
}, {
    "type": "robot_wait",
    "message0": "Wait for %1s",
    "args0": [{
        "type": "input_value",
        "check": "Number",
        "name": "TIME"
    }],
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Puts the robot in a safe resting position."
}, {
    "type": "robot_say",
    "message0": "Say %1",
    "args0": [{
        "type": "input_value",
        "name": "TEXT"
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Speak the specified text."
}, {
    "type": "robot_kia_ora",
    "message0": "Say 'Kia ora'",
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Say kia ora."
}, {
    "type": "text_concat",
    "message0": "Append %1 to %2",
    "args0": [{
        "type": "input_value",
        "name": "TEXT"
    },
    {
        "type": "input_value",
        "name": "VAR"
    }
    ],
    "inputsInline": true,
    "colour": 165,
    "output": null,
    "tooltip": "Append text to a variable."
}, {
    "type": "robot_move",
    "message0": "Move forward %1s seconds",
    "args0": [{
        "type": "input_value",
        "check": "Number",
        "name": "SECONDS"
    }
    ],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Moves forward for the duration."
}, {
    "type": "robot_turn",
    "message0": "Turn %1 for %2 seconds",
    "args0": [{
        "type": "field_dropdown",
        "name": "DIR",
        "options": [
            ["left", "LEFT"],
            ["right", "RIGHT"],
        ]
    },{
        "type": "input_value",
        "check": "Number",
        "name": "SECONDS"
    }],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Turns for the duration."
}, {
    "type": "robot_walk",
    "message0": "Walk forward %1s, sideways %2s",
    "args0": [{
        "type": "input_value",
        "check": "Number",
        "name": "X"
    },
    {
        "type": "input_value",
        "check": "Number",
        "name": "Y"
    }
    ],
    "inputsInline": true,
    "nextStatement": null,
    "previousStatement": null,
    "colour": 65,
    "tooltip": "Walks for the duration."
}];
Blockly.NaoLang.Postures.forEach(function (posture) {
    blocks.push({
        "type": "robot_posture_" + posture[1],
        "message0": posture[0],
        "inputsInline": true,
        "nextStatement": null,
        "previousStatement": null,
        "colour": 65,
        "tooltip": "Moves to the " + posture[0] + " position."
    });
});
Blockly.NaoLang.Actions.forEach(function (action) {
    blocks.push({
        "type": "robot_action_" + action[1],
        "message0": action[0],
        "inputsInline": true,
        "nextStatement": null,
        "previousStatement": null,
        "colour": 65,
        "tooltip": "Performs a " + action[0] + ' action'
    });
});
console.groupCollapsed('[NaoLang] Defining blocks');
blocks.forEach(function (block) {
    console.log('[NaoLang] Defining ' + block.type);
    Blockly.Blocks[block.type] = {
        init: function () {
            console.log('[NaoLang] Initialising ' + block.type);
            this.jsonInit(block);
            var thisBlock = this;
            this.setTooltip(function () {
                return block.tooltip;
            });
        }
    };
});
console.groupEnd();