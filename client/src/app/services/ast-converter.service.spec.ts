import { TestBed } from '@angular/core/testing';

import { AstConverterService } from './ast-converter.service';

// There is currently no easy way to genrate these tests. The current process is:
// 1. Build a program in the editor
// 2. Save the program - a rough version of the expected XML is logged in the console
// 3. Load the program - the raw JSON is in the network panel, make sure to only include the output array
// 4. Run the test and modify the extected XML until it matches
// While not perfect this will get close enough to the expected and raw values that we (as humans) can figure out what things should be!
// Main points to change:
// * change namespace  https://developers.google.com/blockly/xml to http://www.w3.org/1999/xhtml
// * change shadow elements to block elements
// * remove the hash (#) from in front of any colour codes
// * remove any id attributes
// * remove any x and y attributes
// * may need to add a closing element to an closed element (and open it)
describe('AstConverterService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    expect(service).toBeTruthy();
  });

  it("convert changeLEDColour(CHEST, #ff0000)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Constant","value":"CHEST","lineNumber":2,"linePosition":18},"type":"Constant"},{"token":{"type":"Colour","value":"ff0000","lineNumber":2,"linePosition":25},"type":"Constant"}],"token":{"type":"Identifier","value":"changeLEDColour","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_change_chest"><value name="COLOUR"><block type="colour_picker"><field name="COLOUR">ff0000</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert changeLEDColour(BOTH_EYES, #ff0000)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Constant","value":"BOTH_EYES","lineNumber":2,"linePosition":18},"type":"Constant"},{"token":{"type":"Colour","value":"ff0000","lineNumber":2,"linePosition":29},"type":"Constant"}],"token":{"type":"Identifier","value":"changeLEDColour","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_change_eye"><field name="EYE">BOTH_EYES</field><value name="COLOUR"><block type="colour_picker"><field name="COLOUR">ff0000</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert rest()", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"token":{"type":"Identifier","value":"rest","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_rest"></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert wait(1)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Number","value":"1","lineNumber":2,"linePosition":7},"type":"Constant"}],"token":{"type":"Identifier","value":"wait","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_wait"><value name="TIME"><block type="math_number"><field name="NUM">1</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say('abc')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"abc","lineNumber":2,"linePosition":6},"type":"Constant"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="text"><field name="TEXT">abc</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert position('Stand')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"Stand","lineNumber":2,"linePosition":11},"type":"Constant"}],"token":{"type":"Identifier","value":"position","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_posture"><field name="POSTURE">Stand</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert position('Stand', 'abc')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"Stand","lineNumber":2,"linePosition":11},"type":"Constant"},{"token":{"type":"Text","value":"abc","lineNumber":2,"linePosition":20},"type":"Constant"}],"token":{"type":"Identifier","value":"position","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_posture_and_say"><field name="POSTURE">Stand</field><value name="TEXT"><block type="text"><field name="TEXT">abc</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert wave()", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"token":{"type":"Identifier","value":"wave","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_action"><field name="ACTION">wave</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert wave('abc')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"abc","lineNumber":2,"linePosition":7},"type":"Constant"}],"token":{"type":"Identifier","value":"wave","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_action_and_say"><field name="ACTION">wave</field><value name="TEXT"><block type="text"><field name="TEXT">abc</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert wipe_forehead()", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"token":{"type":"Identifier","value":"wipe_forehead","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_action"><field name="ACTION">wipe_forehead</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert wipe_forehead('abc')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"abc","lineNumber":2,"linePosition":16},"type":"Constant"}],"token":{"type":"Identifier","value":"wipe_forehead","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_action_and_say"><field name="ACTION">wipe_forehead</field><value name="TEXT"><block type="text"><field name="TEXT">abc</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert look('left')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"left","lineNumber":2,"linePosition":7},"type":"Constant"}],"token":{"type":"Identifier","value":"look","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_look"><field name="DIR">LEFT</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert changeHand('open','left')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"open","lineNumber":2,"linePosition":13},"type":"Constant"},{"token":{"type":"Text","value":"left","lineNumber":2,"linePosition":20},"type":"Constant"}],"token":{"type":"Identifier","value":"changeHand","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_hand"><field name="ACTION">open</field><field name="HAND">LEFT</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert point('left','out')", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"left","lineNumber":2,"linePosition":8},"type":"Constant"},{"token":{"type":"Text","value":"out","lineNumber":2,"linePosition":15},"type":"Constant"}],"token":{"type":"Identifier","value":"point","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_point"><field name="ARM">LEFT</field><field name="DIR">OUT</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert turn(0)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Number","value":"0","lineNumber":2,"linePosition":7},"type":"Constant"}],"token":{"type":"Identifier","value":"turn","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_turn"><value name="ANGLE"><block type="math_number"><field name="NUM">0</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert walk(1,0)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Number","value":"1","lineNumber":2,"linePosition":7},"type":"Constant"},{"token":{"type":"Number","value":"0","lineNumber":2,"linePosition":9},"type":"Constant"}],"token":{"type":"Identifier","value":"walk","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_walk"><value name="X"><block type="math_number"><field name="NUM">1</field></block></value><value name="Y"><block type="math_number"><field name="NUM">0</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert loop(3){\nsay('Hello')\n}", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Number","value":"3","lineNumber":2,"linePosition":7},"type":"Constant"}],"children":[{"arguments":[{"token":{"type":"Text","value":"Hello","lineNumber":3,"linePosition":10},"type":"Constant"}],"token":{"type":"Identifier","value":"say","lineNumber":3,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"loop","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":6,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="controls_repeat_ext"><value name="TIMES"><block type="math_number"><field name="NUM">3</field></block></value><statement name="DO"><block type="robot_say"><value name="TEXT"><block type="text"><field name="TEXT">Hello</field></block></value></block></statement></block></xml>';
    expect(xmlText).toBe(expected);
  });

  // it("convert while(TRUE){\nsay((len('abc')))\n}", () => {
  //   const service: AstConverterService = TestBed.get(AstConverterService);
  //   const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Boolean","value":"TRUE","lineNumber":2,"linePosition":8},"type":"Constant"}],"children":[{"arguments":[{"arguments":[{"token":{"type":"Text","value":"abc","lineNumber":3,"linePosition":15},"type":"Constant"}],"token":{"type":"Identifier","value":"len","lineNumber":3,"linePosition":11},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":3,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"while","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":6,"linePosition":0},"type":"Function"}]');
  //   const xml = service.convert(json, false);
  //   const xmlText = new XMLSerializer().serializeToString(xml);
  //   const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="controls_whileUntil"><field name="MODE">WHILE</field><value name="BOOL"><block type="logic_boolean"><field name="BOOL">TRUE</field></block></value><statement name="DO"><block type="robot_say"><value name="TEXT"><block type="text"><field name="TEXT">abc</field></block><block type="text_length"><value name="VALUE"><block type="text"><field name="TEXT">abc</field></block></value></block></value></block></statement></block></xml>';
  //   expect(xmlText).toBe(expected);
  // });

  // it("convert while(not(isEven(0))){\nposition('Stand')\n}", () => {
  //   const service: AstConverterService = TestBed.get(AstConverterService);
  //   const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"arguments":[{"token":{"type":"Number","value":"0","lineNumber":2,"linePosition":19},"type":"Constant"}],"token":{"type":"Identifier","value":"isEven","lineNumber":2,"linePosition":12},"type":"Function"}],"token":{"type":"Identifier","value":"not","lineNumber":2,"linePosition":8},"type":"Function"}],"children":[{"arguments":[{"token":{"type":"Text","value":"Stand","lineNumber":3,"linePosition":15},"type":"Constant"}],"token":{"type":"Identifier","value":"position","lineNumber":3,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"while","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":6,"linePosition":0},"type":"Function"}]');
  //   const xml = service.convert(json, false);
  //   const xmlText = new XMLSerializer().serializeToString(xml);
  //   const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="controls_whileUntil"><field name="MODE">UNTIL</field><value name="BOOL"><block type="math_number_property"><mutation divisor_input="false"/><field name="PROPERTY">EVEN</field><value name="NUMBER_TO_CHECK"><block type="math_number"><field name="NUM">0</field></block></value></block></value><statement name="DO"><block type="robot_posture"><field name="POSTURE">Stand</field></block></statement></block></xml>';
  //   expect(xmlText).toBe(expected);
  // });

  it("convert changeLEDColour(CHEST, (randomColour()))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Constant","value":"CHEST","lineNumber":2,"linePosition":18},"type":"Constant"},{"token":{"type":"Identifier","value":"randomColour","lineNumber":2,"linePosition":26},"type":"Function"}],"token":{"type":"Identifier","value":"changeLEDColour","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_change_chest"><value name="COLOUR"><block type="colour_random"></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((len('abc')))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Text","value":"abc","lineNumber":2,"linePosition":11},"type":"Constant"}],"token":{"type":"Identifier","value":"len","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="text_length"><value name="VALUE"><block type="text"><field name="TEXT">abc</field></block></value></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((append('Hello','world')))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Text","value":"Hello","lineNumber":2,"linePosition":14},"type":"Constant"},{"token":{"type":"Text","value":"world","lineNumber":2,"linePosition":22},"type":"Constant"}],"token":{"type":"Identifier","value":"append","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="text_concat"><value name="VAR"><block type="text"><field name="TEXT">Hello</field></block></value><value name="TEXT"><block type="text"><field name="TEXT">world</field></block></value></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((round(3.1)))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Number","value":"3.1","lineNumber":2,"linePosition":13},"type":"Constant"}],"token":{"type":"Identifier","value":"round","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="math_round"><field name="OP">ROUND</field><value name="NUM"><block type="math_number"><field name="NUM">3.1</field></block></value></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((isEven(0)))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Number","value":"0","lineNumber":2,"linePosition":14},"type":"Constant"}],"token":{"type":"Identifier","value":"isEven","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="math_number_property"><value name="NUMBER_TO_CHECK"><block type="math_number"><field name="NUM">0</field></block></value><field name="PROPERTY">EVEN</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((isOdd(0)))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Number","value":"0","lineNumber":2,"linePosition":14},"type":"Constant"}],"token":{"type":"Identifier","value":"isOdd","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="math_number_property"><value name="NUMBER_TO_CHECK"><block type="math_number"><field name="NUM">0</field></block></value><field name="PROPERTY">ODD</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((isPrime(0)))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Number","value":"0","lineNumber":2,"linePosition":14},"type":"Constant"}],"token":{"type":"Identifier","value":"isPrime","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="math_number_property"><value name="NUMBER_TO_CHECK"><block type="math_number"><field name="NUM">0</field></block></value><field name="PROPERTY">PRIME</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert dance('gangnam', TRUE)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"gangnam","lineNumber":2,"linePosition":8},"type":"Constant"},{"token":{"type":"Boolean","value":"TRUE","lineNumber":2,"linePosition":19},"type":"Constant"}],"token":{"type":"Identifier","value":"dance","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_gangnam"><field name="MUSIC">TRUE</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert dance('taichi', FALSE)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Text","value":"taichi","lineNumber":2,"linePosition":8},"type":"Constant"},{"token":{"type":"Boolean","value":"FALSE","lineNumber":2,"linePosition":18},"type":"Constant"}],"token":{"type":"Identifier","value":"dance","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_taichi"><field name="MUSIC">FALSE</field></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say(readSensor(HEAD_FRONT))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Constant","value":"HEAD_FRONT","lineNumber":2,"linePosition":17},"type":"Constant"}],"token":{"type":"Identifier","value":"readSensor","lineNumber":2,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="robot_sensor_head"><field name="SENSOR">FRONT</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say(readSensor(BATTERY))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Constant","value":"BATTERY","lineNumber":2,"linePosition":17},"type":"Constant"}],"token":{"type":"Identifier","value":"readSensor","lineNumber":2,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="robot_sensor_battery"></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say(readSensor(SONAR_LEFT))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Constant","value":"SONAR_LEFT","lineNumber":2,"linePosition":17},"type":"Constant"}],"token":{"type":"Identifier","value":"readSensor","lineNumber":2,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="robot_sensor_sonar"><field name="SENSOR">LEFT</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say(readSensor(GYROSCOPE_X))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Constant","value":"GYROSCOPE_X","lineNumber":2,"linePosition":17},"type":"Constant"}],"token":{"type":"Identifier","value":"readSensor","lineNumber":2,"linePosition":6},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="robot_sensor_gyroscope"><field name="SENSOR">X</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say((randomInt(1, 100)))", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"arguments":[{"token":{"type":"Number","value":"1","lineNumber":2,"linePosition":17},"type":"Constant"},{"token":{"type":"Number","value":"100","lineNumber":2,"linePosition":20},"type":"Constant"}],"token":{"type":"Identifier","value":"randomInt","lineNumber":2,"linePosition":7},"type":"Function"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><block type="robot_say"><value name="TEXT"><block type="math_random_int"><value name="FROM"><block type="math_number"><field name="NUM">1</field></block></value><value name="TO"><block type="math_number"><field name="NUM">100</field></block></value></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });

  it("convert say(@test)", () => {
    const service: AstConverterService = TestBed.get(AstConverterService);
    const json = JSON.parse('[{"token":{"type":"Identifier","value":"reset","lineNumber":0,"linePosition":0},"type":"Function"},{"children":[{"arguments":[{"token":{"type":"Variable","value":"test","lineNumber":2,"linePosition":6},"type":"Variable"}],"token":{"type":"Identifier","value":"say","lineNumber":2,"linePosition":2},"type":"Function"}],"token":{"type":"Identifier","value":"start","lineNumber":1,"linePosition":0},"type":"Function"},{"token":{"type":"Identifier","value":"go","lineNumber":4,"linePosition":0},"type":"Function"}]');
    const xml = service.convert(json, false);
    const xmlText = new XMLSerializer().serializeToString(xml);
    const expected = '<xml xmlns="http://www.w3.org/1999/xhtml"><variables><variable id="test">test</variable></variables><block type="robot_say"><value name="TEXT"><block type="variables_get"><field name="VAR" id="test">test</field></block></value></block></xml>';
    expect(xmlText).toBe(expected);
  });
});
