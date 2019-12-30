# Robot Functonality/Blocks

This page lists all the blocks that are available and their current stage of testing.

## Blocks

| Category | Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-|-
| *Robot Actions* | Change Chest To `[colour]` | changeLEDColour(CHEST, `[colour]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Change `{eye}` eye(s) to `[colour]` | changeLEDColour(`{BOTH_EYES,LEFT_EYE,RIGHT_EYE}`, `[colour]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Rest | rest() | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Wait for `[number]` s | wait(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Say `[text]` | say(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| *Robot Movements* | Move to `{position}` | position(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Move to `{position}` and say `[text]` | position(`[text]`, `[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Do `{wave}` | wave() | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Do `{wave}` and `[text]` | wave(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Look `{direction}` | look('`{left|right,ahead}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | `{Open/close}` `{side}` hand(s) | changeHand('`{open,close}`','`{left,right,both}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Point `{side}` arm `{direction}` | point('`{left,right|`','`{up,down,out,ahead}`'}) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Turn `[number]` degrees | turn(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| *Logic* | if `<condition>` do `<action>` | | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | `<value>` `{equality}` `<value>` | | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | `<value>` `{logic condition}` `<value>` | | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| |  not `<value>` | | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| | `{true/false}` | | :heavy_check_mark: Working | :heavy_check_mark: Working | 
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
