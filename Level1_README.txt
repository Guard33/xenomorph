XENOMORPH - LEVEL 1 "QUIET FARM TOWN" (tutorial)

Open the xenomite folder in Unity with our team version (Unity 6000.6.3f1).

Play:    open Assets/Scenes/StartScreen.unity, press Play, then ENTER
         Stage select (button on the start screen, or Tab): jump to any built level and pick
         the skills the alien would have evolved by then
Scene:   Assets/Scenes/Level1.unity
Rebuild: menu Alien Game > Build Level 1 (rebuilds the scene from Assets/Scripts/Level1Data.cs)
         The builder is Assets/Editor/LevelBuilder.cs (Build Level 2 makes Level 2 the same way).
         Don't edit Level1.unity by hand - rebuilding overwrites it. Change Level1Data.cs or Level1Builder.cs.

Play controls
  W A S D   move up / left / down / right
  H         Small Beam - befriends the first person in front of the alien
  J         skill picked on the Evolve screen after Level 1 (Wide Wave, Calm Hum or Friend Trap)
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


LEVEL 2 - "FORT MAPLE" (night)
  Scene:   Assets/Scenes/Level2.unity (menu Alien Game > Build Level 2)
  Layout:  Tools/level2_design.py writes Assets/Scripts/Level2Data.cs - edit the layout there and run it
  Art:     Tools/make_level2_art.py makes the street lamp, floodlight, soldier and asphalt road sprites
  - 62 x 32 map. Every area has a job (the story is at the top of Tools/level2_design.py):
      crash farm (start, SE): the UFO came down in a farmer's corn; farmhouse, barn, pasture
      town (W): main street shops, church, police station, fountain square with both lookouts
      town farm (NW) on the farm lane; Maple woods behind town = cover up to the base fence
      army base (NE, Talia's sprites at 2x): main gate + barracks, HQ + floodlit parade ground,
      airfield (aircraft in front of hangars), supply depot + motor pool by the rear gate, ammo dump
  - Night: dim blue sky light, street lamps in town, floodlights on the base, two sweeping
    searchlights on the gate huts (URP 2D lights, made by the builder)
  - 30 people (18 townspeople, 2 lookouts, 6 base staff, 1 police officer, 3 soldiers), 2 exits
    (west road, base north gate)
  - Patrols: the police officer walks from town up through the south gate; the soldiers walk the
    base road, the north gate road and the outside of the west fence. They need 3 hits and raise
    the alarm (scares everyone nearby) if the alien gets inside their circle
  - Befriend 15 to fill the meter; the level ends when nobody is left
  - Played after Level 1 + the Evolve screen. Pressing Play in Level2.unity directly gives J = Wide Wave
