export interface ToolboxDefinition {
    modes: string[];
    style: string;
}

export class Toolbox {
    private eventBlocks: string = '<category name="Robot Events" colour="290">'+
            '<block type="robot_on_start"></block>'+
            '<block type="robot_on_button"></block>'+
        '</category>';
    private postureBlocks = '<category name="Robot Postures" colour="65">'+
            '<block type="robot_posture_Stand"></block>'+
            '<block type="robot_posture_Sit"></block>'+
            '<block type="robot_posture_SitRelax"></block>'+
            '<block type="robot_posture_Crouch"></block>'+
            '<block type="robot_posture_LyingBelly"></block>'+
            '<block type="robot_posture_LyingBack"></block>'+
        '</category>'
    private simpleActionBlocks = '<category name="Robot Actions" colour="65">'+
            '<block type="robot_say">'+
                '<value name="TEXT"><shadow type="text"><field name="TEXT">abc</field></shadow></value>'+
            '</block>'+
            '<block type="robot_wait">'+
                '<value name="TIME"><shadow type="math_number"><field name="NUM">1</field></shadow></value>'+
            '</block>'+
            '<block type="robot_action_wave"></block>'+
            '<block type="robot_action_wipe_forehead"></block>'+
        '</category>';
    private robotActionBlocks = '<category name="Robot Actions" colour="65">'+
            '<block type="robot_change_chest">'+
                '<value name="COLOUR"><shadow type="colour_picker"><field name="COLOUR">#ff0000</field></shadow></value>'+
            '</block>'+
            '<block type="robot_change_eye">'+
                '<value name="COLOUR"><shadow type="colour_picker"><field name="COLOUR">#ff0000</field></shadow></value>'+
            '</block>'+
            '<block type="robot_rest"></block>'+
            '<block type="robot_wait">'+
                '<value name="TIME"><shadow type="math_number"><field name="NUM">1</field></shadow></value>'+
            '</block>'+
            '<block type="robot_say">'+
                '<value name="TEXT"><shadow type="text"><field name="TEXT">abc</field></shadow></value>'+
            '</block>'+
        '</category>';
    private movementBlocks = '<category name="Robot Movements" colour="65">'+
            '<block type="robot_posture"></block>'+
            '<block type="robot_posture_and_say">'+
                '<value name="TEXT"><shadow type="text"><field name="TEXT">abc</field></shadow></value>'+
            '</block>'+
            '<block type="robot_action"></block>'+
            '<block type="robot_action_and_say">'+
                '<value name="TEXT"><shadow type="text"><field name="TEXT">abc</field></shadow></value>'+
            '</block>'+
            '<block type="robot_look"></block>'+
            '<block type="robot_hand"></block>'+
            '<block type="robot_point"></block>'+
            '<block type="robot_turn">'+
                '<value name="ANGLE"><shadow type="math_number"><field name="NUM">0</field></shadow></value>'+
            '</block>'+
            '<block type="robot_walk">'+
                '<value name="X"><shadow type="math_number"><field name="NUM">1</field></shadow></value>'+
                '<value name="Y"><shadow type="math_number"><field name="NUM">0</field></shadow></value>'+
            '</block>'+
        '</category>';
    private danceBlocks = '<category name="Robot Dances" colour="65">'+
            '<block type="robot_gangnam"></block>'+
            '<block type="robot_taichi"></block>'+
        '</category>';
    private sensorBlocks = '<category name="Robot Sensors" colour="85">'+
            '<block type="robot_sensor_head"></block>'+
            '<block type="robot_sensor_battery"></block>'+
            '<block type="robot_sensor_sonar"></block>'+
            '<block type="robot_sensor_gyroscope"></block>'+
        '</category>';
    private logicBlocks = '<category name="Logic" colour="%{BKY_LOGIC_HUE}" mode="Conditionals">'+
            '<block type="controls_if"></block>'+
            '<block type="logic_compare"></block>'+
            '<block type="logic_operation"></block>'+
            '<block type="logic_negate"></block>'+
            '<block type="logic_boolean"></block>'+
        '</category>';
    private loopBlocks = '<category name="Loops" colour="%{BKY_LOOPS_HUE}">'+
            '<block type="controls_repeat_ext">'+
                '<value name="TIMES"><shadow type="math_number"><field name="NUM">10</field></shadow></value>'+
            '</block>'+
            '<block type="controls_whileUntil"></block>'+
        '</category>';
    private mathBlocks = '<category name="Maths" colour="%{BKY_MATH_HUE}">'+
            '<block type="math_number"></block>'+
            '<block type="math_arithmetic">'+
                '<value name="A"><shadow type="math_number"><field name="NUM">1</field></shadow></value>'+
                '<value name="B"><shadow type="math_number"><field name="NUM">1</field></shadow></value>'+
            '</block>'+
            '<block type="math_number_property">'+
                '<value name="NUMBER_TO_CHECK"><shadow type="math_number"><field name="NUM">0</field></shadow></value>'+
            '</block>'+
            '<block type="math_round">'+
                '<value name="NUM"><shadow type="math_number"><field name="NUM">3.1</field></shadow></value>'+
            '</block>'+
            '<block type="math_random_int">'+
                '<value name="FROM"><shadow type="math_number"><field name="NUM">1</field></shadow></value>'+
                '<value name="TO"><shadow type="math_number"><field name="NUM">100</field></shadow></value>'+
            '</block>'+
        '</category>';
    private textBlocks = '<category name="Text" colour="%{BKY_TEXTS_HUE}" mode="Variables">'+
            '<block type="text"></block>'+
            '<block type="text_concat">'+
                '<value name="TEXT"><shadow type="text"></shadow></value>'+
            '</block>'+
            '<block type="text_length">'+
                '<value name="VALUE"><shadow type="text"><field name="TEXT">abc</field></shadow></value>'+
            '</block>'+
        '</category>';
    private colourBlocks = '<category name="Colour" colour="%{BKY_COLOUR_HUE}" mode="Variables">'+
            '<block type="colour_picker"></block>'+
            '<block type="colour_random"></block>'+
        '</category>';
    private variableBlocks = '<category name="Variables" colour="%{BKY_VARIABLES_HUE}" custom="VARIABLE" mode="Variables">'+
        '</category>';

    private toolboxConfig = [
        { xml: this.postureBlocks, style: 'simple' },
        { xml: this.simpleActionBlocks, style: 'simple' },
        { xml: this.eventBlocks, mode: 'events', style: 'default' },
        { xml: this.movementBlocks, style: 'default' },
        { xml: this.robotActionBlocks, style: 'default' },
        { xml: this.colourBlocks, style: 'default' },
        { xml: this.danceBlocks, mode: 'dances', style: 'default' },
        { xml: this.sensorBlocks, mode: 'sensors', style: 'default' },
        { xml: this.logicBlocks, mode: 'conditionals', style: 'default' },
        { xml: this.loopBlocks, mode: 'loops', style: 'default' },
        { xml: this.mathBlocks, mode: 'variables', style: 'default' },
        { xml: this.textBlocks, mode: 'variables', style: 'default' },
        { xml: this.variableBlocks, mode: 'variables', style: 'default', seperatorBefore: true },
    ];

    modes: string[] = [];
    style: string = 'default';

    constructor(definition?: ToolboxDefinition) {
        if (definition) {
            if (definition.modes) this.modes = definition.modes;
            if (definition.style) this.style = definition.style;
        }
    }

    useDefaultStyle(): Toolbox {
        this.style = 'default';
        return this;
    }

    useSimpleStyle(): Toolbox {
        this.style = 'simple';
        return this;
    }

    includeEvents(): Toolbox {
        this.modes.push('events');
        return this;
    }

    includeDances(): Toolbox {
        this.modes.push('dances');
        return this;
    }

    includeSensors(): Toolbox {
        this.modes.push('sensors');
        return this;
    }

    includeConditionals(): Toolbox {
        this.modes.push('conditionals');
        return this;
    }

    includeLoops(): Toolbox {
        this.modes.push('loops');
        return this;
    }

    includeVariables(): Toolbox {
        this.modes.push('variables');
        return this;
    }

    build(): string {
        let xml = '<xml>' +
            this.toolboxConfig
                .filter(c => c.style == this.style)
                .map(c => {
                    if (c.mode && !this.modes.includes(c.mode)) return '';
                    return (c.seperatorBefore ? '<sep></sep>' : '') +
                        c.xml;
                })
                .join('') +
            '</xml>';
        return xml;
    }
}
