// Override the default soupid_ to provide a safe set of characters for the language generation
Blockly.utils.genUid.soup_ = '!#$%()*+,-./:;=?@^_`{|}~' +
    'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';


Blockly.NaoLang = new Blockly.Generator('NaoLang');

// Define the orders
Blockly.NaoLang.ORDER_ATOMIC = 0;
Blockly.NaoLang.ORDER_COLLECTION = 1;
Blockly.NaoLang.ORDER_STRING_CONVERSION = 1;
Blockly.NaoLang.ORDER_MEMBER = 2.1;
Blockly.NaoLang.ORDER_FUNCTION_CALL = 2.2;
Blockly.NaoLang.ORDER_EXPONENTIATION = 3;
Blockly.NaoLang.ORDER_UNARY_SIGN = 4;
Blockly.NaoLang.ORDER_BITWISE_NOT = 4;
Blockly.NaoLang.ORDER_MULTIPLICATIVE = 5;
Blockly.NaoLang.ORDER_ADDITIVE = 6;
Blockly.NaoLang.ORDER_BITWISE_SHIFT = 7;
Blockly.NaoLang.ORDER_BITWISE_AND = 8;
Blockly.NaoLang.ORDER_BITWISE_XOR = 9;
Blockly.NaoLang.ORDER_BITWISE_OR = 10;
Blockly.NaoLang.ORDER_RELATIONAL = 11;
Blockly.NaoLang.ORDER_LOGICAL_NOT = 12;
Blockly.NaoLang.ORDER_LOGICAL_AND = 13;
Blockly.NaoLang.ORDER_LOGICAL_OR = 14;
Blockly.NaoLang.ORDER_CONDITIONAL = 15;
Blockly.NaoLang.ORDER_LAMBDA = 16;
Blockly.NaoLang.ORDER_NONE = 99;

Blockly.NaoLang.addReservedWords('FALSE,TRUE');
Blockly.NaoLang.includeId = true;
Blockly.NaoLang.prefix = '  ';
Blockly.NaoLang.prefixLevel = 0;

// Define the standard postures
Blockly.NaoLang.Postures = [
    ["Stand", "Stand"],
    ["Sit Forward", "Sit"],
    ["Sit Back", "SitRelax"],
    ["Crouch", "Crouch"],
    ["Lie on Front", "LyingBelly"],
    ["Lie on Back", "LyingBack"]
];

// Define the standard actions
Blockly.NaoLang.Actions = [
    ["Wave", "wave"],
    ["Wipe Forehead", "wipe_forehead"],
];

// Initialise code generation
Blockly.NaoLang.init = function (workspace) {
    Blockly.NaoLang.prefixLevel = 1;
    if (!Blockly.NaoLang.variableDB_) {
        Blockly.NaoLang.variableDB_ = new Blockly.Names(Blockly.NaoLang.RESERVED_WORDS_);
    } else {
        Blockly.NaoLang.variableDB_.reset();
    }
    Blockly.NaoLang.variableDB_.setVariableMap(workspace.getVariableMap());
};

// Finish generation
Blockly.NaoLang.finish = function (code) {
    if (Blockly.NaoLang.addStart) {
        return 'reset()\nstart{\n' + code + '}\ngo()\n';
    }
    return 'reset()\n' + code + 'go()\n';
};

// Common tasks for generating code from blocks
Blockly.NaoLang.scrub_ = function (block, code) {
    var nextBlock = block.nextConnection && block.nextConnection.targetBlock(),
        nextCode = Blockly.NaoLang.blockToCode(nextBlock);
    return (Blockly.NaoLang.includeId ? '[' + block.id + ']' : '') + code + nextCode;
};

Blockly.NaoLang.generatePrefix = function () {
    return Blockly.NaoLang.includeId
        ? ''
        : Blockly.NaoLang.prefix.repeat(Blockly.NaoLang.prefixLevel);
}

// Robot events
Blockly.NaoLang.robot_on_button = function (block) {
    var button = block.getFieldValue('BUTTON'),
        innerCode = Blockly.NaoLang.statementToCode(block, 'ACTION'),
        code = button + 'Button{\n' +
            innerCode +
            '}\n';
    return code;
};
Blockly.NaoLang.robot_on_start = function (block) {
    var innerCode = Blockly.NaoLang.statementToCode(block, 'ACTION'),
        code = 'start{\n' +
            innerCode +
            '}\n';
    return code;
};
Blockly.NaoLang.robot_on_recognised = function (block) {
    var innerCode = Blockly.NaoLang.statementToCode(block, 'ACTION'),
        code = 'wordRecognised{\n' +
            innerCode +
            '}\n';
    return code;
};

// Robot sensors
Blockly.NaoLang['robot_sensor_head'] = function (block) {
    var code = 'readSensor(HEAD_' + Blockly.NaoLang.variableDB_.getName(block.getFieldValue('SENSOR'), Blockly.Variables.NAME_TYPE) + ')';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang['robot_sensor_battery'] = function (block) {
    var code = 'readSensor(BATTERY)';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang['robot_sensor_sonar'] = function (block) {
    var code = 'readSensor(SONAR_' + Blockly.NaoLang.variableDB_.getName(block.getFieldValue('SENSOR'), Blockly.Variables.NAME_TYPE) + ')';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang['robot_sensor_gyroscope'] = function (block) {
    var code = 'readSensor(GYROSCOPE_' + Blockly.NaoLang.variableDB_.getName(block.getFieldValue('SENSOR'), Blockly.Variables.NAME_TYPE) + ')';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang['robot_last_word'] = function (block) {
    var code = 'lastRecognisedWord()';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

// Robot actions
Blockly.NaoLang.robot_action = function (block) {
    var code = block.getFieldValue('ACTION');
    return Blockly.NaoLang.generatePrefix() + code + '()\n';
};
Blockly.NaoLang.robot_action_and_say = function (block) {
    var textToSpeak = Blockly.NaoLang.valueToCode(block, 'TEXT', Blockly.NaoLang.ORDER_ATOMIC),
        code = block.getFieldValue('ACTION');
    return Blockly.NaoLang.generatePrefix() + code + '(' + textToSpeak + ')\n';
};
Blockly.NaoLang.robot_change_chest = function (block) {
    var value_colour = Blockly.NaoLang.valueToCode(block, 'COLOUR', Blockly.NaoLang.ORDER_ATOMIC);
    return Blockly.NaoLang.generatePrefix() + 'changeLEDColour(CHEST, ' + value_colour + ')\n';
};
Blockly.NaoLang.robot_change_eye = function (block) {
    var value_eye = block.getFieldValue('EYE'),
        value_colour = Blockly.NaoLang.valueToCode(block, 'COLOUR', Blockly.NaoLang.ORDER_ATOMIC),
        code = 'changeLEDColour(' + value_eye + ', ' + value_colour + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_hand = function (block) {
    var value_action = block.getFieldValue('ACTION'),
        value_hand = block.getFieldValue('HAND'),
        code = 'changeHand(\'' + value_action.toLowerCase() + '\',\'' + value_hand.toLowerCase() + '\')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_play_audio = function (block) {
    var audio = block.getFieldValue('AUDIO'),
        code = 'audio(\'' + audio + '\')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_posture = function (block) {
    var posture = block.getFieldValue('POSTURE'),
        code = 'position(\'' + posture + '\')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.Postures.forEach(function (posture) {
    var postureName = posture[1];
    Blockly.NaoLang["robot_posture_" + postureName] = function (block) {
        var code = 'position(\'' + postureName + '\')\n';
        return code;
    };
});
Blockly.NaoLang.Actions.forEach(function (action) {
    var actionName = action[1];
    Blockly.NaoLang['robot_action_' + actionName] = function (block) {
        return actionName + '()\n';
    };
});
Blockly.NaoLang.robot_posture_and_say = function (block) {
    var posture = block.getFieldValue('POSTURE'),
        textToSpeak = Blockly.NaoLang.valueToCode(block, 'TEXT', Blockly.NaoLang.ORDER_ATOMIC),
        code = 'position(\'' + posture + '\', ' + textToSpeak + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_record_audio = function (block) {
    var time = Blockly.NaoLang.valueToCode(block, 'TIME', Blockly.NaoLang.ORDER_ATOMIC);
    var code = 'record(\'record\'' + time + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_rest = function (block) {
    var code = 'rest()\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_wait = function (block) {
    var time = Blockly.NaoLang.valueToCode(block, 'TIME', Blockly.NaoLang.ORDER_ATOMIC);
    var code = 'wait(' + time + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_say = function (block) {
    var textToSpeak = Blockly.NaoLang.valueToCode(block, 'TEXT', Blockly.NaoLang.ORDER_ATOMIC);
    var code = 'say(' + textToSpeak + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_turn = function (block) {
    var seconds = Blockly.NaoLang.valueToCode(block, 'SECONDS', Blockly.NaoLang.ORDER_ATOMIC),
        code = 'turn(' + seconds + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_move = function (block) {
    var seconds = Blockly.NaoLang.valueToCode(block, 'SECONDS', Blockly.NaoLang.ORDER_ATOMIC),
        code = 'moveForward(' + seconds + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_walk = function (block) {
    var xPos = Blockly.NaoLang.valueToCode(block, 'X', Blockly.NaoLang.ORDER_ATOMIC),
        yPos = Blockly.NaoLang.valueToCode(block, 'Y', Blockly.NaoLang.ORDER_ATOMIC);
    var code = 'walk(' + xPos + ',' + yPos + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};

// Robot movements
Blockly.NaoLang.robot_look = function (block) {
    var dir = block.getFieldValue('DIR').toLowerCase(),
        code = 'look(\'' + dir + '\')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_point = function (block) {
    var dir = block.getFieldValue('DIR').toLowerCase(),
        arm = block.getFieldValue('ARM').toLowerCase(),
        code = 'point(\'' + arm + '\',\'' + dir + '\')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};

// Robot dances
Blockly.NaoLang.robot_gangnam = function (block) {
    var music = block.getFieldValue('MUSIC'),
        code = 'dance(\'gangnam\', ' + music + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_macaranna = function (block) {
    var music = block.getFieldValue('MUSIC'),
        code = 'dance(\'macaranna\', ' + music + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};
Blockly.NaoLang.robot_taichi = function (block) {
    var music = block.getFieldValue('MUSIC'),
        code = 'dance(\'taichi\', ' + music + ')\n';
    return Blockly.NaoLang.generatePrefix() + code;
};

// Colour functions
Blockly.NaoLang.colour_picker = function (block) {
    var code = block.getFieldValue('COLOUR');
    if (code[0] != '#') code = '#' + code;
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};
Blockly.NaoLang.colour_random = function (block) {
    var code = 'randomColour()';
    return [code, Blockly.NaoLang.ORDER_FUNCTION_CALL];
};

// Math functions
Blockly.NaoLang.math_number = function (block) {
    var code = parseFloat(block.getFieldValue('NUM')),
        order = Blockly.NaoLang.ORDER_ATOMIC;
    return [code, order];
};

Blockly.NaoLang.math_arithmetic = function (block) {
    var OPERATORS = {
        'ADD': [' + ', Blockly.NaoLang.ORDER_ADDITIVE],
        'MINUS': [' - ', Blockly.NaoLang.ORDER_ADDITIVE],
        'MULTIPLY': [' * ', Blockly.NaoLang.ORDER_MULTIPLICATIVE],
        'DIVIDE': [' / ', Blockly.NaoLang.ORDER_MULTIPLICATIVE],
        'POWER': [' ** ', Blockly.NaoLang.ORDER_EXPONENTIATION]
    };
    var tuple = OPERATORS[block.getFieldValue('OP')],
        operator = tuple[0],
        order = tuple[1],
        argument0 = Blockly.NaoLang.valueToCode(block, 'A', order) || '0',
        argument1 = Blockly.NaoLang.valueToCode(block, 'B', order) || '0',
        code = argument0 + operator + argument1;
    return [code, order];
};
Blockly.NaoLang.math_number_property = function (block) {
    var number_to_check = Blockly.NaoLang.valueToCode(block, 'NUMBER_TO_CHECK', Blockly.NaoLang.ORDER_MULTIPLICATIVE) || '0',
        dropdown_property = block.getFieldValue('PROPERTY'),
        code;
    switch (dropdown_property) {
        case 'PRIME':
            code = 'isPrime(' + number_to_check + ')';
            break;
        case 'EVEN':
            code = 'isEven(' + number_to_check + ')';
            break;
        case 'ODD':
            code = 'isOdd(' + number_to_check + ')';
            break;
        case 'WHOLE':
            code = 'isWhole(' + number_to_check + ')'; number_to_check + ' % 1 == 0';
            break;
        case 'POSITIVE':
            code = 'isPostive(' + number_to_check + ')';
            break;
        case 'NEGATIVE':
            code = 'isNegative(' + number_to_check + ')';
            break;
        case 'DIVISIBLE_BY':
            var divisor = Blockly.NaoLang.valueToCode(block, 'DIVISOR', Blockly.NaoLang.ORDER_MULTIPLICATIVE);
            code = 'isPrime(' + number_to_check + ',' + divisor + ')';;
            break;
    }
    return [code, Blockly.NaoLang.ORDER_RELATIONAL];
};

Blockly.NaoLang.math_round = function (block) {
    var operator = block.getFieldValue('OP'),
        code,
        arg = Blockly.NaoLang.valueToCode(block, 'NUM', Blockly.NaoLang.ORDER_NONE) || '0';
    switch (operator) {
        case 'ROUND':
            code = 'round(' + arg + ')';
            break;
        case 'ROUNDUP':
            code = 'roundup(' + arg + ')';
            break;
        case 'ROUNDDOWN':
            code = 'rounddown(' + arg + ')';
            break;
    }
    return [code, Blockly.NaoLang.ORDER_MULTIPLICATIVE];
};

Blockly.NaoLang.math_random_int = function (block) {
    var argument0 = Blockly.NaoLang.valueToCode(block, 'FROM', Blockly.NaoLang.ORDER_NONE) || '0';
    var argument1 = Blockly.NaoLang.valueToCode(block, 'TO', Blockly.NaoLang.ORDER_NONE) || '0';
    var code = 'randomInt(' + argument0 + ', ' + argument1 + ')';
    return [code, Blockly.NaoLang.ORDER_FUNCTION_CALL];
};

Blockly.NaoLang['math_change'] = function (block) {
    var argument0 = Blockly.NaoLang.valueToCode(block, 'DELTA', Blockly.NaoLang.ORDER_ADDITIVE) || '0',
        varName = Blockly.NaoLang.variableDB_.getName(block.getFieldValue('VAR'), Blockly.Variables.NAME_TYPE);
    return 'addTo(@' + varName + ', ' + argument0 + ')\n';
};

// Text functions
Blockly.NaoLang.text = function (block) {
    var code = '\'' + block.getFieldValue('TEXT').replace(/'/g, '\\\'') + '\'';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang.text_concat = function (block) {
    var varName = Blockly.NaoLang.valueToCode(block, 'VAR', Blockly.NaoLang.ORDER_NONE) || '\'\'',
        value = Blockly.NaoLang.valueToCode(block, 'TEXT', Blockly.NaoLang.ORDER_NONE) || '\'\'',
        order = Blockly.NaoLang.ORDER_RELATIONAL;
    return ['append(' + varName + ',' + value + ')', order];
};

Blockly.NaoLang.text_length = function (block) {
    var text = Blockly.NaoLang.valueToCode(block, 'VALUE', Blockly.NaoLang.ORDER_NONE) || '\'\'';
    return ['len(' + text + ')', Blockly.NaoLang.ORDER_FUNCTION_CALL];
};

// Loop functions
Blockly.NaoLang.controls_repeat_ext = function (block) {
    if (block.getField('TIMES')) {
        var repeats = String(parseInt(block.getFieldValue('TIMES'), 10));
    } else {
        var repeats = Blockly.NaoLang.valueToCode(block, 'TIMES', Blockly.NaoLang.ORDER_NONE) || '0';
    }
    Blockly.NaoLang.prefixLevel++;
    var branch = Blockly.NaoLang.statementToCode(block, 'DO'),
        code = 'loop(' + repeats + '){\n' + branch;
    Blockly.NaoLang.prefixLevel--;
    return Blockly.NaoLang.generatePrefix() + code +
        Blockly.NaoLang.generatePrefix() + '}\n';
};

Blockly.NaoLang['controls_whileUntil'] = function (block) {
    Blockly.NaoLang.prefixLevel++;
    var until = block.getFieldValue('MODE') == 'UNTIL',
        argument0 = Blockly.NaoLang.valueToCode(block, 'BOOL', until ? Blockly.NaoLang.ORDER_LOGICAL_NOT : Blockly.NaoLang.ORDER_NONE) || 'FALSE',
        branch = Blockly.NaoLang.statementToCode(block, 'DO');
    Blockly.NaoLang.prefixLevel--;
    if (until) {
        argument0 = 'not(' + argument0 + ')';
    }
    return Blockly.NaoLang.generatePrefix() + 'while(' + argument0 + '){\n' + branch +
        Blockly.NaoLang.generatePrefix() + '}\n';
};

// Logical functions
Blockly.NaoLang.controls_if = function (block) {
    var n = 0,
        code = '';
    do {
        Blockly.NaoLang.prefixLevel++;
        var conditionCode = Blockly.NaoLang.valueToCode(block, 'IF' + n, Blockly.NaoLang.ORDER_NONE) || 'FALSE',
            branchCode = Blockly.NaoLang.statementToCode(block, 'DO' + n) || '';
        Blockly.NaoLang.prefixLevel--;
        code += (n == 0 ? 'if' : 'elseif') + '(' + conditionCode + '){\n' + branchCode + Blockly.NaoLang.generatePrefix() + '}\n';
        ++n;
    } while (block.getInput('IF' + n));

    if (block.getInput('ELSE')) {
        Blockly.NaoLang.prefixLevel++;
        var branchCode = Blockly.NaoLang.statementToCode(block, 'ELSE') || '';
        Blockly.NaoLang.prefixLevel--;
        code += 'else{\n' + branchCode + Blockly.NaoLang.generatePrefix() + '}\n';
    }
    return Blockly.NaoLang.generatePrefix() + code;
};

Blockly.NaoLang['logic_boolean'] = function (block) {
    var code = (block.getFieldValue('BOOL') == 'TRUE') ? 'TRUE' : 'FALSE';
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang['logic_negate'] = function (block) {
    var argument0 = Blockly.NaoLang.valueToCode(block, 'BOOL', Blockly.NaoLang.ORDER_LOGICAL_NOT) || 'TRUE',
        code = 'not(' + argument0 + ')';
    return [code, Blockly.NaoLang.ORDER_LOGICAL_NOT];
};

Blockly.NaoLang['logic_compare'] = function (block) {
    var OPERATORS = {
        'EQ': 'equal',
        'NEQ': 'notEqual',
        'LT': 'lessThan',
        'LTE': 'lessThanEqual',
        'GT': 'greaterThan',
        'GTE': 'greaterThanEqual'
    };
    var operator = OPERATORS[block.getFieldValue('OP')];
    var order = Blockly.NaoLang.ORDER_RELATIONAL;
    var argument0 = Blockly.NaoLang.valueToCode(block, 'A', order) || '0';
    var argument1 = Blockly.NaoLang.valueToCode(block, 'B', order) || '0';
    var code = operator + '(' + argument0 + ', ' + argument1 + ')';
    return [code, order];
};

Blockly.NaoLang['logic_operation'] = function (block) {
    var OPERATORS = {
        'AND': 'and',
        'OR': 'or'
    };
    var operator = OPERATORS[block.getFieldValue('OP')],
        order = Blockly.NaoLang.ORDER_RELATIONAL,
        argument0 = Blockly.NaoLang.valueToCode(block, 'A', order) || 'FALSE',
        argument1 = Blockly.NaoLang.valueToCode(block, 'B', order) || 'FALSE',
        code = operator + '(' + argument0 + ', ' + argument1 + ')';
    return [code, order];
};

// Variables
Blockly.NaoLang['variables_get'] = function (block) {
    var code = '@' + Blockly.NaoLang.variableDB_.getName(block.getFieldValue('VAR'), Blockly.Variables.NAME_TYPE);
    return [code, Blockly.NaoLang.ORDER_ATOMIC];
};

Blockly.NaoLang['variables_set'] = function (block) {
    var argument0 = Blockly.NaoLang.valueToCode(block, 'VALUE', Blockly.NaoLang.ORDER_NONE) || '0',
        varName = Blockly.NaoLang.variableDB_.getName(block.getFieldValue('VAR'), Blockly.Variables.NAME_TYPE);
    return Blockly.NaoLang.generatePrefix() + 'variable(@' + varName + ', ' + argument0 + ')\n';
};