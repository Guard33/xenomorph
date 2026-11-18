# Levels

How the levels are built and what is in each one.

## How levels are made

Levels are generated, not hand-edited. Each level's map (ground, roads, objects, people, exits, patrol routes) is a data file, and an editor script builds the scene from it.

| | Level 1 | Level 2 |
|---|---|---|
| Map data | `Assets/Scripts/Level1Data.cs` | `Assets/Scripts/Level2Data.cs` (written by `Tools/level2_design.py`) |
| Scene | `Assets/Scenes/Level1.unity` | `Assets/Scenes/Level2.unity` |
| Rebuild | menu **Alien Game > Build Level 1** | menu **Alien Game > Build Level 2** |

Level 3 works the same way: `Assets/Scripts/Level3Data.cs`, written by `Tools/level3_design.py`, menu **Alien Game > Build Level 3**. `Tools/make_level3_art.py` makes the General sprite and the Psi Shield icon.

- The builder is `Assets/Editor/LevelBuilder.cs`. Rebuilding overwrites the scene, so change the data file, not the scene.
- `Tools/level2_design.py` checks the Level 2 layout (no overlaps, nothing on roads or water, everyone can reach an exit, patrol routes on open ground), prints a text map and writes `Level2Data.cs`:
  ```
  python Tools/level2_design.py Assets/Scripts/Level2Data.cs
  ```
- `Tools/make_level2_art.py` makes the Level 2 street lamp, floodlight, soldier and asphalt road sprites.

To play: open `Assets/Scenes/StartScreen.unity`, press Play, then Enter. **Stage select** (button on the start screen, or Tab) jumps to any built level and lets you pick the skills the alien would have evolved by then.

## Level 1: Quiet Farm Town (tutorial)

- 40 x 26 tile farm town. The alien starts at its UFO in the corn field.
- 5-step tutorial at the top of the screen: move, beam, exits, lookouts, goal.
- 18 townspeople walk around. When the alien gets close they run along the roads to an exit, and they avoid an exit the alien is guarding.
- 2 lookouts (Town Crier, Mayor) with a yellow watch circle. Stay in it about a second and they ring the alarm: everyone within 7 tiles runs. Beam them first.
- 2 exits: the east bridge and the south road.
- Befriend 10 (100 EXP) to fill the meter. If 11 escape, it's game over. The level ends when nobody is left.
- The alien has 5 HP but can't die in the tutorial (`LevelManager.alienCanDie`).

## Level 2: Fort Maple (night)

- 62 x 32 map. A UFO crash-lands in a farmer's corn field outside a small town next to an army base. Every area has a job (the story is at the top of `Tools/level2_design.py`):
  - **Crash farm** (start, south-east): farmhouse, barn, pasture, three people nearby.
  - **Town** (west): main street shops, church, police station, and the fountain square that both lookouts watch. Exit A is the road west out of town.
  - **Town farm** (north-west) on the farm lane, and the **Maple woods** behind town, which give cover up to the base fence.
  - **Army base** (north-east, Talia's military sprites drawn at 2x): main gate and barracks, HQ with a floodlit parade ground, airfield, supply depot and motor pool by the rear gate (Exit B), ammo dump.
- Night lighting (URP 2D lights): dim blue sky, street lamps, base floodlights, two sweeping searchlights.
- 30 people: 18 townspeople, 2 lookouts, 6 base staff, 1 police officer and 3 soldiers.
- Police and soldiers patrol around the base, take 3 hits, and raise the alarm if the alien gets inside their circle.
- Befriend 15 to fill the meter.
- Played after Level 1 and the Evolve screen. Pressing Play in `Level2.unity` directly gives J = Wide Wave.

## Guards and the base alarm (Levels 2 and 3)

- Soldiers, sentries, police and the General see in a **vision cone** (buildings and vehicles block it). Town lookouts in Levels 1 and 2 still use their watch circle.
- **Soldiers and sentries** carry tranquilliser rifles: 1 damage per dart and a short slow. The alien can be defeated in Levels 2 and 3.
- Spotting the alien raises the **BASE ALARM** for every guard on the map: soldiers leave their routes and hunt the last place the alien was seen, sentries look that way, searchlights sweep faster, and staff near the alien run. It ends 12 s after the alien was last seen.
- **Watchtowers**: the searchlight raises the alarm if the alien stays in its beam for a moment, then locks on (red) and follows it.
- Police (town) still blow a whistle instead of shooting. Disguise fools everyone; Psi Shield blocks darts.

## Level 3: Fort Maple Command (mini boss)

- 96 x 58 map of the main army post, at night, laid out like a real installation. The story is at the top of `Tools/level3_design.py`:
  - **Perimeter** fence and road, with **8 watchtowers** whose searchlights sweep the base.
  - **South gate** (start, entry control point); the main avenue runs north to **HQ** and the north gate (Exit A).
  - **Cantonment**: barracks rows around a quad, dining hall, gym. **Clinic** with a helipad. **Training range**.
  - **HQ** facing the **parade field** (flagpole), flag offices, command centre, bunker. **Cemetery** (NE): rows of headstones.
  - **Motor pool** (fenced yard, vehicles in rows), **ammunition supply point** (own fence, bunkers between berms), **airfield** (hangars, apron, control tower, helipads, fuel, runway). Exit B is the east gate.
- **The General** (mini boss) paces the parade field. Drawn at 2x, 12 hits, health bar at the top of the screen.
  - He sees in a cone 7.5 tiles long and shoots tranquilliser darts (1 damage each). Buildings stop darts and block his view.
  - Below 2/3 health he sounds the base alarm and fires 3-dart spreads; below 1/3 he alarms again and fires faster.
  - Disguise hides the alien from him; Psi Shield blocks his darts.
- The alien **can be defeated** in this level (7 HP at stage 3). Game over says the General's darts knocked it out.
- 32 people: 18 base staff (clerks, cooks, medics, mechanics, pilots, groundskeeper), 5 sentries (2 hits), 8 patrolling soldiers (3 hits) and the General.
- Befriend 17 to fill the meter. Played after Level 2 and the second Evolve screen (the K skill). Pressing Play in `Level3.unity` directly gives J = Wide Wave and K = Long Beam.

## Controls

| Key | Action |
|---|---|
| W A S D | move |
| H | Small Beam: befriends the first person in front of the alien |
| J | skill picked on the Evolve screen (Wide Wave, Calm Hum or Friend Trap) |
| K | skill picked on the second Evolve screen (Long Beam, Disguise or Psi Shield) |
| L | later skill (locked) |
| M | show / hide the mini map |
| R | restart the level |
| Tab | controls page (every key and how to play); also on the start screen |
| Esc | pause menu: resume, restart, controls, settings (volume, fullscreen), main menu, quit |
| S | stage select (start screen) |
