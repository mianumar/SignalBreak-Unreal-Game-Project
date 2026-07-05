# Signal Break — Unreal Engine Project

Portfolio vertical slice — third-person stealth-action puzzle game in **Unreal Engine 5.6** with **C++**.

**Engine:** UE 5.6  
**Template:** Third Person (C++)  
**Repo:** https://github.com/mianumar/SignalBreak-Unreal-Game-Project

## Design Documents

| Document |
|----------|----------------|
| [Game Design Document](docs/GDD.md) | Right-click → Open Preview, or `Ctrl+Shift+V` |
| [Technical Architecture](docs/TechnicalArchitecture.md) | Same as above |

## Current Project State

- `ASignalBreakCharacter` — base third-person character with Enhanced Input (move, look, jump)
- `ASignalBreakPlayerController` — input mapping contexts
- Template variants included (Combat, Platforming, SideScrolling) — **ignore these for now**
- Git initialized, pushed to GitHub
- Design docs in `docs/` — gameplay code not yet started

## Next Milestone: M1 — Core Movement + Interaction

See [Technical Architecture §19](docs/TechnicalArchitecture.md) for full milestone list.

### Step 1 — Open project

1. In Text Editor: **File → Open Folder**
2. Select: `D:\UnrealProjects\SignalBreak\SignalBreak-Unreal-Game-Project`
3. Open `docs/GDD.md` and press **Ctrl+Shift+V** for formatted preview

### Step 2 — Create test map

In Unreal Editor:

1. **File → New Level → Empty Open World** (or Basic)
2. Save as `Content/SignalBreak/Levels/L_Test_CoreSystems`
3. Add a floor, Player Start, and a few cubes to interact with later

### Step 3 — Reorganize Source folders

Create this structure under `Source/SignalBreak/`:

```
Core/
Character/
Abilities/
Interaction/
Interfaces/
Subsystems/
AI/          (later — M3)
UI/          (later — M5)
```

Leave `Variant_Combat`, `Variant_Platforming`, `Variant_SideScrolling` untouched for now (delete later once your systems work).

### Step 4 — Add Interaction system (first C++ feature)

1. Create `UInteractable` interface (`IInteractable`)
2. Create `UInteractionComponent` on the character
3. Add `IA_Interact` input action in Content
4. Press **E** to interact with a test door actor

### Step 5 — Add sprint + crouch

Extend `ASignalBreakCharacter`:

- `IA_Sprint` — walk 350, sprint 550 uu/s
- `IA_Crouch` — toggle crouch, reduce capsule height and speed

### Step 6 — Set up Git LFS (before committing assets)

```powershell
git lfs install
git lfs track "*.uasset" "*.umap" "*.ubulk" "*.uexp"
git add .gitattributes
git commit -m "Add Git LFS tracking for Unreal assets"
```

## What NOT to do yet

- Do not build Combat / Platforming / SideScrolling variants
- Do not start AI (M3) until interaction works
- Do not buy marketplace assets until `L_Test_CoreSystems` is playable
- Do not implement GAS — use custom `UAbilityComponent` per TA

## Week-by-week focus

| Week | Goal |
|------|------|
| 1 | Test map + interaction + sprint/crouch |
| 2 | `UAbilityComponent` skeleton |
| 3–4 | Ping, Stabilize, Overload tools |
| 5–6 | Watcher + Sentinel AI |
| 7–8 | Greybox all 5 zones |
| 9–12 | Polish + portfolio deliverables |
