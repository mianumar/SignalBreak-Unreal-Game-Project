# Signal Break — Technical Architecture

**Version:** 1.0  
**Date:** July 2026  
**Engine:** Unreal Engine 5.4+  
**Primary Language:** C++  
**Companion Doc:** [GDD.md](./GDD.md)  

---

## 1. Architecture Overview

Signal Break uses a **component-driven, data-oriented gameplay architecture** built on Unreal's Gameplay Framework. Core logic lives in C++; Blueprints are reserved for designer-facing tuning hooks, UI layout, and VFX spawning.

### Design Principles

1. **Single Responsibility** — Each component owns one concern (abilities, interaction, stealth state).
2. **Data-Driven** — Tunable values in `UDataAsset` / DataTables, not hardcoded constants.
3. **Interface-First** — Cross-system contracts via `UInterface` (interactables, damageable, resonance targets).
4. **Subsystem for Global State** — Alert/trace, save/load, and objective tracking use `UGameInstanceSubsystem`.
5. **Testable in Isolation** — Key components expose debug commands and work in empty test maps.
6. **No Tick by Default** — Prefer timers, delegates, and event-driven updates.

### High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        UGameInstance                             │
│  ┌──────────────────┐  ┌──────────────────┐  ┌───────────────┐ │
│  │ UResonanceSubsystem│  │ USaveSubsystem   │  │ UObjectiveSub │ │
│  └────────┬─────────┘  └──────────────────┘  └───────────────┘ │
└───────────┼─────────────────────────────────────────────────────┘
            │ Trace / Alert events
┌───────────▼─────────────────────────────────────────────────────┐
│ ASignalBreakPlayerController                                     │
│   └── Input (Enhanced Input) → Tool selection, ability fire      │
├─────────────────────────────────────────────────────────────────┤
│ ASignalBreakCharacter                                            │
│   ├── UAbilityComponent        (tool execution, cooldowns)       │
│   ├── UStealthComponent        (noise, exposure, detection)      │
│   ├── UInteractionComponent    (trace + interact)                │
│   └── UHealthComponent         (damage, death, respawn)          │
├─────────────────────────────────────────────────────────────────┤
│ World Actors                                                     │
│   ├── AEnemyBase → AWatcherDrone | ASentinel | AHunter           │
│   ├── AResonanceTarget (platforms, turrets, force fields)        │
│   ├── AInteractableBase (doors, terminals, pickups)              │
│   └── ACheckpointVolume                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Technology Stack

| Layer | Choice | Rationale |
|-------|--------|-----------|
| Engine | UE 5.4+ | Industry standard; UK studios overwhelmingly use UE |
| Language | C++17 | Portfolio requirement for gameplay roles |
| Scripting | Blueprint (limited) | UI, VFX triggers, designer tweaks |
| Input | Enhanced Input | UE5 standard |
| AI | Behavior Tree + EQS | Built-in, well-documented, interview-friendly |
| UI | UMG | Sufficient for HUD scope |
| Audio | MetaSounds (optional) / Sound Cues | Start with Sound Cues; upgrade if time |
| VFX | Niagara | Industry standard |
| Source Control | Git + Git LFS | `.uasset`, `.umap` via LFS |
| IDE | Rider or Visual Studio | Rider preferred for UE C++ |

### Explicitly NOT Using (Vertical Slice)

| Technology | Reason |
|------------|--------|
| Gameplay Ability System (GAS) | High learning curve; custom component demonstrates same patterns with less overhead |
| Mass Entity / ECS | Overkill for actor count in slice |
| Multiplayer replication | Out of scope |
| Custom engine plugins shipped separately | Focus on gameplay, not plugin packaging |

---

## 3. Project Structure

```
SignalBreak/
├── Config/
│   ├── DefaultEngine.ini
│   ├── DefaultGame.ini
│   └── DefaultInput.ini
├── Content/
│   ├── SignalBreak/
│   │   ├── Characters/
│   │   ├── Enemies/
│   │   ├── Tools/
│   │   ├── UI/
│   │   ├── VFX/
│   │   ├── Audio/
│   │   ├── Levels/
│   │   │   ├── L_Test_CoreSystems.umap
│   │   │   ├── L_Zone01_MaintenanceBay.umap
│   │   │   └── L_VerticalSlice_Persistent.umap  (World Partition or sublevels)
│   │   └── Data/
│   │       ├── DA_Ability_Ping.uasset
│   │       ├── DA_Ability_Stabilize.uasset
│   │       ├── DA_Ability_Overload.uasset
│   │       └── DT_EnemyTuning.uasset
│   └── ThirdParty/           # Marketplace assets only
├── Source/
│   └── SignalBreak/
│       ├── SignalBreak.Build.cs
│       ├── SignalBreak.h / .cpp
│       ├── Public/
│       │   ├── Core/
│       │   ├── Character/
│       │   ├── Abilities/
│       │   ├── AI/
│       │   ├── Interaction/
│       │   ├── Subsystems/
│       │   ├── UI/
│       │   └── Interfaces/
│       └── Private/
│           └── (mirrors Public)
├── docs/
│   ├── GDD.md
│   └── TechnicalArchitecture.md
├── .gitattributes              # Git LFS rules
├── .gitignore
└── README.md
```

### Module Dependencies (`SignalBreak.Build.cs`)

```csharp
PublicDependencyModuleNames.AddRange(new string[]
{
    "Core", "CoreUObject", "Engine", "InputCore",
    "EnhancedInput", "AIModule", "GameplayTasks",
    "NavigationSystem", "UMG", "Niagara"
});
```

---

## 4. Core Class Hierarchy

### 4.1 Game Framework

| Class | Parent | Responsibility |
|-------|--------|----------------|
| `ASignalBreakGameMode` | `AGameModeBase` | Rules, player class, default pawn |
| `ASignalBreakGameState` | `AGameStateBase` | Match-level state (minimal in slice) |
| `USignalBreakGameInstance` | `UGameInstance` | Subsystem registration, travel |
| `ASignalBreakPlayerController` | `APlayerController` | Input binding, HUD creation |
| `ASignalBreakPlayerState` | `APlayerState` | Stats for end screen |

### 4.2 Character

```cpp
ASignalBreakCharacter : ACharacter
├── UAbilityComponent*        AbilityComponent
├── UStealthComponent*        StealthComponent
├── UInteractionComponent*    InteractionComponent
├── UHealthComponent*         HealthComponent
└── USpringArmComponent* / UCameraComponent*
```

### 4.3 Enemy

```cpp
AEnemyBase : ACharacter
├── UHealthComponent*
├── UAIAlertnessComponent*    Per-enemy detection state
├── UAIPerceptionComponent*   Sight + hearing + custom resonance
└── UBehaviorTree* + UBlackboardData* (per archetype)

AWatcherDrone : AEnemyBase     (flying, sight-only)
ASentinel : AEnemyBase         (ground, sight + hearing)
AHunter : AEnemyBase           (elite, resonance sense + shield cycle)
```

---

## 5. Ability System (Custom — Not GAS)

A lightweight **`UAbilityComponent`** manages tool modes without GAS overhead while demonstrating patterns employers recognize.

### 5.1 Data Definition

```cpp
UCLASS(BlueprintType)
class UAbilityDataAsset : public UDataAsset
{
    GENERATED_BODY()
public:
    UPROPERTY(EditDefaultsOnly) FName AbilityId;
    UPROPERTY(EditDefaultsOnly) float Cooldown = 3.f;
    UPROPERTY(EditDefaultsOnly) float TraceCost = 10.f;
    UPROPERTY(EditDefaultsOnly) float Range = 1500.f;  // 15m in cm
    UPROPERTY(EditDefaultsOnly) TSubclassOf<UGameplayEffect> EffectClass; // optional
    UPROPERTY(EditDefaultsOnly) UNiagaraSystem* CastVFX;
    UPROPERTY(EditDefaultsOnly) USoundBase* CastSFX;
};
```

### 5.2 Runtime Component

```cpp
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class UAbilityComponent : public UActorComponent
{
    // Registered abilities from data assets
    TMap<FName, FAbilityRuntimeState> Abilities;

    bool TryActivateAbility(FName AbilityId, const FAbilityActivationParams& Params);
    bool IsOnCooldown(FName AbilityId) const;
    float GetCooldownRemaining(FName AbilityId) const;

    DECLARE_MULTICAST_DELEGATE_TwoParams(FOnAbilityActivated, FName, bool);
    FOnAbilityActivated OnAbilityActivated;
};
```

### 5.3 Ability Implementations

| Class | Pattern | Description |
|-------|---------|-------------|
| `UAbility_Ping` | `UObject` strategy | Sphere/cone trace; applies reveal effect |
| `UAbility_Stabilize` | `UObject` strategy | Raycast to `IResonanceControllable` |
| `UAbility_Overload` | `UObject` strategy | Raycast to `IResonanceDisruptable` |

**Activation flow:**

```
Input → PlayerController → Character::RequestAbility(AbilityId)
  → AbilityComponent::TryActivateAbility
    → Validate cooldown + trace budget (via Subsystem)
    → Execute ability strategy
    → Apply trace cost
    → Start cooldown timer
    → Broadcast delegates → UI + AI perception stimulus
```

### 5.4 Why Not GAS?

| Factor | Custom Component | GAS |
|--------|------------------|-----|
| Time to first ability | 2–3 days | 1–2 weeks |
| Interview explainability | High (you wrote it) | Medium (framework magic) |
| Studio relevance | Shows you can build systems | Shows you know Epic's framework |
| Scope fit | Perfect for 3 abilities | Over-engineered |

**Portfolio note:** Document in README that GAS migration path exists (`UAbilityComponent` → `UAbilitySystemComponent`).

---

## 6. Resonance Subsystem

Global trace and alert state lives in **`UResonanceSubsystem`** (`UGameInstanceSubsystem`).

```cpp
UCLASS()
class UResonanceSubsystem : public UGameInstanceSubsystem
{
    float CurrentTrace = 0.f;
    EAlertLevel AlertLevel = EAlertLevel::Silent;

    void AddTrace(float Amount, FVector SourceLocation);
    void TickTraceDecay(float DeltaTime);  // Called from subsystem tick or timer

    EAlertLevel GetAlertLevel() const;
    FVector GetLastPingLocation() const;

    DECLARE_MULTICAST_DELEGATE_OneParam(FOnAlertLevelChanged, EAlertLevel);
    FOnAlertLevelChanged OnAlertLevelChanged;
};
```

### Alert Level Enum

```cpp
UENUM(BlueprintType)
enum class EAlertLevel : uint8
{
    Silent     UMETA(DisplayName = "Silent"),      // 0-29
    Unstable   UMETA(DisplayName = "Unstable"),    // 30-59
    Hunting    UMETA(DisplayName = "Hunting"),     // 60-89
    Lockdown   UMETA(DisplayName = "Lockdown")     // 90-100
};
```

### Integration Points

| System | Listens To | Action |
|--------|------------|--------|
| AI GameMode modifier | `OnAlertLevelChanged` | Adjust global patrol speed multiplier |
| Hunter spawn trigger | Alert >= Hunting | Activate Hunter in Zone 4 |
| UI Trace Widget | Trace value each frame | Update bar + color |
| Audio Manager | Alert level | Crossfade music layers |
| Lockdown sequence | Alert == Lockdown | Start 60 sec escape timer |

---

## 7. Interaction System

### 7.1 Interface

```cpp
UINTERFACE(MinimalAPI, Blueprintable)
class UInteractable : public UInterface { GENERATED_BODY() };

class IInteractable
{
    GENERATED_BODY()
public:
    virtual bool CanInteract(AActor* Interactor) const = 0;
    virtual void Interact(AActor* Interactor) = 0;
    virtual FText GetInteractionPrompt() const = 0;
};
```

### 7.2 Interaction Component

```cpp
UInteractionComponent : UActorComponent
{
    float InteractionRange = 200.f;
    TWeakObjectPtr<AActor> CurrentTarget;

    void TickInteractionFocus();  // Timer-based, NOT every frame unless needed
    void TryInteract();

    DECLARE_MULTICAST_DELEGATE_OneParam(FOnFocusChanged, AActor*);
};
```

### 7.3 Resonance Target Interfaces

```cpp
// Stabilize targets
class IResonanceControllable
{
    virtual bool CanBeStabilized() const = 0;
    virtual void ApplyStabilize(float Duration) = 0;
};

// Overload targets
class IResonanceDisruptable
{
    virtual bool CanBeOverloaded() const = 0;
    virtual void ApplyOverload(float Duration) = 0;
};

// Ping reveal
class IResonanceRevealable
{
    virtual void Reveal(float Duration) = 0;
    virtual bool IsHiddenUntilPing() const = 0;
};
```

---

## 8. Stealth Component

```cpp
UStealthComponent : UActorComponent
{
    EStealthStance CurrentStance;  // Stand, Crouch, Sprint
    bool bInShadowVolume = false;
    bool bInEnemyLOS = false;

    float GetCurrentNoiseRadius() const;
    float GetSightExposureModifier() const;

    DECLARE_MULTICAST_DELEGATE(FOnExposureChanged);
};
```

### Noise Calculation

Data-driven via `UStealthSettingsDataAsset`:

| Stance | Base Noise | Sprint Multiplier |
|--------|------------|-------------------|
| Crouch | 200 uu | — |
| Walk | 500 uu | — |
| Sprint | 500 uu | ×2.4 |

Surface types (Physical Material) modify footstep noise: metal +50%, carpet -30%.

---

## 9. AI Architecture

### 9.1 Perception Setup

```cpp
// AEnemyBase constructor
UAIPerceptionComponent* PerceptionComp;

// Configured senses:
// 1. UAISense_Sight   — cone, radius per enemy type
// 2. UAISense_Hearing — noise events from StealthComponent
// 3. UResonanceSense  — CUSTOM: detects Ping/Overload trace events
```

### 9.2 Custom Resonance Sense

```cpp
UCLASS()
class UResonanceSense : public UAISense
{
    // Register stimulus when UResonanceSubsystem broadcasts ping location
    static void ReportResonanceEvent(UWorld* World, FVector Location, float Strength);
};

UCLASS()
class UResonanceSenseConfig : public UAISenseConfig
{
    float ResonanceRadius = 2500.f;  // 25m for Hunter
};
```

### 9.3 Behavior Tree Structure (Sentinel Example)

```
Selector (Root)
├── Sequence [Combat]
│   ├── Decorator: Has LOS to Target
│   └── Task: Chase / Attack
├── Sequence [Search]
│   ├── Decorator: Alert State == Searching
│   └── Task: Move to Last Known → EQS sweep
├── Sequence [Investigate]
│   ├── Decorator: Has Stimulus (Hearing or Resonance)
│   └── Task: Move to Stimulus Location → Wait → Look Around
└── Sequence [Patrol]
    └── Task: Follow Spline / Move to Next Patrol Point
```

### 9.4 Blackboard Keys

| Key | Type | Purpose |
|-----|------|---------|
| `TargetActor` | Object | Player reference |
| `LastKnownLocation` | Vector | Last seen/heard position |
| `PatrolIndex` | Int | Current patrol point |
| `AlertState` | Enum | Unaware/Suspicious/Searching/Alerted |
| `InvestigateLocation` | Vector | Stimulus origin |
| `bIsStunned` | Bool | Overload active |

### 9.5 EQS Usage (Minimal)

| Query | Use Case |
|-------|----------|
| `EQS_FindCover` | Hunter approach — pick cover point near player |
| `EQS_Investigate` | Search — pick points in radius of last known |
| `EQS_Flee` | Optional — Sentinel retreats when overloaded |

### 9.6 AI Debug Requirements

Implement console commands:
- `SB.DebugAI 1` — Draw sight cones, last known positions
- `SB.DebugTrace 1` — Show trace meter sources
- `SB.DebugAbilities 1` — Log ability activation pipeline

---

## 10. Input Architecture (Enhanced Input)

### 10.1 Mapping Contexts

| Context | Priority | When Active |
|---------|----------|-------------|
| `IMC_Default` | 0 | Gameplay |
| `IMC_UI` | 1 | Menus |
| `IMC_Dialogue` | 2 | Terminal/log playback (optional) |

### 10.2 Input Actions

| Action | Type | Handler |
|--------|------|---------|
| `IA_Move` | Axis2D | `Character::Move` |
| `IA_Look` | Axis2D | `Controller::Look` |
| `IA_Sprint` | Bool | `StealthComponent::SetSprinting` |
| `IA_Crouch` | Bool | `Character::ToggleCrouch` |
| `IA_Interact` | Bool | `InteractionComponent::TryInteract` |
| `IA_UseTool` | Bool | `Character::RequestAbility(Fire)` |
| `IA_AltTool` | Bool | `Character::RequestAbility(AltFire/Hold)` |
| `IA_ToolPing` | Bool | Select Ping |
| `IA_ToolStabilize` | Bool | Select Stabilize |
| `IA_ToolOverload` | Bool | Select Overload |

All bindings wired in `ASignalBreakPlayerController::SetupInputComponent()` — C++ only.

---

## 11. UI Architecture

### 11.1 HUD Class

```cpp
ASignalBreakHUD : AHUD
{
    TSubclassOf<UUserWidget> HUDWidgetClass;
    UUserWidget* HUDWidget;

    void InitHUD();
    void BindToSubsystems();  // Trace, objectives, ability events
};
```

### 11.2 Widgets (UMG — Blueprint layout, C++ binding)

| Widget | C++ Controller | Data Source |
|--------|----------------|-------------|
| `WBP_HUD_Main` | `UHUDWidget` | Health, trace, objective |
| `WBP_ToolIndicator` | `UToolIndicatorWidget` | AbilityComponent cooldowns |
| `WBP_DetectionWarning` | `UDetectionWidget` | StealthComponent exposure |
| `WBP_PauseMenu` | `UPauseMenuWidget` | — |
| `WBP_EndScreen` | `UEndScreenWidget` | PlayerState stats |

**Pattern:** Widgets expose `NativeConstruct` bindings; C++ subscribes to delegates; no gameplay logic in Widget Blueprint graphs.

---

## 12. Save System

### 12.1 Save Subsystem

```cpp
UCLASS()
class USaveSubsystem : public UGameInstanceSubsystem
{
    bool SaveCheckpoint(FName CheckpointId);
    bool LoadLatestCheckpoint();
    bool HasSave() const;
};
```

### 12.2 Save Game Object

```cpp
UCLASS()
class USignalBreakSaveGame : public USaveGame
{
    FName LastCheckpointId;
    FVector PlayerLocation;
    FRotator PlayerRotation;
    float SavedTrace;
    TArray<FName> CompletedObjectives;
    TArray<FName> CollectedShards;
    FTimespan TotalPlayTime;
};
```

Checkpoints triggered by `ACheckpointVolume` overlap → auto-save → UI notification.

---

## 13. Level & Streaming

### 13.1 Recommended Approach

**Option A (Recommended for solo):** Single persistent map with **5 sublevels** loaded via Level Streaming Volumes.

```
L_VerticalSlice_Persistent
├── SL_Zone01_MaintenanceBay
├── SL_Zone02_TransitCorridor
├── SL_Zone03_PumpRoom
├── SL_Zone04_SecurityNexus
└── SL_Zone05_CoreRelay
```

**Option B:** World Partition with data layers (more setup, better for large teams).

### 13.2 Test Map

`L_Test_CoreSystems` — isolated room with:
- Dummy enemy
- Moving platform (stabilize target)
- Turret (overload target)
- Hidden door (ping reveal)

Build every system here **before** greyboxing zones.

---

## 14. Performance Budget

### 14.1 Targets

| Metric | Target |
|--------|--------|
| Frame rate | 60 FPS @ 1080p |
| Frame time | ≤16.6 ms |
| Draw calls | <2000 (mid scene) |
| Tickable actors | Minimize — audit monthly |

### 14.2 Rules

1. **No Tick** on `UAbilityComponent`, `UInteractionComponent` — use timers
2. **AI Tick** — ensure BT tasks sleep appropriately; 10 Hz perception updates acceptable for grunts
3. **Ping reveal** — use pooled `AActor` markers or instanced static meshes, not spawn/destroy spam
4. **Object pooling** for VFX if spawn rate exceeds 5/sec
5. Run `stat unit`, `stat ai`, Unreal Insights each milestone

### 14.3 Profiling Checklist (M6)

- [ ] Worst-case: Zone 4 with Hunter + 3 Sentinels + Lockdown
- [ ] Ping hold in crowded area
- [ ] No GC spikes >5ms during normal play

---

## 15. C++ vs Blueprint Boundaries

| System | C++ | Blueprint |
|--------|-----|-----------|
| Character movement | ✓ | — |
| Ability logic | ✓ | — |
| AI controllers & BT tasks | ✓ | BT layout only |
| Interaction | ✓ | — |
| Subsystems | ✓ | — |
| Data assets | Defined in C++ | Instanced in editor |
| HUD layout | Bindings in C++ | Visual layout in UMG |
| VFX spawn calls | Trigger from C++ | Niagara systems |
| Level scripting | — | Sequencer + BP for doors |
| Material parameters | Set from C++ | Material graphs |

**Rule:** If it has gameplay logic, it lives in C++. Blueprint executes designer-authored content only.

---

## 16. Coding Standards

### 16.1 Naming Conventions

| Type | Prefix | Example |
|------|--------|---------|
| Actor | `A` | `ASignalBreakCharacter` |
| Component | `U` | `UAbilityComponent` |
| Interface | `I` | `IInteractable` |
| Struct | `F` | `FAbilityRuntimeState` |
| Enum | `E` | `EAlertLevel` |
| Data Asset | `U` + `DA_` asset name | `DA_Ability_Ping` |

### 16.2 File Organization

One public header per class. Implementation in matching `.cpp`. No mega-files.

### 16.3 Documentation

- Doxygen-style comments on public API
- `README.md` architecture section with system diagram
- `docs/TechnicalArchitecture.md` (this file) kept in sync with major changes

---

## 17. Testing Strategy

### 17.1 Manual Test Cases (Minimum)

| ID | Test | Pass Criteria |
|----|------|---------------|
| T01 | Ping reveals hidden door | Door visible 4 sec, trace +8 |
| T02 | Stabilize freezes platform | Platform stops 4 sec, resumes |
| T03 | Overload stuns Watcher | Drone disabled 5 sec |
| T04 | Trace triggers Lockdown | At 90+ trace, lockdown sequence starts |
| T05 | Sentinel investigates noise | Sprint near sentinel → investigate |
| T06 | Checkpoint saves/loads | Quit and reload → same state |
| T07 | Full playthrough | Start to end screen without blockers |

### 17.2 Automation (Nice to Have)

```cpp
IMPLEMENT_SIMPLE_AUTOMATION_TEST(FAbilityPingCooldownTest, "SignalBreak.Abilities.PingCooldown",
    EAutomationTestFlags::ApplicationContextMask | EAutomationTestFlags::ProductFilter)
```

Test: Activate Ping → immediate second activation fails → wait cooldown → succeeds.

---

## 18. Build & Deployment

### 18.1 Git LFS (`.gitattributes`)

```
*.uasset filter=lfs diff=lfs merge=lfs -text
*.umap filter=lfs diff=lfs merge=lfs -text
*.ubulk filter=lfs diff=lfs merge=lfs -text
*.uexp filter=lfs diff=lfs merge=lfs -text
*.png filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
```

### 18.2 README Requirements

- UE version (exact, e.g. 5.4.4)
- Build steps (Generate VS project → Build Development Editor)
- Controls reference
- Architecture summary + link to docs
- Trailer link
- Screenshot/GIF
- License (MIT for code; marketplace assets noted separately)

### 18.3 Shipping Build

Package: `Development` or `Shipping` for demo distribution.  
Include: `.exe` + `Pak` files on itch.io for reviewers who won't clone the repo.

---

## 19. Milestone → Technical Deliverables

| Milestone | Technical Deliverables |
|-----------|------------------------|
| M0 | Project created, Git LFS, folder structure, empty subsystems |
| M1 | `ASignalBreakCharacter`, Enhanced Input, `UInteractionComponent` |
| M2 | `UAbilityComponent` + 3 abilities + `UResonanceSubsystem` |
| M3 | `AEnemyBase`, perception, BT for Watcher + Sentinel |
| M4 | All zones streamed, checkpoints, objectives |
| M5 | HUD wired, Hunter AI, audio/VFX hooks |
| M6 | Profiling pass, automation test (optional), bug fixes |
| M7 | Packaged build, GitHub polish, architecture PDF |

---

## 20. Key Implementation Snippets

### 20.1 Ability Activation (Character)

```cpp
void ASignalBreakCharacter::RequestAbility(FName AbilityId, const FAbilityActivationParams& Params)
{
    if (!AbilityComponent) return;

    if (AbilityComponent->TryActivateAbility(AbilityId, Params))
    {
        if (UResonanceSubsystem* ResSub = GetGameInstance()->GetSubsystem<UResonanceSubsystem>())
        {
            const float Cost = AbilityComponent->GetTraceCost(AbilityId);
            ResSub->AddTrace(Cost, GetActorLocation());
        }
    }
}
```

### 20.2 Ping Reveal (Ability Strategy)

```cpp
void UAbility_Ping::Execute(ASignalBreakCharacter* Owner, const FAbilityActivationParams& Params)
{
    const float Radius = Data->Range;
    TArray<FOverlapResult> Overlaps;
    // Sphere overlap on ResonanceRevealable channel
    for (const FOverlapResult& Hit : Overlaps)
    {
        if (IResonanceRevealable* Revealable = Cast<IResonanceRevealable>(Hit.GetActor()))
        {
            Revealable->Reveal(RevealDuration);
        }
    }
    // Report to AI perception
    UResonanceSense::ReportResonanceEvent(Owner->GetWorld(), Owner->GetActorLocation(), Params.Strength);
}
```

### 20.3 Stealth Noise Event

```cpp
void UStealthComponent::BroadcastFootstepNoise()
{
    if (UAISense_Hearing::ReportNoiseEvent(GetWorld(), GetOwner()->GetActorLocation(),
        GetCurrentNoiseRadius(), GetOwner()))
    {
        // Optional: debug draw
    }
}
```

---

## 21. Migration & Extension Paths

Document these in interviews to show forward thinking:

| Current | Future Extension |
|---------|------------------|
| `UAbilityComponent` | Migrate to GAS for AAA studio pipelines |
| Single-player save | Add co-op with replicated `UResonanceSubsystem` state |
| Scripting via Sequencer | Add quest subsystem with data-driven graph |
| 3 abilities | Modifiers via `UAbilityModifierDataAsset` |
| Hand-placed patrols | Procedural patrol generation for roguelike mode |

---

## 22. Security & Legal (Portfolio)

- Do not commit marketplace assets to public GitHub unless license allows
- Use placeholder assets during development; document replacements in README
- No licensed music in public builds without rights
- `.gitignore` engine intermediates: `Binaries/`, `Intermediate/`, `Saved/`, `DerivedDataCache/`

---

## 23. Architecture Decision Records (ADR)

### ADR-001: Custom Ability Component over GAS

**Status:** Accepted  
**Context:** Solo portfolio project, 3 abilities, 12-week timeline  
**Decision:** Custom `UAbilityComponent` with data assets  
**Consequences:** Faster iteration; must document GAS equivalence in README for studios that use it  

### ADR-002: GameInstance Subsystem for Trace State

**Status:** Accepted  
**Context:** Trace affects AI globally, UI, audio  
**Decision:** Single `UResonanceSubsystem` as source of truth  
**Consequences:** Easy to bind; avoid duplicating trace on Character  

### ADR-003: Level Streaming over Single Map

**Status:** Accepted  
**Context:** 5 zones, solo developer  
**Decision:** Persistent map + 5 streamed sublevels  
**Consequences:** Easier iteration per zone; requires streaming volume setup  

---

*End of Technical Architecture Document*
