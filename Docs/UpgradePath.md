# Alien Upgrade Path

*Danesh's part. Builds on the proposal ("learn from your friends", choose your abilities), the outline (brain waves), Jory's `Docs/Progression.md` and the H/J/K/L skill keys in `AlienPlayer`.*

## How it works

- The alien starts with **H: Small Beam** (befriend one person).
- At the end of each level the alien **evolves** and picks **one of two** new skills.
- Each evolution unlocks the next skill key: **J**, then **K**, then **L**.
- There are two paths, and the player can mix them:
  - **Brain path** (pink): catch more people at once.
  - **Sneak path** (blue): control the crowd and stay hidden.
- Each choice answers the new threat of the next level, so the old level feels easier after evolving.

## The path

| Evolution | Key | Next level | New threat | Brain path | Sneak path |
|---|---|---|---|---|---|
| Start | H | Level 1: Quiet Farm Town | none (tutorial) | Small Beam: befriend one person | - |
| 1 | J | Level 2: Suburbs | panic spreads | **Wide Wave**: the wave spreads out and hits 3 people | **Calm Hum**: people near the alien stop panicking for 4 s |
| 2 | K | Level 3: Downtown | police patrols | **Long Beam**: double range, goes through people | **Disguise**: look human for 5 s, nobody runs |
| 3 | L | Level 4: City Centre | army + time limit | **Tractor Beam**: pull everyone in a big circle | **Friend Army**: friends come back for 8 s and block an exit |

Suggested numbers to start playtesting with:

| Skill | Cooldown | Notes |
|---|---|---|
| Wide Wave | 1.2 s | 3 beams in a 40 degree fan |
| Calm Hum | 8 s | radius 3 tiles, 4 s |
| Long Beam | 1.0 s | range 7 tiles, hits everyone in the line |
| Disguise | 15 s | 5 s; ends early if you use a beam |
| Tractor Beam | 12 s | radius 5 tiles, pulls for 1.5 s |
| Friend Army | 20 s | 8 s, blocks the closest exit |

Small bonus every evolution: **+1 max HP** and the alien sprite gets bigger (Talia's stage sprites).

## In Unity

- `Assets/Scripts/Upgrades/UpgradePath.cs`: all the skills and the player's picks (`UpgradePath.Chosen`).
- `Assets/Scripts/Upgrades/UpgradeScreen.cs`: the **Evolve!** screen. It shows the whole path as a tree; click a card or press 1 / 2, then Evolve.
- `Assets/Editor/UpgradeScreenBuilder.cs`: menu **Alien Game > Build Upgrade Screen** makes `Assets/Scenes/UpgradeScreen.unity` and adds it to the build.
- Icons: `Assets/Resources/Upgrades/` (16 x 16 pixel art).

To connect it to a level: when the level is complete, load the `UpgradeScreen` scene (for example from the "Level complete" popup). Then `AlienPlayer` can unlock skill slot J/K/L with the name of `UpgradePath.Picked(tier)`.
