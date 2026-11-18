# Alien Upgrade Path

*Danesh's part. Builds on the proposal ("learn from your friends", choose your abilities), the outline (brain waves), Jory's `Docs/Progression.md` and the H/J/K/L skill keys in `AlienPlayer`.*
*Updated by Hemanth (2026-11-05): Friend Trap added as a third J choice; J is now in the game.*

## How it works

- The alien starts with **H: Small Beam** (befriend one person).
- At the end of each level the alien **evolves** and picks **one** new skill (2 or 3 to choose from).
- Each evolution unlocks the next skill key: **J**, then **K**, then **L**.
- There are two paths, and the player can mix them:
  - **Brain path** (pink): catch more people at once.
  - **Sneak path** (blue): control the crowd and stay hidden.
- Each choice answers the new threat of the next level, so the old level feels easier after evolving.

## The path

| Evolution | Key | Next level | New threat | Choices |
|---|---|---|---|---|
| Start | H | Level 1: Quiet Farm Town | none (tutorial) | Small Beam: befriend one person |
| 1 | J | Level 2: Fort Maple | police patrols (3 hits each, whistles) | **Wide Wave** (brain): 3 beams in a 40 degree fan · **Calm Hum** (sneak): people near the alien stop panicking for 4 s · **Friend Trap** (sneak, Hemanth): drop a trap, it befriends everyone near when someone walks by |
| 2 | K | Level 3: Fort Maple Command | the General (mini boss) + sentries | **Long Beam** (brain): 2 hits, double range, goes through people · **Disguise** (sneak): look like a soldier for 5 s, nobody notices · **Psi Shield** (brain, new): a psionic field for 10 s that blocks all hits and gives other skills +1 damage |
| 3 | L | Level 4: City Centre | army + time limit | **Tractor Beam** (brain): pull everyone in a big circle · **Friend Army** (sneak): friends come back for 8 s and block an exit |

K is in the game (Level 3). L is designed but not built yet (no Level 4).

Every evolution also makes the alien stronger: **+1 max HP, +0.5 tiles of beam range and 6% more speed** (Level 3 alien: 7 HP, 4.5-tile beam).

Numbers in the game now (playtest and tune):

| Skill | Cooldown | Notes |
|---|---|---|
| Small Beam (H) | 0.6 s | range 3.5 tiles, first person hit |
| Wide Wave (J) | 1.2 s | 3 beams at -20/0/20 degrees, range 3.5 tiles, each hits the first person |
| Calm Hum (J) | 8 s | radius 3 tiles, 4 s: runners stop and settle, lookouts/police in range don't raise the alarm |
| Friend Trap (J) | 6 s | one trap at a time, goes off when someone is within 1.3 tiles, 2 hits to everyone within 3 tiles, lasts 25 s |
| Long Beam (K) | 1.5 s | twice the beam range, 2 hits to everyone in the line |
| Disguise (K) | 12 s | 5 s as a soldier: nobody notices, guards and the General ignore the alien; using another skill ends it |
| Psi Shield (K) | 20 s | 10 s psionic field: blocks all damage (the General's darts) and every other skill does +1 damage while it is up |

Police officers need 3 hits; every beam or trap hit counts as one.

Alien stage models: put the frames for stage N in `Assets/Resources/Alien/stageN/` and that level uses them (see the README there). Until a model exists, the slime is drawn a little bigger each stage.

## In Unity

- `Assets/Scripts/Upgrades/UpgradePath.cs`: all the skills, which level each evolution leads to, and the player's picks (`UpgradePath.Chosen`).
- `Assets/Scripts/Upgrades/UpgradeScreen.cs`: the **Evolve!** screen. It shows the whole path as a tree; click a card or press 1 / 2 / 3, then Evolve. After evolving it loads the next level.
- `Assets/Scripts/AlienPlayer.cs`: puts the picked J skill on the J key (`SetSkill`) and fires it.
- `Assets/Scripts/FriendTrap.cs`: the Friend Trap.
- `Assets/Editor/UpgradeScreenBuilder.cs`: menu **Alien Game > Build Upgrade Screen** makes `Assets/Scenes/UpgradeScreen.unity` and adds it to the build.
- Icons: `Assets/Resources/Upgrades/` (16 x 16 pixel art). `friend_trap.png` added for the trap.

Flow: Level 1 complete → **Evolve** → pick a J skill → Level 2. Starting a new game from the start screen resets the picks.
