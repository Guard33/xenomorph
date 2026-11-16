# Xenomorph

A top-down pixel-art game made in Unity. You're a lonely alien who crash-landed on Earth: befriend the townspeople with your brain-wave beam before they escape, then evolve and learn new powers.

Made by a team of four for **CSCI 3836 (Computer Game Design & Development)** at Saint Mary's University.

![Fort Maple at night](Docs/screenshots/level2_base.png)

## Gameplay

- **Befriend, don't fight.** Beam people to win them over. Anyone who notices you runs for the exits, and every escape counts against you.
- **Herd the crowd.** Scared people run along the roads and avoid an exit you're guarding, so you can steer them.
- **Watch the watchers.** Lookouts, police and soldiers raise the alarm if you linger in their circle, and the whole area panics.
- **Evolve.** Fill the friendship meter to evolve and pick a new skill. Each level's new threat has an answer in the skill tree.

| | |
|---|---|
| ![Title screen](Docs/screenshots/title.png) | ![Level 1, Quiet Farm Town](Docs/screenshots/level1.png) |
| ![Evolve screen](Docs/screenshots/evolve.png) | ![Fort Maple town at night](Docs/screenshots/level2_town.png) |
| ![Crash site](Docs/screenshots/level2_crash.png) | ![Stage select](Docs/screenshots/stage_select.png) |

## Levels

| Level | Setting | New threat | Goal |
|---|---|---|---|
| 1. Quiet Farm Town | Daytime farm town (tutorial) | Lookouts ring an alarm | Befriend 10 of 20 |
| 2. Fort Maple | Town next to an army base, at night | Police and soldiers on patrol (3 hits, alarms) | Befriend 15 of 30 |
| 3. Downtown | *Planned* | Soldiers | |
| 4. City Centre | *Planned* | Army and a time limit | |

## Skills

The alien starts with **H: Small Beam** and picks one new skill on the Evolve screen after each level.

| Key | Choices |
|---|---|
| H | Small Beam: befriend the first person in front of you |
| J | **Wide Wave**: 3 beams in a fan · **Calm Hum**: nearby people stop panicking and guards can't raise the alarm · **Friend Trap**: a trap that befriends everyone near it when someone walks by |
| K | Long Beam · Disguise *(designed, not built yet)* |
| L | Tractor Beam · Friend Army *(designed, not built yet)* |

## Controls

| Key | Action |
|---|---|
| W A S D | Move |
| H J K L | Skills |
| M | Mini map |
| R | Restart level |
| Tab | Stage select (start screen) |

## Running it

1. Install **Unity 6000.6.3f1** (2D URP) and [Git LFS](https://git-lfs.com), then run `git lfs install` once.
2. Clone the repo:
   ```
   git clone https://github.com/Guard33/xenomorph.git
   ```
3. In Unity Hub: **Add > Add project from disk**, and pick the cloned folder.
4. Open `Assets/Scenes/StartScreen.unity` and press **Play**.

## How it's built

- **Levels from data.** Each level's map lives in a C# data file, and an editor script (`Assets/Editor/LevelBuilder.cs`) builds the scene from it. Level 2's layout is written and validated by `Tools/level2_design.py`. See [Docs/Levels.md](Docs/Levels.md).
- **Pathfinding.** People path on the tile grid with one cost map per exit (roads are cheaper than grass). While running, they recheck the way ahead and switch exits if the alien blocks it.
- **Pixel UI in code.** The HUD, menus, speech bubbles and wooden signs use a 5x7 pixel font and pixel panels drawn with `OnGUI` (`Assets/Scripts/Hud.cs`, `PixelFont.cs`).
- **Night lighting** with URP 2D lights: street lamps, floodlights and sweeping searchlights.
- **Upgrade path** data and the Evolve screen: `Assets/Scripts/Upgrades/`. Design notes: [Docs/UpgradePath.md](Docs/UpgradePath.md).

```
Assets/
  Art/        sprites (Level1Pieces, Level2Pieces, Military, Alien, TownFolkSprites)
  Editor/     scene builders and the pixel-art import rule
  Scenes/     StartScreen, Level1, Level2, UpgradeScreen, GameOver
  Scripts/    gameplay, AI, HUD, level data
Docs/         level, progression and upgrade design notes
Tools/        Python scripts for the Level 2 layout and art
```

## Team

| | |
|---|---|
| Hemanth ([@Guard33](https://github.com/Guard33)) | Project lead, repo and tooling, Level 1 rework, Level 2, AI and pathfinding, pixel UI |
| Danesh ([@daneshelahi](https://github.com/daneshelahi)) | Original Level 1 map and builder, start / game over screens, mini map, upgrade path and Evolve screen |
| Jory ([@JustDory](https://github.com/JustDory)) | Progression and difficulty design ([Docs/Progression.md](Docs/Progression.md)) |
| Talia ([@Talia123456789](https://github.com/Talia123456789)) | Military base sprites for Level 2 |

Want to contribute? Read [CONTRIBUTING.md](CONTRIBUTING.md) first.

## Credits

- **Tiny Tile Town** by Aleksandr Makarov ([@IknowKingRabbit](https://twitter.com/IknowKingRabbit)): town tiles, buildings and props. Only the pieces used in the game are included; the asset pack itself is not redistributed here.
- **Town Folk Sprites** by freezepops (CC0).
- **Military sprites** by Talia (team member).

Full list: [Assets/Art/Credits.txt](Assets/Art/Credits.txt).

No license has been chosen yet, so all rights are reserved by the authors. Third-party art stays under its own license.
