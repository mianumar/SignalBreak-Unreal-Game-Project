# Signal Break — Game Design Document

**Version:** 1.0  
**Date:** July 2026  
**Author:** Portfolio Project  
**Engine:** Unreal Engine 5.4+  
**Language:** C++ (primary), Blueprint (UI/VFX hooks)  
**Target Platform:** PC (Windows)  
**Scope:** Vertical slice — 15–20 minutes playable demo  

---

## 1. Executive Summary

**Signal Break** is a third-person stealth-action puzzle game set in a decaying research facility where electromagnetic infrastructure has gone unstable. The player is a maintenance operative equipped with a **Resonance Tool** — a device that manipulates environmental frequencies to reveal hidden information, control machinery, and disrupt enemy security systems.

The project is designed as a **portfolio vertical slice** demonstrating C++ gameplay programming, modular ability systems, AI perception, data-driven design, and production polish suitable for UK junior gameplay programmer roles.

**Elevator pitch:** *Metal Gear meets Portal — stealth through sound, sight, and systems.*

---

## 2. Design Pillars

| Pillar | Description | Player Feeling |
|--------|-------------|----------------|
| **Read the Room** | Information is a resource. Ping reveals patrols, hazards, and hidden routes before you commit. | Smart, prepared, in control |
| **Risk the Resonance** | Every tool emits a trace. Overuse attracts hunters and raises facility alert. | Tension, consequence |
| **Compose Solutions** | Levels are puzzles with multiple valid routes — stealth, distraction, or timed platforming. | Creative agency |
| **Clarity Under Pressure** | UI, audio, and VFX communicate state instantly. No ambiguity about detection or cooldowns. | Confidence |

---

## 3. Target Audience & Market Position

### Primary Audience
- Portfolio reviewers at UK game studios (gameplay, AI, generalist programming roles)
- Players who enjoy stealth-puzzle games: *Dishonored*, *Mark of the Ninja*, *The Turing Test*

### Secondary Audience
- Game dev students and hobbyists evaluating technical breakdown content

### Market Position
Not a commercial product. A **technical showcase** disguised as a cohesive game experience. Every system exists to answer: *"Can this developer ship gameplay code in our engine?"*

---

## 4. Setting & Narrative

### World
**Helix Substation Zero** — an underground relay station that routed city-wide communication signals. After a cascade failure, automated security drones and corrupted maintenance bots patrol corridors while resonant anomalies warp the environment.

### Tone
Sci-fi industrial. Quiet dread, not horror. Think sterile corridors, flickering holograms, low hum of failing transformers.

### Story (Minimal — Environmental)
The player receives a final transmission: reach the **Core Relay** and broadcast a shutdown signal before the facility locks down permanently. No cutscenes longer than 30 seconds. Story delivered through:
- 6–8 audio logs (30–45 sec each)
- Environmental signage and terminal readouts
- One mid-level NPC hologram (non-interactive)

### Narrative Beats (Vertical Slice)

| Beat | Location | Story Moment |
|------|----------|--------------|
| 1 | Maintenance Bay | Wake up; tool calibration; first log explains lockdown |
| 2 | Transit Corridor | See patrol patterns; log hints at "resonance traces" |
| 3 | Pump Room | Environmental hazard; log reveals sabotage implication |
| 4 | Security Nexus | Hunter enemy introduced; log from security chief |
| 5 | Core Relay | Final set-piece; broadcast shutdown; escape timer |

---

## 5. Player Fantasy

You are not a soldier. You are a **systems thinker with a tool** — someone who wins by observing, planning, and executing precise interventions. The fantasy is outsmarting machines that are faster and stronger than you.

---

## 6. Core Gameplay Loop

```
Observe (Ping) → Plan route → Execute (Stabilize / Overload / Traverse) → Reassess alert state → Repeat
```

### Session Loop (Level Scale)
1. Enter new zone — assess patrols and hazards
2. Identify objective (keycard, switch, exit)
3. Choose approach: ghost / distraction / rush
4. Manage resonance trace meter
5. Reach checkpoint or complete objective
6. Brief breather (safe room, audio log)
7. Next zone with combined mechanics

### Moment-to-Moment Loop
1. Hold Ping (risk: trace) → gather intel
2. Move between cover using stealth movement
3. Apply Stabilize or Overload at critical moment
4. Traverse environmental challenge
5. Hide or break line-of-sight to reduce alert

---

## 7. Player Character

### Name
**Kira Voss** (placeholder — easily renamed)

### Abilities (Base — No Upgrades in Slice)

| Action | Input | Description |
|--------|-------|-------------|
| Move | WASD | Standard third-person movement |
| Sprint | Shift | Faster movement, louder footsteps (+AI hearing) |
| Crouch | Ctrl | Slower, quieter, smaller detection profile |
| Cover | Q (context) | Snap to cover point when near |
| Interact | E | Doors, switches, terminals, ladders |
| Tool Wheel | Tab / 1-3 | Select Ping / Stabilize / Overload |
| Use Tool | LMB | Fire selected tool |
| Ping Hold | Hold RMB (with Ping selected) | Continuous scan, higher trace rate |

### Movement Parameters (Initial Tuning)

| Parameter | Value | Notes |
|-----------|-------|-------|
| Walk speed | 350 uu/s | Unreal default scale |
| Sprint speed | 550 uu/s | |
| Crouch speed | 200 uu/s | |
| Jump height | Low | Not a platformer — jump is utility only |
| Health | 100 | Regenerates only in safe zones (optional) |

---

## 8. Resonance Tool — Core Mechanic

The Resonance Tool is the game's identity. Three modes, one resource system.

### 8.1 Ping (Intel Mode)

**Purpose:** Reveal hidden information at the cost of generating a detectable signal.

**Behavior:**
- Short pulse (tap): 360° sonar pulse, 15m radius, 2 sec reveal
- Long scan (hold): Cone sweep 30m, reveals patrol paths through walls (silhouette), 1 sec per sweep
- Reveals: enemy positions (last known), hidden doors, floor hazards, interactable devices
- Visual: cyan wireframe overlay on revealed objects, fades over 4 sec

**Costs:**
- Cooldown: 3 sec (tap), no cooldown but continuous trace (hold)
- Trace: +8 (tap), +4/sec (hold)

### 8.2 Stabilize (Control Mode)

**Purpose:** Temporarily freeze or lock environmental systems.

**Behavior:**
- Targeted raycast, 12m range
- Valid targets: moving platforms, rotating hazards, pendulum obstacles, timed door mechanisms
- Effect: Freezes target for **4 seconds**, then 6 sec cooldown before re-use on same target
- Visual: amber time-distortion shader on target

**Costs:**
- Cooldown: 5 sec global
- Trace: +15 per use

### 8.3 Overload (Disrupt Mode)

**Purpose:** Disable enemies or security devices briefly.

**Behavior:**
- Targeted raycast, 10m range
- Valid targets: drones (stun 5 sec), turrets (disable 8 sec), force fields (down 6 sec)
- Invalid: Hunter during "shielded" state (telegraphed — teaches timing)
- Visual: white flash + electrical arc VFX

**Costs:**
- Cooldown: 8 sec global
- Trace: +25 per use

---

## 9. Resonance Trace & Alert System

### Trace Meter
- Range: 0–100
- Decay: -5 per second when hidden (no LOS from enemies, not sprinting)
- Sources: tool use, sprinting (+2/sec), failed stealth takedown, walking on metal grates

### Alert Thresholds

| Trace Level | State | Effect |
|-------------|-------|--------|
| 0–29 | **Silent** | Normal patrols |
| 30–59 | **Unstable** | Enemies investigate last ping origin; faster patrol transitions |
| 60–89 | **Hunting** | Hunter spawns or activates; doors lock partially |
| 90–100 | **Lockdown** | All enemies converge on player last known position; 60 sec survive/escape |

Trace resets to 0 at checkpoints and after 30 sec in safe room.

**Design intent:** Force players to ping intelligently, not spam. Creates rhythm of intel → action → cool-down.

---

## 10. Stealth & Detection

### Player Noise Tiers

| Action | Noise Radius |
|--------|--------------|
| Crouch walk | 2m |
| Walk | 5m |
| Sprint | 12m |
| Tool Ping (tap) | 8m |
| Tool Overload | 10m |

### Light & Shadow (Simplified for Scope)
- Binary states: **Lit** (full sight cone) / **Shadow** (50% sight range)
- No dynamic light simulation — artist-placed volumes

### Detection Stages (Per Enemy)
1. **Unaware** — default patrol
2. **Suspicious** — heard noise or saw ping echo; investigate point
3. **Searching** — lost player; sweep area 15 sec
4. **Alerted** — has LOS; pursue/combat
5. **Reset** — return to patrol if player escapes for 20 sec

---

## 11. Enemies

### 11.1 Watcher Drone (Basic)

| Attribute | Value |
|-----------|-------|
| Role | Patrol + area denial |
| Detection | Sight only (120° cone, 18m) |
| Speed | Slow hover |
| Weakness | Overload stuns 5 sec |
| Behavior | Fixed or spline patrol; investigates ping origin |

**Teaching role:** Introduces patrol reading and Overload timing.

### 11.2 Sentinel (Humanoid Bot)

| Attribute | Value |
|-----------|-------|
| Role | Ground patrol, hearing-sensitive |
| Detection | Sight (90° cone, 15m) + Hearing (8m) |
| Speed | Medium |
| Weakness | Overload disables 5 sec; can be Stabilize-frozen mid-door if scripted |
| Behavior | BT: Patrol → Investigate → Search → Alert |

**Teaching role:** Introduces sound discipline and cover usage.

### 11.3 Hunter (Elite — Set-Piece Only)

| Attribute | Value |
|-----------|-------|
| Role | Pressure enemy for Security Nexus |
| Detection | Sight (360°, 20m) + Resonance sense (detects Ping origin at 25m) |
| Speed | Fast |
| Weakness | Shield cycle — 3 sec vulnerable window after Overload (doesn't stun, but opens shield) |
| Behavior | Aggressive search; uses EQS cover during approach |

**Teaching role:** Combines all systems; appears only in Zone 4–5.

---

## 12. Level Structure — Vertical Slice

**Total playtime:** 15–20 minutes (first play)  
**Zones:** 5 + intro tutorial space  
**Checkpoints:** 4  

### Zone Map Overview

```
[Maintenance Bay] → [Transit Corridor] → [Pump Room] → [Security Nexus] → [Core Relay]
     Tutorial           Stealth intro      Puzzle + hazard     Hunter set-piece      Finale
```

### Zone 1: Maintenance Bay (Tutorial — 3 min)
- Teach: move, crouch, interact, Ping tap
- Single Watcher drone in optional side path
- Objective: Calibrate tool at terminal, exit through vent
- No fail state — soft guidance via hologram prompts

### Zone 2: Transit Corridor (Stealth — 4 min)
- Teach: cover, noise, Stabilize on closing door timer
- 2 Sentinels with overlapping patrols
- Optional: overload turret to create distraction route
- Objective: Retrieve keycard from locker room
- Checkpoint at corridor midpoint

### Zone 3: Pump Room (Puzzle/Platform — 4 min)
- Teach: Stabilize on moving platforms + rotating hazard
- Environmental: rising water (scripted timer, not instant death — 30 sec to complete)
- 1 Watcher + 1 Sentinel
- Objective: Restore power to transit lift
- Audio log: sabotage hint

### Zone 4: Security Nexus (Set-Piece — 4 min)
- Hunter introduced via scripted moment (cannot be killed — only evaded/overloaded)
- Multi-route: overhead catwalk (stealth) vs ground (distraction via overload chain)
- Objective: Disable 3 security nodes within 90 sec (doesn't have to be simultaneous)
- Checkpoint before Hunter activation

### Zone 5: Core Relay (Finale — 3–5 min)
- Combined mechanics: ping to navigate blackout, stabilize final platform sequence
- Lockdown triggered at 50% broadcast progress — 60 sec escape timer
- Final objective: Hold interact at Core Terminal for 10 sec (defend position or pre-clear)
- End screen: stats (time, trace peak, detections, tools used)

---

## 13. Progression & Upgrades

**Vertical slice: NO permanent upgrades.**

Optional pickups (collectibles for portfolio depth):
- 5 **Data Shards** hidden across levels — unlock dev commentary or lore entries in menu
- Does not affect gameplay balance

Future expansion hook (document only):
- Tool mods (wider ping, longer stabilize) via metroidvania backtracking

---

## 14. UI/UX Design

### HUD Elements

| Element | Position | Info |
|---------|----------|------|
| Health bar | Bottom-left | Simple, minimal |
| Tool indicator | Bottom-right | Current tool + cooldown ring |
| Trace meter | Top-center | Horizontal bar, color shifts at thresholds |
| Objective text | Top-left | One line, updates on milestone |
| Detection indicator | Center edge | Red pulse on enemy LOS (like survival horror) |
| Interaction prompt | Context | "E — Open" |

### Menus
- Main menu: New Game, Continue (checkpoint), Settings, Quit
- Pause: Resume, Settings, Restart Checkpoint, Main Menu
- Settings: Graphics presets, sensitivity, invert Y, subtitles, audio sliders
- End screen: Stats + Restart + Quit

### Accessibility (Minimum)
- Subtitles for all logs
- Colorblind-safe trace meter (icon + pattern, not color alone)
- Remappable keys
- Hold vs toggle crouch option

---

## 15. Audio Direction

### Music
- Ambient electronic — low synth pads, rhythmic pulses synced to facility hum
- Combat/alert layer: add percussion when trace > 60
- Zone stingers on Hunter activation and Lockdown

### SFX Priority
1. Tool feedback (distinct per mode — Ping=soft chime, Stabilize=low thrum, Overload=sharp crack)
2. Footstep surfaces (metal, grates, water)
3. Enemy states (patrol hum, alert klaxon, Hunter servo)
4. UI confirmations

### VO
- 6–8 audio logs (AI or recorded — student voice acceptable with good writing)
- No player VO

---

## 16. Visual Direction

### Art Style
- **Realistic stylized** — UE5 Lumen, Nanite where sensible
- Palette: cold blues and greys, accent cyan (Ping), amber (Stabilize), white (Overload)
- Modular kit: industrial corridors, pipes, cable trays, holographic terminals

### VFX Requirements
- Ping pulse (Niagara sphere + post-process brief flash)
- Reveal silhouette (custom depth/stencil or overlay material)
- Stabilize time-freeze (material parameter collection)
- Overload arc (Niagara beam + enemy flicker)
- Trace meter full — screen edge distortion

### Lighting
- Strong readable contrast for stealth (shadow volumes marked in blockout)
- Flickering emergency lights in alert states

---

## 17. Controls Reference

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left stick |
| Look | Mouse | Right stick |
| Sprint | Shift | L3 |
| Crouch | Ctrl | B / Circle |
| Jump | Space | A / Cross |
| Interact | E | X / Square |
| Use Tool | LMB | RT |
| Alt Tool (Hold Scan) | RMB | LT |
| Tool 1/2/3 | 1/2/3 | D-pad |
| Tool Wheel | Tab | LB |
| Pause | Esc | Start |

---

## 18. Scope Definition

### IN SCOPE (Must Ship)
- 5 zones + tutorial as described
- 3 tool modes fully functional
- 3 enemy types (Watcher, Sentinel, Hunter)
- Trace + alert system
- 4 checkpoints + save/load
- Full HUD + main/pause/end menus
- 6 audio logs
- Playable from start to credits in 20 min
- GitHub repo with build instructions
- 3–5 min trailer + technical breakdown

### OUT OF SCOPE (Do Not Build)
- Multiple levels / chapter select
- Skill trees or RPG stats
- Inventory beyond key items
- Multiplayer
- Full dialogue system
- Procedural generation
- Mobile/console ports

### NICE TO HAVE (If Core Done Early)
- Data Shard collectibles with lore menu
- Photo mode
- Speedrun timer with splits
- Editor utility widget for ability tuning

---

## 19. Production Milestones

| Milestone | Week | Deliverable | Success Criteria |
|-----------|------|-------------|------------------|
| M0: Pre-production | 1 | GDD + TA + blockout | Docs approved; greybox Zone 1 |
| M1: Core Movement | 2 | C++ character | Move, crouch, interact, cover |
| M2: Tool Prototype | 4 | All 3 tools functional | Ping reveals; Stabilize freezes; Overload stuns |
| M3: AI Foundation | 6 | Watcher + Sentinel | Patrol, investigate, search, alert |
| M4: Vertical Slice Greybox | 8 | All 5 zones blocked | Playable start-to-finish greybox |
| M5: Content Complete | 10 | Art pass + audio | Looks like a game, not a test map |
| M6: Polish | 11 | Optimization + bugs | 60 FPS target, no blockers |
| M7: Portfolio | 12 | Trailer + GitHub + PDF | Ready to apply |

---

## 20. Portfolio Success Metrics

| Metric | Target |
|--------|--------|
| Playtime (first run) | 15–20 min |
| Frame rate | 60 FPS @ 1080p on mid-range PC (RTX 3060 / equivalent) |
| C++ vs Blueprint ratio | ~70% gameplay logic in C++ |
| GitHub commits | Regular, meaningful messages |
| Code review readiness | Clear folder structure, README, architecture doc |
| Interview demo | Can explain any system in 5 min with code walkthrough |

---

## 21. Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope creep | Project never finishes | Strict OUT OF SCOPE list; cut Zone 5 before Zone 1 polish |
| GAS complexity | Weeks lost learning | Use lightweight custom ability component (see TA) |
| AI bugs | Unfun stealth | Lock behavior early; test with AI debug tools |
| Performance | Bad first impression | Weekly perf pass; no Tick on 100+ actors |
| Art time | Looks unfinished | Buy 1 modular kit; reuse aggressively |
| Perfectionism | No portfolio ship date | M7 deadline is non-negotiable |

---

## 22. Reference Games

| Game | Borrow |
|------|--------|
| *Dishonored* | Multi-route level design, readable stealth |
| *Mark of the Ninja* | Clarity of vision cones and information as power |
| *The Turing Test* | Puzzle pacing in sci-fi facility |
| *Metal Gear Solid* | Alert phases and evasion fantasy |
| *Return of the Obra Dinn* | Minimal story, strong environmental delivery |

---

## 23. Glossary

| Term | Definition |
|------|------------|
| **Resonance Tool** | Player's multi-mode electromagnetic device |
| **Trace** | Accumulated detectable signal; drives alert escalation |
| **Ping** | Intel mode; reveals hidden elements |
| **Stabilize** | Control mode; freezes environmental systems |
| **Overload** | Disrupt mode; stuns enemies/disables devices |
| **Vertical Slice** | One polished section representing full game quality |
| **Safe Room** | Checkpoint area where trace decays and enemies cannot enter |

---

*End of Game Design Document*
