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
});