import { Injectable } from '@angular/core';
import { RoboLangAstNode } from '../data/robo-lang-ast-node';

declare var Blockly: any;

export enum AstConversionMode {
  Default,
  Simplified,
  Tangible
}

@Injectable({
  providedIn: 'root'
})
export class AstConverterService {
  private xmlBlocks: { [id: string]: IBlockDefinition } = {
    'append': new BlockDefinition('text_concat', [this.generateValue('VAR'), this.generateValue('TEXT')]),
    'and': new BlockDefinition('logic_operation', [this.generateField('OP', 'AND'), this.generateValue('A'), this.generateValue('B')]),
    'changeHand': new BlockDefinition('robot_hand', [this.generateField('ACTION'), this.generateField('HAND', this.toUpper)]),
    'changeLEDColour': new FunctionBlockDefinition(this.generateLedBlockDefinition()),
    'dance': new FunctionBlockDefinition(this.generateDanceBlock()),
    'equal': new BlockDefinition('logic_compare', [this.generateField('OP', 'EQ'), this.generateValue('A'), this.generateValue('B')]),
    'if': new FunctionBlockDefinition(this.generateIfBlock),
    'isEven': new BlockDefinition('math_number_property', [this.generateValue('NUMBER_TO_CHECK'), this.generateField('PROPERTY', 'EVEN')]),
    'isOdd': new BlockDefinition('math_number_property', [this.generateValue('NUMBER_TO_CHECK'), this.generateField('PROPERTY', 'ODD')]),
    'isPrime': new BlockDefinition('math_number_property', [this.generateValue('NUMBER_TO_CHECK'), this.generateField('PROPERTY', 'PRIME')]),
    'len': new BlockDefinition('text_length', this.generateValue('VALUE')),
    'lessThan': new BlockDefinition('logic_compare', [this.generateField('OP', 'LT'), this.generateValue('A'), this.generateValue('B')]),
    'look': new BlockDefinition('robot_look', [this.generateField('DIR', this.toUpper)]),
    'loop': new BlockDefinition('controls_repeat_ext', this.generateValue('TIMES'), 'DO'),
    'not': new BlockDefinition('logic_negate', this.generateValue('BOOL')),
    'notEqual': new BlockDefinition('logic_compare', [this.generateField('OP', 'NEQ'), this.generateValue('A'), this.generateValue('B')]),
    'or': new BlockDefinition('logic_operation', [this.generateField('OP', 'OR'), this.generateValue('A'), this.generateValue('B')]),
    'point': new BlockDefinition('robot_point', [this.generateField('ARM', this.toUpper), this.generateField('DIR', this.toUpper)]),
    'position': new FunctionBlockDefinition(this.generatePositionBlockDefinition()),
    'randomColour': new BlockDefinition('colour_random'),
    'randomInt': new BlockDefinition('math_random_int', [this.generateValue('FROM'), this.generateValue('TO')]),
    'readSensor': new FunctionBlockDefinition(this.generateSensorBlockDefinition()),
    'rest': new BlockDefinition('robot_rest'),
    'round': new BlockDefinition('math_round', [this.generateField('OP', 'ROUND'), this.generateValue('NUM')]),
    'say': new BlockDefinition('robot_say', this.generateValue('TEXT')),
    'moveForward': new BlockDefinition('robot_move', this.generateValue('SECONDS')),
    'turn': new BlockDefinition('robot_turn', [this.generateField('DIR'), this.generateValue('SECONDS')]),
    'variable': new BlockDefinition('variables_set', [this.generateField('VAR'), this.generateValue('VALUE')]),
    'walk': new BlockDefinition('robot_walk', [this.generateValue('X'), this.generateValue('Y')]),
    'wait': new BlockDefinition('robot_wait', this.generateValue('TIME')),
    'wave': new FunctionBlockDefinition(this.generateActionDefinition('wave')),
    'while': new BlockDefinition('controls_repeat_ext', this.generateValue('TIMES'), 'DO'),
    'wipe_forehead': new FunctionBlockDefinition(this.generateActionDefinition('wipe_forehead'))
  };

  private valueBlocks: { [id: string]: string[] } = {
    'Boolean': ['logic_boolean', 'BOOL'],
    'Number': ['math_number', 'NUM'],
    'Text': ['text', 'TEXT'],
    'Colour': ['colour_picker', 'COLOUR']
  };

  private mode: AstConversionMode = AstConversionMode.Default;

  constructor() { }

  convert(ast: RoboLangAstNode[], requireEvents: boolean, mode: AstConversionMode): HTMLElement {
    let xml: HTMLElement = document.createElement('xml'),
      environ: GeneratorEnvironment = new GeneratorEnvironment(),
      block: HTMLElement | undefined;
    this.mode = mode;

    if (requireEvents) {
      ast.shift();
      ast.pop();
      block = this.generateBlock(ast, environ);
    } else {
      block = this.generateBlock(ast[1].children, environ);
    }

    if (environ.hasVariables()) {
      xml.appendChild(environ.generateVariablesDefinition());
    }
    if (block) xml.appendChild(block);
    return xml;
  }

  private generateDanceBlock(): nodeGenerator {
    const that = this;
    return function (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment): void {
      var dfn = new BlockDefinition(
        'robot_' + elem.arguments[0].token?.value,
        that.generateField('MUSIC', elem.arguments[1].token?.value));
      dfn.populate(node, elem, environ);
    }
  }

  private generateIfBlock(elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment): void {
    node.setAttribute('type', 'controls_if');
    var ifCount = 0,
      elseCount = 0;
    for (var loop = 0; loop < elem.children.length; loop++) {
      var conditionElem = elem.children[loop];
      switch (conditionElem.token?.value) {
        case 'if':
          this.generateValue('IF0')(conditionElem, node, environ, 0);
          if (conditionElem.children) this.generateStatementBlock(node, 'DO0', conditionElem.children, environ);
          break;

        case 'elseif':
          ifCount++;
          this.generateValue('IF' + ifCount)(conditionElem, node, environ, 0);
          if (conditionElem.children) this.generateStatementBlock(node, 'DO' + ifCount, conditionElem.children, environ);
          break;

        case 'else':
          if (conditionElem.children) this.generateStatementBlock(node, 'ELSE', conditionElem.children, environ);
          elseCount++;
          break;
      }
    }
    if (ifCount || elseCount) {
      var mutationBlock = document.createElement('mutation');
      if (ifCount) mutationBlock.setAttribute('elseif', ifCount.toString());
      if (elseCount) mutationBlock.setAttribute('else', elseCount.toString());
      node.insertBefore(mutationBlock, node.firstChild);
    }
  }

  private generateSensorBlockDefinition(): nodeGenerator {
    var battery = new BlockDefinition('robot_sensor_battery'),
      gyroscope = new BlockDefinition('robot_sensor_gyroscope', this.generateField('SENSOR', this.stripPrefix('GYROSCOPE_'))),
      sonar = new BlockDefinition('robot_sensor_sonar', this.generateField('SENSOR', this.stripPrefix('SONAR_'))),
      head = new BlockDefinition('robot_sensor_head', this.generateField('SENSOR', this.stripPrefix('HEAD_')));
    return function (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment): void {
      var sensorType = elem.arguments[0].token?.value || '';
      if (sensorType == 'BATTERY') {
        battery.populate(node, elem, environ);
      } else if (sensorType.startsWith('GYROSCOPE_')) {
        gyroscope.populate(node, elem, environ);
      } else if (sensorType.startsWith('SONAR_')) {
        sonar.populate(node, elem, environ);
      } else if (sensorType.startsWith('HEAD_')) {
        head.populate(node, elem, environ);
      } else {
        throw 'Unknown sensor type: ' + sensorType;
      }
    };
  }

  private generateLedBlockDefinition(): nodeGenerator {
    var chest = new BlockDefinition('robot_change_chest', this.generateValue('COLOUR', 1)),
      eye = new BlockDefinition('robot_change_eye', [this.generateField('EYE'), this.generateValue('COLOUR')]);
    return function (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment): void {
      if (elem.arguments[0].token?.value == 'CHEST') {
        chest.populate(node, elem, environ);
      } else {
        eye.populate(node, elem, environ);
      }
    };
  }

  private generatePositionBlockDefinition(): nodeGenerator {
    var def = new BlockDefinition('robot_posture', this.generateField('POSTURE')),
      say = new BlockDefinition('robot_posture_and_say',
        [this.generateField('POSTURE'),
        this.generateValue('TEXT')]);
    let simpleBlocks: { [id: string]: IBlockDefinition } = {};
    Blockly.NaoLang.Postures.forEach(function (posture: string) {
      let blockDef = new BlockDefinition('robot_posture_' + posture[1]);
      simpleBlocks[posture[1]] = blockDef;
    });
    return (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment): void => {
      if (this.mode == AstConversionMode.Default) {
        if (elem.arguments && elem.arguments.length && (elem.arguments.length > 1)) {
          say.populate(node, elem, environ);
        } else {
          def.populate(node, elem, environ);
        }
      } else {
        let posture = elem.arguments[0].token?.value,
          blockDef = simpleBlocks[posture || ''];
        blockDef.populate(node, elem, environ);
      }
    };
  }

  private generateActionDefinition(action: string): nodeGenerator {
    var def = new BlockDefinition('robot_action', this.generateField('ACTION', action)),
      say = new BlockDefinition('robot_action_and_say',
        [this.generateField('ACTION', action),
        this.generateValue('TEXT')]);
    let simpleBlocks: { [id: string]: IBlockDefinition } = {};
    Blockly.NaoLang.Actions.forEach(function (action: any) {
      let blockDef = new BlockDefinition('robot_action_' + action[1]);
      simpleBlocks[action[1]] = blockDef;
    });
    return  (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment): void => {
      if (this.mode == AstConversionMode.Default) {
        if (elem.arguments && elem.arguments.length) {
          say.populate(node, elem, environ);
        } else {
          def.populate(node, elem, environ);
        }
      } else {
        let action = elem.token?.value,
          blockDef = simpleBlocks[action || ''];
        blockDef.populate(node, elem, environ);
      }
    };
  }

  private generateField(name: string, valueOrFunc?: any): nodeConverter {
    return function (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment, loop: number): number {
      var child = document.createElement('field'),
        argPos = 0;
      child.setAttribute('name', name);
      if (valueOrFunc && !(valueOrFunc instanceof Function)) {
        child.appendChild(document.createTextNode(valueOrFunc));
      } else {
        var arg = elem.arguments[loop].token,
          argValue = arg?.value || '';
        if (arg?.type == 'Variable') {
          var variableDefn = environ.getOrAddVariable(argValue);
          child.setAttribute('id', variableDefn.id);
        } else {
          if (valueOrFunc) {
            argValue = valueOrFunc(argValue);
          }
        }
        child.appendChild(document.createTextNode(argValue));
        argPos = 1;
      }
      node.appendChild(child);
      return argPos;
    };
  }

  private generateValueLiteral(arg: RoboLangAstNode): HTMLElement {
    var shadow = document.createElement('block'),
      field = document.createElement('field'),
      defn = this.valueBlocks[arg.token?.type || ''];
    if (!defn) throw 'Unable to find definition for ' + arg.token?.type;
    shadow.setAttribute('type', defn[0]);
    if (arg.sourceId) shadow.setAttribute('id', arg.sourceId);
    shadow.appendChild(field);

    field.setAttribute('name', defn[1]);
    field.appendChild(document.createTextNode(arg.token?.value || ''));

    return shadow;
  }

  private generateValue(name: string, offset?: number): nodeConverter {
    let that = this;
    return function (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment, loop: number): number {
      var child = document.createElement('value'),
        arg = elem.arguments[loop + (offset || 0)];

      child.setAttribute('name', name);
      node.appendChild(child);

      switch (arg.type) {
        case 'Constant':
          child.appendChild(that.generateValueLiteral(arg));
          break;

        case 'Variable':
          child.appendChild(that.generateVariableGet(arg, environ));
          break;

        case 'Function':
          let block = that.generateBlock([arg], environ);
          if (block) child.appendChild(block);
          break;

        default:
          throw 'Unknown value type ' + arg.type;
      }

      return 1;
    };
  }

  private generateVariableGet(arg: RoboLangAstNode, environ: GeneratorEnvironment): HTMLElement {
    var shadow = document.createElement('block'),
      field = document.createElement('field');
    shadow.setAttribute('type', 'variables_get');
    if (arg.sourceId) shadow.setAttribute('id', arg.sourceId);
    shadow.appendChild(field);

    field.setAttribute('name', 'VAR');
    var varName = arg.token?.value || '',
      variableDefn = environ.getOrAddVariable(varName);
    field.setAttribute('id', variableDefn.id);
    field.appendChild(document.createTextNode(varName));

    return shadow;
  }


  private generateBlock(ast: RoboLangAstNode[], environ: GeneratorEnvironment): HTMLElement | undefined {
    var block: HTMLElement | undefined;
    console.groupCollapsed('Converting AST block');
    try {
      for (var pos = ast.length - 1; pos >= 0; pos--) {
        let elem = ast[pos],
          next = document.createElement('block'),
          xmlBlock: IBlockDefinition | undefined;

        try {
          xmlBlock = this.xmlBlocks[elem.token?.value || ''];
        } catch (err) {
          console.log('Unknown block type: ' + elem.token?.value);
          console.error(err);
        }

        console.log('Processing ' + elem.token?.value + ' [' + pos + ']');
        if (!xmlBlock) throw 'Unable to find block for ' + elem.token?.value;
        xmlBlock.populate(next, elem, environ);
        if (elem.sourceId) next.setAttribute('id', elem.sourceId);
        if (elem.children && xmlBlock.statementName) {
          this.generateStatementBlock(next, xmlBlock.statementName, elem.children, environ);
        }
        if (block) {
          var nextBlock = document.createElement('next');
          nextBlock.appendChild(block);
          next.appendChild(nextBlock);
        }
        block = next;
      }
    } finally {
      console.groupEnd()
    }

    return block;
  }

  private generateStatementBlock(next: HTMLElement, name: string, children: RoboLangAstNode[], environ: GeneratorEnvironment) {
    var statementBlock = document.createElement('statement');
    statementBlock.setAttribute('name', name);
    var blockChildren = this.generateBlock(children, environ);
    if (blockChildren) {
      statementBlock.appendChild(blockChildren);
      next.append(statementBlock);
    }
  }

  private toUpper(value: string): string {
    return value.toUpperCase();
  }

  private stripPrefix(prefix: string) {
    return function (value: string): string {
      return value.startsWith(prefix)
        ? value.substring(prefix.length)
        : value;
    }
  }
}

class GeneratorEnvironment {
  variables: any[] = [];
  variableMap: { [id: string]: any } = {};

  getOrAddVariable(name: string): VariableDefinition {
    var existing = this.variableMap[name];
    if (!existing) {
      existing = new VariableDefinition(name);
      this.variables.push(existing);
      this.variableMap[name] = existing;
    }
    return existing;
  }

  hasVariables(): boolean {
    return !!this.variables.length;
  }

  generateVariablesDefinition(): HTMLElement {
    var varTable = document.createElement('variables');
    for (const newVar of this.variables) {
      var varItem = document.createElement('variable');
      varItem.setAttribute('id', newVar.id);
      varItem.appendChild(document.createTextNode(newVar.name));
      varTable.appendChild(varItem);
    }
    return varTable;
  }

}

class VariableDefinition {
  name: string;
  id: string;

  constructor(name: string) {
    this.name = name;
    this.id = name;
  }
}

interface IBlockDefinition {
  statementName: string;
  populate(next: HTMLElement, elem: RoboLangAstNode, environ: GeneratorEnvironment): void;
}

class BlockDefinition implements IBlockDefinition {
  name: string;
  defineChildren: any;
  statementName: string;

  constructor(name: string, defineChildren?: any, statementName?: string) {
    this.name = name;
    this.defineChildren = defineChildren;
    this.statementName = statementName || '';
  }

  populate(next: HTMLElement, elem: RoboLangAstNode, environ: GeneratorEnvironment): void {
    next.setAttribute('type', this.name);
    if (this.defineChildren) {
      if (typeof (this.defineChildren) === 'function') {
        this.defineChildren(elem, next, environ, 0);
      } else {
        var pos = 0;
        for (var loop = 0; loop < this.defineChildren.length; loop++) {
          var defineChild = this.defineChildren[loop]
          pos += defineChild(elem, next, environ, pos);
        }
      }
    }
  }
}

class FunctionBlockDefinition implements IBlockDefinition {
  statementName: string = '';
  private func: nodeGenerator;

  constructor(func: nodeGenerator) {
    this.func = func;
  }

  populate(next: HTMLElement, elem: RoboLangAstNode, environ: GeneratorEnvironment): void {
    this.func(elem, next, environ);
  }
}

type nodeConverter = (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment, loop: number) => number;
type nodeGenerator = (elem: RoboLangAstNode, node: HTMLElement, environ: GeneratorEnvironment) => void;