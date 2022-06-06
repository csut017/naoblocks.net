# Commands (Robot Functonality/Blocks)

This page lists all the blocks that are available and their current stage of testing.

**Note:** this list is out-of-date. We just copied it from the old version of NaoBlocks.Net and have not updated it yet.

## Robot Actions Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| Change Chest To `[colour]` | changeLEDColour(CHEST, `[colour]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Change `{eye}` eye(s) to `[colour]` | changeLEDColour(`{BOTH_EYES,LEFT_EYE,RIGHT_EYE}`, `[colour]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Rest | rest() | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Wait for `[number]` s | wait(`[number]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Say `[text]` | say(`[text]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Robot Movements Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| Move to `{position}` | position(`[text]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Move to `{position}` and say `[text]` | position(`[text]`, `[text]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Do `{wave}` | wave() | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Do `{wave}` and say `[text]` | wave(`[text]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Do `{Wipe Forehead}` | wipe_forehead() | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Do `{Wipe Forehead}` and say `[text]` | wipe_forehead(`[text]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Look `{direction}` | look('`{left,right,ahead}`') | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| `{Open/close}` `{side}` hand(s) | changeHand('`{open,close}`','`{left,right,both}`') | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Point `{side}` arm `{direction}` | point('`{left,right}`','`{up,down,out,ahead}`'}) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Turn `[number]` degrees | turn(`[number]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Walk forward `[number]` s, sideways `[number]`s | walk(`[number]`,`[number]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Robot Dances Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| Gangnam (Music `{on/off}`) | dance('gangnam', `{TRUE,FALSE}`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Tai Chi (Music `{on/off}`) | dance('tai chi', `{TRUE,FALSE}`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Logic Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| `{sensor}` head touched | readSensor(`{HEAD_FRONT,HEAD_MIDDLE,HEAD_REAR}`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| battery charge | readSensor(BATTERY) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| distance to `{sonar}` front | readSensor(`{SONAR_LEFT,SONAR_RIGHT}`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| `{dir}` gyroscope | readSensor(`{GYROSCOPE_X,GYROSCOPE_Y,GYROSCOPE_Z}`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Logic Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| if `<condition>` do `<action>` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| `<value>` `{equality}` `<value>` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| `<value>` `{logic condition}` `<value>` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
|  not `<value>` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
 | `{true/false}` | `{TRUE,FALSE}` | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Loops Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| Repeat `[number]` time do `<action>` | loop(`[number]`){<br>&nbsp;&nbsp;&nbsp;`<action>`<br>} | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Repeat `{while}` `<condition>` do `<action>` | while(`<condition>`){<br>&nbsp;&nbsp;&nbsp;`<action>`<br>} | ![Working](Images/Status-Working.png) | ![Functionality Broken](Images/broken.png) | ![Unknown](Images/Status-Unknown.png)
| Repeat `{until}` `<condition>` do `<action>` | while(not(`<condition>`)){<br>&nbsp;&nbsp;&nbsp;`<action>`<br>} | ![Working](Images/Status-Working.png) | ![Functionality Broken](Images/broken.png) | ![Unknown](Images/Status-Unknown.png)

## Maths Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| `[number]` | `[number]` | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{operation}` `[number]` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{even}` | isEven(`[number]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{odd}` | isOdd(`[number]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{prime}` | isPrime(`[number]`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{whole}` | isWhole(`[number]`) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{positive}` | isPositive(`[number]`) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| `[number]` `{negative}` | isNegative(`[number]`) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| Round `<value>` | round(`<value>`) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Random integer from `[number]` to `[number]` | randomInt(`[number]`, `[number]`)) | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Text Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| "`[text]`" | '`[text]`' | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| append `[text]` to `[text]` | append('`[text]`','`[text]`') | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| length of `<value>` | len('`[text]`') | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Colour Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| `[colour]` | #`[colour]` | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)
| Random colour | randomColour() | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)

## Variables Blocks

| Block | NaoLang Construct | To NaoLang | From NaoLang | Robot
|-|-|:-:|:-:|:-:|
| Set `{variable}` to `<value>` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| Change `{variable}` by `<value>` | | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png) | ![Unknown](Images/Status-Unknown.png)
| `{variable}` | @`{variable}` | ![Working](Images/Status-Working.png) | ![Working](Images/Status-Working.png) | ![Unknown](Images/Status-Unknown.png)