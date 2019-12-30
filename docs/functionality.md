# Robot Functonality/Blocks

This page lists all the blocks that are available and their current stage of testing.

## Blocks

| Category | Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-|-
| *Robot Actions* | Change Chest To `[colour]` | changeLEDColour(CHEST, `[colour]`) | :heavy_check_mark: Working | Working | 
| | Change `{eye}` eye(s) to `[colour]` | changeLEDColour(`{BOTH_EYES|LEFT_EYE|RIGHT_EYE}`, `[colour]`) | Working | Working | 
| | Rest | rest() | Working | Working | 
| | Wait for `[number]` s | wait(`[number]`) | Working | Working | 
| | Say `[text]` | say(`[text]`) | Working | Working | 
| *Robot Movements* | Move to `{position}` | position(`[text]`) | Working | Working | 
| | Move to `{position}` and say `[text]` | position(`[text]`, `[text]`) | Working | Working | 
| | Do `{wave}` | wave() | Working | Working | 
| | Do `{wave}` and `[text]` | wave(`[text]`) | Working | Working | 
| | Look `{direction}` | look('`{left|right|ahead}`') | Working | Working | 
| | `{Open/close}` `{side}` hand(s) | changeHand('`{open|close}`','`{left|right|both}`') | Working | Working | 
| | Point `{side}` arm `{direction}` | point('`{left|right|`','`{up|down|out|ahead}`'}) | Working | Working | 
| | Turn `[number]` degrees | turn(`[number]`) | Working | Working | 
| | Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | Working | Working | 
| *Logic* | if `<condition>` do `<action>` | | Working | Working | 
| | `<value>` `{equality}` `<value>` | | Working | Working | 
| | `<value>` `{logic condition}` `<value>` | | Working | Working | 
| |  not `<value>` | | Working | Working | 
| | `{true/false}` | | Working | Working | 
| *Loops* | Repeat `[number]` time do `<action>` | | | | 
| | Repeat `{while}` `<condition>` do `<action>` | | | | 
| | Repeat `{until}` `<condition>` do `<action>` | | | | 
| *Maths* | `[number]` | | | | 
| | `[number]` `{operation}` `[number]` | | | | 
| | `[number]` `{odd/even}` | | | | 
| | Round `<value>` | | | | 
| | Random integer from `[number]` to `[number]` | | | | 
| *Text* | "`[text]`" | | | | 
| | append `[text]` to `[text]` | | | | 
| | length of `<value>` | | | | 
| *Colour* | `[colour]` | | | | 
| | Random colour | | | | 
| *Variables* | Set `{variable}` to `<value>' | | | | 
| | Change `{variable}` by `<value>` | | | | 
| | `{variable}` | | | | 
