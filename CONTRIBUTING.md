# How we work on Xenomite

Short version: **pull → make a branch → work → commit → push → pull request → merge.** Nobody pushes straight to `main`.

## One-time setup

1. Install **Unity 6000.6.3f1** through Unity Hub (Installs → Install Editor → Archive tab if it's not listed). Only this version, nothing newer.
2. Install [Git LFS](https://git-lfs.com) and run `git lfs install` once.
3. Clone the repo. Easiest is [GitHub Desktop](https://desktop.github.com): File → Clone repository → `Guard33/xenomite`.
4. Unity Hub → Add → Add project from disk → pick the cloned `xenomite` folder.
5. Optional, but it saves you when scenes conflict: set up Unity's merge tool. In the repo folder, run (fix the path if Unity is installed somewhere else):

   ```
   git config merge.unityyamlmerge.name "Unity SmartMerge"
   git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -h -p --force %O %B %A %A"
   git config merge.unityyamlmerge.recursive binary
   ```

## Every time you work

1. **Save and close Unity** before you switch branches or pull. Unity rewrites files while it's open.
2. **Get the latest `main`**: switch to `main` and Fetch/Pull (GitHub Desktop: Current Branch → main → Pull origin).
3. **Make a branch** for what you're doing, named `yourname-thing`, e.g. `talia-alien-sprite`, `danesh-level2`
   (GitHub Desktop: Current Branch → New Branch, based on `main`).
4. Open Unity and work. Save your scene (Ctrl+S) before committing, because unsaved scene changes aren't in the files yet.
5. **Commit** small, working steps with a message that says what changed:
   "Add alien walk animation", not "stuff" or "update".
6. **Push** the branch (GitHub Desktop: Push origin / Publish branch).
7. **Open a pull request** into `main` on GitHub. Post the link in the group chat.
8. Someone else opens your branch, presses Play, and either approves or comments. Then merge it on GitHub.
9. After it's merged, everyone pulls `main` again.

Same thing on the command line:

```
git switch main
git pull
git switch -c yourname-thing
# ...work in Unity, save, close Unity...
git add -A
git commit -m "Add alien walk animation"
git push -u origin yourname-thing
```

## Before you commit, check the file list

- Every new asset has a matching `.meta` file. Commit them together, never one without the other.
- You should **never** see `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `.csproj` or `.sln` files. If you do, stop and ask.
- Only commit changes you meant to make. Unity sometimes touches `ProjectSettings/` files or scenes you only opened. Discard those (right-click → Discard changes) unless you changed a setting on purpose.
- Images, models and audio go through Git LFS automatically. Don't change `.gitattributes`.

## Scenes: the one rule that matters

**Only one person edits a given scene at a time.** Unity scenes don't merge well. Before you touch someone else's scene, ask in the group chat.
Prefer making things as **prefabs** and testing in your own scene (e.g. `Assets/Scenes/Sandbox/YourName.unity`). Then only small changes go into the shared level scenes.

Who owns what (update this as we go):

| Scene | Owner |
|---|---|
| `Assets/Scenes/Level1.unity` | Danesh (built by *Alien Game → Build Level 1* from `Level1Data.cs`; edit the data file, not the scene by hand) |

## Folders

- `Assets/Scripts/`: game code
- `Assets/Editor/`: editor-only tools (not in the built game)
- `Assets/Art/`: sprites and art packs; keep each pack's license file with it, and add credits to `Assets/Art/Credits.txt`
- `Assets/Scenes/`: levels
- `Assets/Tiles/`: tilemap tiles

## If something goes wrong

- **Merge conflict on a scene or prefab:** don't hand-edit the file. Pick one version (GitHub Desktop: "Use mine" / "Use theirs"), then redo the smaller change in Unity. Ask if unsure.
- **Pink/missing sprites after pulling:** run `git lfs pull`, then reopen Unity.
- **"Missing script" on objects:** someone committed a script without its `.meta` file. Tell them.
- **Stuck or about to force-push:** don't. Ask in the chat first. A force-push can delete other people's work.
