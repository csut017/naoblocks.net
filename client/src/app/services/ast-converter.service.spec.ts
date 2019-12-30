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

  it("convert changeLEDColour(CHEST, #ff0000)", () => {
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
});
