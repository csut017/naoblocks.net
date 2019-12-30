# Robot Functonality/Blocks

This page lists all the blocks that are available and their current stage of testing.

## Robot Actions Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Change Chest To `[colour]` | changeLEDColour(CHEST, `[colour]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Change `{eye}` eye(s) to `[colour]` | changeLEDColour(`{BOTH_EYES,LEFT_EYE,RIGHT_EYE}`, `[colour]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Rest | rest() | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Wait for `[number]` s | wait(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Say `[text]` | say(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown

## Robot Movements Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Move to `{position}` | position(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Move to `{position}` and say `[text]` | position(`[text]`, `[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Do `{wave}` | wave() | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Do `{wave}` and `[text]` | wave(`[text]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Look `{direction}` | look('`{left,right,ahead}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| `{Open/close}` `{side}` hand(s) | changeHand('`{open,close}`','`{left,right,both}`') | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Point `{side}` arm `{direction}` | point('`{left,right}`','`{up,down,out,ahead}`'}) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Turn `[number]` degrees | turn(`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown

## Logic Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| if `<condition>` do `<action>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| `<value>` `{equality}` `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| `<value>` `{logic condition}` `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
|  not `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
 | `{true/false}` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown

## Loops Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Repeat `[number]` time do `<action>` | | :heavy_check_mark: Working | :x: Test failing | :grey_question: Unknown
| Repeat `{while}` `<condition>` do `<action>` | | :heavy_check_mark: Working | :x: Broken | :grey_question: Unknown
| Repeat `{until}` `<condition>` do `<action>` | | :heavy_check_mark: Working | :x: Broken | :grey_question: Unknown

## Maths Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[number]` | `[number]` | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| `[number]` `{operation}` `[number]` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| `[number]` `{odd/even}` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| Round `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| Random integer from `[number]` to `[number]` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown

## Text Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| "`[text]`" | '`[text]`' | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| append `[text]` to `[text]` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| length of `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown

## Colour Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[colour]` | #`[colour]` | :heavy_check_mark: Working | :heavy_check_mark: Working | :grey_question: Unknown
| Random colour | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown

## Variables Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Set `{variable}` to `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| Change `{variable}` by `<value>` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
| `{variable}` | | :grey_question: Unknown | :grey_question: Unknown | :grey_question: Unknown
