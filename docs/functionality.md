# Robot Functonality/Blocks

This page lists all the blocks that are available and their current stage of testing.

## Robot Actions Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Change Chest To `[colour]` | changeLEDColour(CHEST, `[colour]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Change `{eye}` eye(s) to `[colour]` | changeLEDColour(`{BOTH_EYES,LEFT_EYE,RIGHT_EYE}`, `[colour]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Rest | rest() | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Wait for `[number]` s | wait(`[number]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Say `[text]` | say(`[text]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)

## Robot Movements Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Move to `{position}` | position(`[text]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Move to `{position}` and say `[text]` | position(`[text]`, `[text]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Do `{wave}` | wave() | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Do `{wave}` and say `[text]` | wave(`[text]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Do `{Wipe Forehead}` | wipe_forehead() | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Do `{Wipe Forehead}` and say `[text]` | wipe_forehead(`[text]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Look `{direction}` | look('`{left,right,ahead}`') | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| `{Open/close}` `{side}` hand(s) | changeHand('`{open,close}`','`{left,right,both}`') | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Point `{side}` arm `{direction}` | point('`{left,right}`','`{up,down,out,ahead}`'}) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Turn `[number]` degrees | turn(`[number]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)

## Logic Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| if `<condition>` do `<action>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| `<value>` `{equality}` `<value>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| `<value>` `{logic condition}` `<value>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
|  not `<value>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
 | `{true/false}` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)

## Loops Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Repeat `[number]` time do `<action>` | loop(`[number]`){<br>&nbsp;&nbsp;&nbsp;`<action>`<br>} | ![Working](images/working.png) | ![Tests Failing](images/failing.png) | ![Unknown](images/unknown.png)
| Repeat `{while}` `<condition>` do `<action>` | while(`<condition>`){<br>&nbsp;&nbsp;&nbsp;`<action>`<br>} | ![Working](images/working.png) | ![Functionality Broken](images/broken.png) | ![Unknown](images/unknown.png)
| Repeat `{until}` `<condition>` do `<action>` | while(not(`<condition>`)){<br>&nbsp;&nbsp;&nbsp;`<action>`<br>} | ![Working](images/working.png) | ![Functionality Broken](images/broken.png) | ![Unknown](images/unknown.png)

## Maths Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[number]` | `[number]` | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| `[number]` `{operation}` `[number]` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| `[number]` `{odd/even}` | | ![Working](images/working.png) | ![Functionality Broken](images/broken.png) | ![Unknown](images/unknown.png)
| Round `<value>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| Random integer from `[number]` to `[number]` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)

## Text Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| "`[text]`" | '`[text]`' | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| append `[text]` to `[text]` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| length of `<value>` | | ![Working](images/working.png) | ![Functionality Broken](images/broken.png) | ![Unknown](images/unknown.png)

## Colour Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| `[colour]` | #`[colour]` | ![Working](images/working.png) | ![Working](images/working.png) | ![Unknown](images/unknown.png)
| Random colour | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)

## Variables Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|-|-|-
| Set `{variable}` to `<value>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| Change `{variable}` by `<value>` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
| `{variable}` | | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png) | ![Unknown](images/unknown.png)
