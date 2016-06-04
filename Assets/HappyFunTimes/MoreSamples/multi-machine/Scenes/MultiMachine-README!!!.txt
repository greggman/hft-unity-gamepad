**THIS EXAMPLE MUST BE RUN OUTSIDE OF UNITY!!!**

This is an example of running across a game across multiple machines.
It is similar to the 2dPlatformScene example except players can jump from one machine to the next.

2 Big differences:

(1) this example has a custom controller in `Assets/WebPlayerTemplates/HappyFunTimes/controllers/mulit-machine/controller.html`.
That controller shows one example of using an avatar on the phone.

(2) Each player is controlled by the `Assets/HappyFunTimes/MoreSamples/MultiMachineBirdScript.cs`.
That script does its own communication with the controller for all inputs and outputs. When a player goes off the left of the screen the code calls `SwitchGame`.
`SwitchGame`Sends the player and a bunch of state to another game by id and kills the player in the current game. From the receiving game's POV a new player
is starting, just like a single player game, but it arrives with the state data sent by the previous game which is enough to make it appear the games are one large game.

The ids for the games are up to you to make up. In this example they are just game0 through gameN. You pass in the id for each game on the command line. When the
player jumps off the left or right of the screen the code goes to the game with the same id + or - 1 in number. So a player on game5 goes to game6 if jumping off
the right and game4 if jumping off the left. A more complex game would make up more ids.

To run it build the game with just this scene. Save it as MultiMachine.

On OSX, from a Terminal

```
nameofgame.app/Contents/MacOS/nameofgame --num-games=3 --hft-master --hft-id=game0 &

nameofgame.app/Contents/MacOS/nameofgame --num-games=3 --hft-url=ws://localhost:18679 --hft-id=game2 &

nameofgame.app/Contents/MacOS/nameofgame --num-games=3 --hft-url=ws://localhost:18679 --hft-id=game2 &
```

On Windows

```
start /B nameofgame.exe --num-games=3 --hft-master --hft-id=game0

start /B nameofgame.exe --num-games=3 --hft-url=ws://localhost:18679 --hft-id=game2

start /B nameofgame.exe --num-games=3 --hft-url=ws://localhost:18679 --hft-id=game2
```

Make each game a window so you see all three games then connect a phone and you should be able to jump from one game to the next.


