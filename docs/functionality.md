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
| Look `{direction}` | look('`{left,right,ahead}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| `{Open/close}` `{side}` hand(s) | changeHand('`{open,close}`','`{left,right,both}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Point `{side}` arm `{direction}` | point('`{left,right}`','`{up,down,out,ahead}`'}) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Turn `[number]` degrees | turn(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | 

## Logic Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| if `<condition>` do `<action>` | | :grey_question: Unknown | :grey_question: Unknown | 
| `<value>` `{equality}` `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 
| `<value>` `{logic condition}` `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 
|  not `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 
 | `{true/false}` | | :grey_question: Unknown | :grey_question: Unknown | 

## Loops Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Repeat `[number]` time do `<action>` | | :heavy_check_mark: Working | :x: Test failing | 
| Repeat `{while}` `<condition>` do `<action>` | | :heavy_check_mark: Working | :x: Broken | 
| Repeat `{until}` `<condition>` do `<action>` | | :heavy_check_mark: Working | :x: Broken | 

## Maths Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[number]` | `[number]` | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| `[number]` `{operation}` `[number]` | | :grey_question: Unknown | :grey_question: Unknown | 
| `[number]` `{odd/even}` | | :grey_question: Unknown | :grey_question: Unknown | 
| Round `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 
| Random integer from `[number]` to `[number]` | | :grey_question: Unknown | :grey_question: Unknown | 

## Text Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| "`[text]`" | '`[text]`' | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| append `[text]` to `[text]` | | :grey_question: Unknown | :grey_question: Unknown | 
| length of `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 

## Colour Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[colour]` | #`[colour]` | :heavy_check_mark: Working | :heavy_check_mark: Working | 
| Random colour | | :grey_question: Unknown | :grey_question: Unknown | 

## Variables Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Set `{variable}` to `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 
| Change `{variable}` by `<value>` | | :grey_question: Unknown | :grey_question: Unknown | 
| `{variable}` | | :grey_question: Unknown | :grey_question: Unknown | 
