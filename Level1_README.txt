XENOMORPH - LEVEL 1 "QUIET FARM TOWN" (tutorial)

Open the xenomite folder in Unity with our team version (Unity 6000.6.3f1).

Play:    open Assets/Scenes/StartScreen.unity, press Play, then ENTER
Scene:   Assets/Scenes/Level1.unity
Rebuild: menu Alien Game > Build Level 1 (rebuilds the scene from Assets/Scripts/Level1Data.cs)
         The builder is Assets/Editor/LevelBuilder.cs (Build Level 2 makes Level 2 the same way).
         Don't edit Level1.unity by hand - rebuilding overwrites it. Change Level1Data.cs or Level1Builder.cs.

Play controls
  W A S D   move up / left / down / right
  H         Small Beam - befriends the first person in front of the alien
  J         skill picked on the Evolve screen after Level 1 (Shotgun Beam, Friend Trap or Piercing Beam)
  K L       skill slots for later powers (locked)
  M         show / hide the mini map
  R         restart the level

What is in the level
  - 40 x 26 tile map made with the Tiny Tile Town tiles
  - The alien: green slime with a 2-frame idle bounce (Assets/Art/Alien)
  - UFO start in the corn field
  - 5-step tutorial at the top of the screen: move, beam, exits, lookouts, goal
  - 18 civilians (Town Folk sprites). They walk around, and when the alien gets close
    they run along the roads to the closest exit
  - 2 lookouts (Town Crier, Mayor) with a yellow watch circle. They never run.
    Stay in their circle ~1 second and they ring the alarm: everyone within 7 tiles runs.
    Beam them first to keep the town calm. They don't hurt the alien.
  - Only 2 exits: Exit A (east bridge) and Exit B (south road)
  - Friendship meter: 10 friends x 10 EXP = 100 EXP. If 11 people escape, game over.
  - The alien has 5 HP (shown top-left). Nothing damages it in Level 1 and it can't die here;
    LevelManager.alienCanDie turns death on for later levels.


LEVEL 2 - "FORT MAPLE"
  Scene:   Assets/Scenes/Level2.unity (menu Alien Game > Build Level 2)
  Layout:  Tools/level2_design.py writes Assets/Scripts/Level2Data.cs - edit the layout there and run it
  - 48 x 30 map: a fenced army base (Talia's military sprites) next to a small town, farmland, UFO field
  - 30 people (18 townspeople, 2 lookouts, 6 base staff, 4 police), 2 exits (west road, base north gate)
  - Befriend 15 to fill the meter; the level ends when nobody is left
  - Police patrol the streets, need 3 hits, and blow a whistle that scares everyone nearby
  - Played after Level 1 + the Evolve screen. Pressing Play in Level2.unity directly gives J = Shotgun Beam
