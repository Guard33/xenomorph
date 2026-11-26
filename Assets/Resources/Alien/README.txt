Alien stage models

Put the frames of each alien stage model here, one folder per stage:
  stage2/  used in Level 2 (after the first evolution)
  stage3/  used in Level 3
Any PNG names work; frames play in name order (e.g. idle_0.png, idle_1.png).
Frames whose names start with "walk" (walk_0.png, walk_1.png ...) play while the alien moves;
all the others are the standing loop.
They import as pixel art at 16 pixels per tile (Assets/Editor/AlienStageImporter.cs), so a
24 x 32 drawing is 1.5 x 2 tiles big. Draw the feet on the bottom row of the image.
If a stage folder is empty, the game uses the previous stage drawn a bit bigger
(or the Level 1 slime, drawn bigger).
