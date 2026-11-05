# Alien Upgrade Path

*Danesh's part. Builds on the proposal ("learn from your friends", choose your abilities), the outline (brain waves), Jory's `Docs/Progression.md` and the H/J/K/L skill keys in `AlienPlayer`.*
*Updated by Hemanth (2026-11-05): the J evolution now offers three skills, chosen by Hemanth as game lead.*

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
| 1 | J | Level 2: Fort Maple | police patrols (3 hits each, whistles) | **Shotgun Beam** (brain): 5 short beams in a wide fan · **Friend Trap** (sneak): drop a trap, it befriends everyone near when someone walks by · **Piercing Beam** (brain): long beam that goes through people and hits all of them |
| 2 | K | Level 3: Downtown | soldiers | **Calm Hum** (sneak): people near the alien stop panicking for 4 s · **Disguise** (sneak): look human for 5 s, nobody runs |
| 3 | L | Level 4: City Centre | army + time limit | **Tractor Beam** (brain): pull everyone in a big circle · **Friend Army** (sneak): friends come back for 8 s and block an exit |

K and L are designed but not in the game yet (no Level 3 / 4). They need re-planning now that J has the
wide and long beams (Wide Wave and Long Beam were folded into Shotgun Beam and Piercing Beam).

Numbers in the game now (playtest and tune):

| Skill | Cooldown | Notes |
|---|---|---|
| Small Beam (H) | 0.6 s | range 3.5 tiles, first person hit |
| Shotgun Beam (J) | 1.4 s | 5 beams at -32/-16/0/16/32 degrees, range 3.2 tiles, each hits the first person |
| Friend Trap (J) | 6 s | one trap at a time, goes off when someone is within 1.3 tiles, hits everyone within 3 tiles, lasts 25 s |
| Piercing Beam (J) | 2.5 s | range 9 tiles, hits everyone in the line |

Police officers need 3 hits; every beam or trap hit counts as one.

Ideas for later (not built): **+1 max HP** every evolution and the alien sprite gets bigger (Talia's stage sprites).

## In Unity

- `Assets/Scripts/Upgrades/UpgradePath.cs`: all the skills, which level each evolution leads to, and the player's picks (`UpgradePath.Chosen`).
- `Assets/Scripts/Upgrades/UpgradeScreen.cs`: the **Evolve!** screen. It shows the whole path as a tree; click a card or press 1 / 2 / 3, then Evolve. After evolving it loads the next level.
- `Assets/Scripts/AlienPlayer.cs`: puts the picked J skill on the J key (`SetSkill`) and fires it.
- `Assets/Scripts/FriendTrap.cs`: the Friend Trap.
- `Assets/Editor/UpgradeScreenBuilder.cs`: menu **Alien Game > Build Upgrade Screen** makes `Assets/Scenes/UpgradeScreen.unity` and adds it to the build.
- Icons: `Assets/Resources/Upgrades/` (16 x 16 pixel art). `friend_trap.png` added for the trap.

Flow: Level 1 complete → **Evolve** → pick a J skill → Level 2. Starting a new game from the start screen resets the picks.
