# Robot Functonality/Blocks

This page lists all the blocks that are available and their current stage of testing.

## Robot Actions Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Change Chest To `[colour]` | changeLEDColour(CHEST, `[colour]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Change `{eye}` eye(s) to `[colour]` | changeLEDColour(`{BOTH_EYES,LEFT_EYE,RIGHT_EYE}`, `[colour]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Rest | rest() | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Wait for `[number]` s | wait(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Say `[text]` | say(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 

## Robot Movements Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Move to `{position}` | position(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Move to `{position}` and say `[text]` | position(`[text]`, `[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Do `{wave}` | wave() | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Do `{wave}` and `[text]` | wave(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Look `{direction}` | look('`{left|right,ahead}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| `{Open/close}` `{side}` hand(s) | changeHand('`{open,close}`','`{left,right,both}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Point `{side}` arm `{direction}` | point('`{left,right}`','`{up,down,out,ahead}`'}) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Turn `[number]` degrees | turn(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 

## Logic Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| if `<condition>` do `<action>` | | | | 
| `<value>` `{equality}` `<value>` | | | | 
| `<value>` `{logic condition}` `<value>` | | | | 
|  not `<value>` | | | | 
 | `{true/false}` | | | | 

## Loops Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Repeat `[number]` time do `<action>` | | | | 
| Repeat `{while}` `<condition>` do `<action>` | | | | 
| Repeat `{until}` `<condition>` do `<action>` | | | | 

## Maths Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[number]` | | | | 
| `[number]` `{operation}` `[number]` | | | | 
| `[number]` `{odd/even}` | | | | 
| Round `<value>` | | | | 
| Random integer from `[number]` to `[number]` | | | | 

## Text Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| "`[text]`" | | | | 
| append `[text]` to `[text]` | | | | 
| length of `<value>` | | | | 

## Colour Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[colour]` | | | | 
| Random colour | | | | 

## Variables Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Set `{variable}` to `<value>' | | | | 
| Change `{variable}` by `<value>` | | | | 
| `{variable}` | | | | 
