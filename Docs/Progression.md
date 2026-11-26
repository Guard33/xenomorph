# Game Progression & Difficulty Scaling

*Jory's part of the proposal. Built on Danesh's Level 1 "Quiet Farm Town" (see `Level1_README.txt` on the `danesh-level1` branch).*

## Core loop

1. Land in a town and **befriend civilians** (+10 EXP each).
2. Civilians who notice the alien **run to the closest exit**. Anyone who escapes is lost for that level.
3. **Fill the friendship meter** before too many escape, then **evolve** and move on to the next town.
4. If too many escape, the level restarts.

Each level = one alien evolution stage. Each evolution unlocks one new power (from Danesh's alien powers list).

## Level progression

| | Level 1: Quiet Farm Town | Level 2: Suburbs | Level 3: Downtown | Level 4: City Centre (final) |
|---|---|---|---|---|
| Alien stage | 1 | 2 | 3 | 4 |
| Civilians | 20 | 28 | 36 | 45 |
| Exits | 2 | 3 | 3 | 4 |
| Friends needed | 10 (50%) | 15 (~54%) | 21 (~58%) | 27 (60%) |
| EXP to evolve | 100 | 150 | 210 | 270 |
| Escapes allowed | 10 | 13 | 15 | 18 |
| Civilian speed (vs alien) | 0.8x | 0.9x | 1.0x | 1.1x |
| Notice radius | 3 tiles | 4 tiles | 5 tiles | 5 tiles |
| New threat | none (tutorial) | panic spreads | police patrols | army + time limit |

Level 1 values match what is already built (20 civilians, 2 exits, 10 friends x 10 EXP, restart when 11 escape).

## Difficulty levers

These are the knobs we turn between levels. Only change one or two big ones per level so players can tell what got harder.

- **Required friends (%)**: rises from 50% to 60%, so the player can afford fewer mistakes.
- **Exits**: more exits means shorter escape routes, so civilians get out faster.
- **Civilian speed**: starts slower than the alien, ends faster. Late levels can't be won by just chasing.
- **Notice radius**: how close the alien can get before someone runs. Bigger radius rewards sneaking up.
- **Panic spread (Level 2+)**: when one civilian starts running, others within 3 tiles also run.
- **Hunters (Level 3+)**: police / army NPCs patrol and stun the alien for 2 seconds if they catch it.
- **Time limit (Level 4)**: optional pressure for the final level.

## Inside a level

- **Rising alert**: every escape makes the remaining civilians a bit more alert (+5% notice radius). Early stealth pays off, and a bad start snowballs.
- **Calm-down**: if no one escapes for 20 seconds, alert drops back one step. This gives the player a way to recover.
- **Map layout**: the first area near the UFO should have a few easy, isolated civilians so the player can learn befriending before the crowded parts.

## Evolution and powers

- Evolving happens at the end of a level (full friendship meter), not mid-level, so each level is balanced around one known power set.
- Each new power should answer the threat the next level adds. For example: a calming power before panic spread shows up, or a stealth power before police appear. The actual powers come from Danesh's list.
- Rule of thumb: after evolving, the previous level should feel easy. That's the player's reward.

## Replay and scoring (optional)

- Star rating per level: 1 star = cleared, 2 stars = half the allowed escapes or fewer, 3 stars = no escapes.
- Bonus EXP for befriending people quickly one after another (a combo), to reward skilled play without making it required.

*Built by Danesh (`LevelManager.cs`, `Scoreboard.cs`):*
- *Stars show on the level complete card (they pop in one by one) and on the Scoreboard screen, which keeps the most stars for each level.*
- *Combo: befriend the next person within 3 seconds to keep it going. Each extra person gives a bonus (x2 = +5, x3 = +10, ...). The bonus is added to the score on the level complete card, not to the friendship meter, so it never changes when a level is won. Tune with `comboWindow` and `comboBonus` on the LevelManager.*

## How to build it in Unity

- Put each level's numbers (civilians, exits, friends needed, civilian speed, notice radius, threats on/off) in one **LevelConfig ScriptableObject** per level.
- The level manager reads the config instead of hard-coded values, so new levels mostly need new data and a new map, not new code.
- Tune by playtesting: if most testers clear a level on the first try, raise required friends or notice radius. If most fail 3+ times, lower civilian speed first.
