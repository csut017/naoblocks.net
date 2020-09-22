// Mapping between tag codes and code blocks
var Dict = {
  31: "position('Stand')",
  61: "wave()",
  59: "dance('gangnam', TRUE)",
  55: "say('hello')",
  47: "position('Stand', 'Hello, I am Red One')",
  121: "walk ( 6 , 0 )", // Walks forward for 6 seconds and sideways for 0 seconds
  117: "look('left')",
  109: "look('right')",
  93: "point('left','out')",
  79: "point('right','out')",
};

// Reversed codes
// displayed code: recognised code
// 31: 31
// 47: 61
// 55: 59
// 59: 55
// 61: 47
// 79: 121
// 87: 117
// 91: 109
// 93: 93
// 121: 79

var BlockDict = {
  31: "<img src='img/stand_block.png' />",
  61: "<img src='img/wave_block.png' />",
  59: "<img src='img/dance_block.png' />",
  55: "<img src='img/speak_block.png' />",
  47: "<img src='img/stand_block.png' />",
  61: "<img src='img/rest_block.png' />",
  121: "<img src='img/walk_block.png' />", // Walks forward for 6 seconds and sideways for 0 seconds
  117: "<img src='img/look_left_block.png' />",
  109: "<img src='img/look_right_block.png' />",
  93: "<img src='img/raise_left_arm_block.png' />",
  79: "<img src='img/raise_right_arm_block.png' />",
};

function getValue(key) {
  return Dict[key];
}

function getBlock(key) {
  return BlockDict[key];
}
