# Construct — Project Handoff for Claude Code

> Compiled 2026-10-21 from the design, architecture and implementation conversations held in claude.ai.
> Place this file at the root of the repo (`E:\PortfolioProject\Construct\CLAUDE.md`) so Claude Code loads it automatically.

---

## 0. Read this first

### Who you're working with
- **Frédéric**, C#/.NET developer with a game-dev background (C++/Unreal 4, Unity). Uses Windows + PowerShell, GitHub user `flevesque12`.
- **Talk to him in informal Québécois French.** All code, file names, comments and commit messages stay in **English**.
- **Work step by step, one piece at a time, with explanations.** Do not dump a whole system at once. For each piece: what file to create, the code, a short "what to understand" section, how to set it up in the scene, how to test, then the commit command. Wait for his confirmation before the next piece.
- He wants to *understand* the code (portfolio project + job interviews), so explain the why behind patterns, not just the what.

### First thing to do in a new session
Some steps below were given to him but **may not be applied yet** (marked ⚠️). Before writing anything new:
1. Look at the repo (`Assets/_Project/Scripts/`, assembly definitions, `Sandbox` scene) and compare with section 6.
2. Tell him what's actually there vs. what this doc expects, and fix gaps first.
3. Check the code-style conventions (section 5) are respected — e.g. the `TrainingDummy` given in chat used `aimPoint` instead of `_aimPoint`; the version in this doc is corrected.

---

## 1. Project at a glance

| | |
|---|---|
| **Name** | Construct |
| **Genre** | 3D third-person action-RPG, combat-focused |
| **Purpose** | Portfolio piece — answers "have you shipped a 3D game?" in Unity interviews |
| **Engine** | Unity 6, URP (Universal 3D template) |
| **Length / scope** | 30–40 min game, 6-week production, PC, released on itch.io |
| **Repo** | `E:\PortfolioProject\Construct`, Git + Git LFS, public GitHub repo (`flevesque12`) |
| **Art** | Low-poly / grey-box: white faceless mannequins (Mixamo Y Bot/X Bot or Quaternius), ProBuilder geometry with triplanar grid shader |
| **Packages** | Input System, Cinemachine 3.x, ProBuilder, AI Navigation (NavMesh), URP post-processing, TextMeshPro |

---

## 2. Game design (summary of the GDD)

### Vision
You are **KERNEL**, a combat program inside a virtual training dojo (the Construct) corrupted by a virus called **NULL**. KERNEL **hacks infected enemies to steal their abilities**.

**Pillars** (every decision must serve at least one):
1. **Tight, readable combat** — every hit is felt (hitstop, impact, sound), every enemy attack is telegraphed.
2. **Every enemy is a resource** — you don't just kill, you choose what to steal and when.
3. **Grey-box is the aesthetic** — the simulation justifies the minimalist look.

**Inspirations:** Kingdom Hearts (aerial combos, lock-on, boss patterns), FF7 Remake (pressure/stagger gauge), Brave Fencer Musashi & Threads of Fate (absorbing enemy powers).

**Portfolio goal:** show in < 2 min of video a 3D combat with solid feel and a data-driven ability system, in a *finished* game.

### World & art direction
- No cutscenes. Story via on-screen system messages ("PROGRAM 2 CORRUPTED — INTEGRITY 34%") and visual degradation of levels.
- Color carries information: player white with cyan accents, infected enemies with magenta glitch, stolen ability = color of the source enemy on KERNEL's arm/blade.
- Glitch shader on infected enemies (vertex jitter, scanlines, dissolve), stronger in program 3.
- URP post: light bloom, brief chromatic aberration + distortion during a hack.
- Palette: max 3 accent colors (cyan, magenta, amber for danger).
- Audio: minimalist electronic, digital SFX (beeps, bitcrushed impacts).

### Weapon: the cursor-sword
One weapon: a light sword whose guard is shaped like a mouse cursor. Used for melee, it can be thrown, and it opens the hack. The blade takes the color of the active stolen ability. (Out of scope bonus: blade changes shape per ability.)

### Controls
| Action | Gamepad | Keyboard / mouse |
|---|---|---|
| Move | Left stick | WASD |
| Camera / switch target | Right stick | Mouse |
| Jump | A | Space |
| Light attack | X | Left click |
| Heavy attack | Y | Right click |
| Dodge | B | Shift |
| Lock-on | R3 | Middle click |
| Quick throw at locked target | LT (tap) | E (tap) |
| Aim | LT held | E held |
| Throw while aiming | RT | Left click |
| Hack (sword planted in Breached enemy) | LT held | E held |
| Recall sword (planted in ground) | LT (tap) | E (tap) |
| Ability 1 / 2 | LB+X / LB+Y | Q / R |

LT is contextual: if the sword is planted in a Breached enemy, holding it starts the hack; otherwise it aims.

### KERNEL moveset
- **Light combo:** 3 hits, the 3rd pushes back.
- **Heavy attack:** slow, big Integrity damage, breaks guards.
- **Launcher:** heavy after 2 lights, launches enemy airborne.
- **Aerial combo:** 3 lights in the air + smash back to ground (Kingdom Hearts style).
- **Sword throw:** ranged, hits and returns like a boomerang, builds target's Integrity.
- **Offensive recall:** returning sword hits enemies on its path (God of War axe).
- **Dodge:** short roll with i-frames, cancelable after an attack's active window.
- **Perfect Dodge (Overclock):** dodge right before impact → 0.5 s slow-mo + 25% Charge. Main skill expression.

### Aim system
- **Quick throw:** tap LT with lock-on active, straight line to target if in range.
- **Free aim:** hold LT → over-the-shoulder Cinemachine camera (0.15 s blend), center reticle, KERNEL moves at 50% speed. RT throws, releasing LT cancels.
- **Slow-mo:** time at 70% for the first 1.5 s of aiming.
- **Aim assist:** light magnetism in a 5° cone, stronger on gamepad.
- **Range 8 m.** Reticle color = predicted result: white (nothing/out of range), cyan (enemy in range, sword hits & returns), magenta (Breached enemy, sword will plant → hack possible).
- **Trajectory:** straight, ~30 m/s (tune). Walls and a Firewall's front shield stop it.

### Resources
| Resource | On | Rises when | Used for |
|---|---|---|---|
| HP | Player & enemies | — | Survival |
| Charge | Player | Hitting, Perfect Dodge | Activating stolen abilities |
| Integrity | Enemies | Being hit (esp. heavy) | Full = **Breach**, enemy becomes hackable |

Starting values (tune):
| Attack | HP dmg | Integrity dmg | Charge gained |
|---|---|---|---|
| Light (per hit) | 10 | 6 | 4% |
| Heavy | 25 | 25 | 8% |
| Aerial smash | 30 | 20 | 10% |

### Enemy readability
- Every enemy attack has a **telegraph**: amber flash + glitch 0.4–0.8 s before impact.
- Unblockable / AoE attacks have a ground marker.
- Max 2 enemies attack at once (**attack tokens**), others circle.

### Game feel (non-negotiable)
- Hitstop 0.05 s (light) to 0.12 s (heavy, smash)
- Cinemachine Impulse on heavy hits and Breach
- White hit flash + "pixel" particles
- Input buffer 0.2 s
- Cancel windows: dodge possible at the end of every attack
- Distinct sounds for: hit, Breach, successful hack, Perfect Dodge

### Hack system (core loop)
To hack a Breached enemy, KERNEL **throws the sword**: it plants in the enemy, KERNEL extends a hand toward it to inject code (visible data stream hand → sword). At the end: **Exploit** (steal ability) or **Execute** (delete it to heal).

Rules:
1. **Breach:** Integrity full → stunned 3 s, heavy glitch, hack icon. Reticle turns magenta on it.
2. **Throw:** quick or aimed throw on a Breached enemy **plants** instead of returning.
3. **Hack:** hold LT for 1 s. KERNEL is immobile, hand extended. Other enemies keep attacking.
4. **Choice** (2 buttons, time slows): **Exploit** = enemy deleted, its ability goes into a slot; **Execute** = enemy deleted, KERNEL heals 20% HP.
5. **Return:** sword returns to hand at the end of the hack either way.
6. **Slots:** 2 ability slots. Stolen ability has **3 uses** and costs Charge. If both full, player chooses which to replace.
7. **Boss abilities:** hacking a boss in Breach at its last phase gives a **permanent** ability in a 3rd slot (Kernel), unlimited uses.

Throw risks:
- **Disarmed:** while the sword isn't back, KERNEL can only move and dodge (no attacks, no abilities).
- **Max range 8 m** to throw and hack; moving beyond during the hack cancels it.
- **Missed throw:** sword plants in ground/wall; must be recalled (tap LT), ~0.6 s to return.
- **Hit during hack:** hack canceled, sword auto-returns, enemy recovers 50% Integrity.

### Enemies
| Enemy | Role | Behavior | Stolen ability | Useful against |
|---|---|---|---|---|
| **Ping** | Fast melee | Group attacks, 2-hit combo, low HP | **Dash Strike**: dash that hits everything on the line | Caster (close distance) |
| **Firewall** | Defense | Front shield, counterattacks if hit from front | **Barrier**: 3 s shield that reflects projectiles | Caster, Warden |
| **Packet** (Caster) | Ranged | Keeps distance, shoots, teleports when approached | **Bolt**: fast projectile, big Integrity dmg | Firewall (over the shield) |
| **Overflow** | Tank | Slow, super armor, AoE slam with ground marker | **Shockwave**: AoE that launches airborne | Ping groups, aerial combo setup |

### Bosses
**Warden** (end of program 2): P1 combines Firewall shield + Packet projectiles (shield blocks frontal throws). P2 (< 50% HP) summons 2 Pings + line charges. Breach in P2 → permanent **Warp** (teleport to where the sword is planted).

**NULL** (final boss, program 3): P1 fast melee mirroring KERNEL's moveset (glitched mannequin). P2 uses the 4 enemy abilities, changing color per ability it prepares (telegraph). P3 arena degrades, floor disappears in sections, faster attacks. Must be hacked to win — the final hack is the last action of the game.

### Structure
| Program | Content | Enemies | Boss | Target length |
|---|---|---|---|---|
| Hub (Lobby) | Program select, results, controls | — | — | — |
| 1. Initialisation | Built-in tutorial: combo, dodge, Breach, first hack | Ping, Firewall | — | 8 min |
| 2. Simulation | 4 arenas, abilities and group fights | Packet, Overflow + previous | Warden | 12 min |
| 3. Corruption | Glitched arenas, mixed enemies, unstable floor | All, corrupted versions | NULL | 15 min |

- Typical arena: 2–3 waves, doors lock at start, open after last wave.
- End-of-program grade (S/A/B/C) from time, damage taken, number of hacks — future hook for his leaderboard API.
- Progression only via permanent boss abilities. No XP, no inventory, no skill tree.

### UI / HUD
Minimal, terminal style (monospace, cyan on transparent).
- Bottom-left: HP bar, Charge bar below, greyed sword icon when sword not in hand.
- Bottom-right: 2 ability slots (icon, source-enemy color, uses left) + Kernel slot when unlocked.
- Locked target: reticle + HP & Integrity bars above enemy.
- Aim: center reticle (white/cyan/magenta).
- Breach: pulsing hack icon above enemy + circular gauge during hack.
- Top-left: system message feed (narration, tutorial).
- No damage numbers by default (menu option).
- Menus: title, pause (resume, controls, quit), program results (grade, time, hacks), end screen.

### Scope
**In (v1.0):** full KERNEL moveset + lock-on; sword quick throw, free aim (reticle, aim assist, slow-mo), recall, offensive recall; full hack system (planting, Breach, Exploit, Execute, 2 slots + Kernel); 4 enemies, 2 bosses, 3 programs, hub; HUD, menus, grades; gamepad + KB/M; Windows build on itch.io + 1–2 min video + GitHub README.

**Out (maybe later):** guard/parry; blade shape change; Corruption gauge; ability combos, skill tree, equipment; online leaderboard; voice, cutscenes, > 3 programs.

**Rule:** any new idea that doesn't serve a pillar or threatens the end date goes to "Out".

### Risks
| Risk | Impact | Mitigation |
|---|---|---|
| Limited Mixamo attack animations | Stiff combat | Pick the animation pack in week 1, before coding combos. A paid combat pack is worth it |
| Aim camera & transitions | Uncomfortable aiming | Prototype aiming in week 1, one aim Cinemachine camera, short blends |
| Feel tuning takes longer | Content delay | Lock base combat end of week 2, then value tweaks only |
| Boss AI too ambitious | Week 5 overflows | Simple FSM patterns; cut a phase rather than ship an unfinished boss |
| Scope creep | Game never ships | Every addition goes through the Out list |

### Production plan (6 weeks, 3 phases, 3 gates)
| Phase | Weeks | Content | Gate |
|---|---|---|---|
| **Base combat** | 1–2 | W1: controller & camera; lock-on, aim, combo. W2: hitbox, feel, dodge; Ping, Integrity, Breach | **Combat locked** — 1 playable arena that's fun |
| **Hack & content** | 3–4 | W3: throw, hack, Exploit; 4 enemies, 4 abilities. W4: programs 1 & 2; Warden | **Content complete** — playable start to finish |
| **Polish & release** | 5–6 | W5: NULL, program 3; HUD, menus, audio. W6: polish, bugs, build; itch.io, video, README | **v1.0 published** on itch.io |

The most important gate is the first: if one arena with a Ping and a hack isn't fun at the end of week 2, fix combat before adding content.

---

## 3. Architecture

### Principles
Three rules: **one component = one responsibility**, **data lives in ScriptableObjects**, **systems talk through events rather than direct references**.

| Pattern | Where | Why |
|---|---|---|
| **State** | `StateMachine` + one class per state (player & enemies) | Each state owns its rules, no giant `Update()` full of `if` |
| **Composition** | `Combatant`, `Health`, `Integrity` on any entity | Same damage code for player, enemies, bosses |
| **Interface** | `IDamageable`, `ITargetable`, later `IHackable` | Systems depend on a contract, not a concrete class |
| **Data-driven (SO)** | `AttackData`, `EnemyData`, later `AbilityData` | Add an attack or enemy in the editor, no code |
| **Observer (event channels)** | `GameEvents` SOs + C# events on components | UI, audio, VFX listen to gameplay without knowing it |
| **Command (input buffer)** | `InputBuffer` keeping inputs 0.2 s | Fluid combos even if pressed a bit early |
| **Strategy** (phase 2) | Abstract `AbilityEffect`, one subtype per ability | Interchangeable ability behaviors |
| **Object Pool** | `UnityEngine.Pool.ObjectPool<T>` for VFX & projectiles | No allocations mid-combat |

**Deliberately avoided:** singletons everywhere and Service Locator. References go through the Inspector or event channels so dependencies stay visible.

### Folder structure
```
Assets/_Project/
  Scripts/
    Core/        → StateMachine, IState, ITargetable, event channels, utilities
    Input/       → GameControls (asset + generated class), InputReader, CursorLock
    Combat/      → Combatant, Health, Integrity, Hitbox, Hurtbox, HitInfo, AttackData, HitFeedback, TrainingDummy
    Player/      → PlayerController, PlayerMotor, PlayerState, States/, ComboController
    Weapon/      → SwordController, AimSystem, LockOnSystem
    Enemies/     → EnemyBrain, enemy states, EnemyData, AttackTokenManager
    Hack/        → (phase 2) HackSystem, AbilitySlots, AbilityData
    UI/          → HUD, menus
  Data/          → ScriptableObject assets (InputReader, attacks, enemies, event channels)
  Prefabs/
  Scenes/        → Sandbox (current test scene)
  Art/           → models, materials, shaders
  Animations/
```
The `_Project` prefix keeps project code apart from imported asset packs (which land at the root of `Assets/`).

### Assembly definitions (one per Scripts subfolder, Root Namespace = assembly name)
| Assembly | References |
|---|---|
| `Construct.Core` | — |
| `Construct.Input` | Core, `Unity.InputSystem` |
| `Construct.Combat` | Core |
| `Construct.Cameras` | Core, Input, `Unity.Cinemachine` — the only assembly allowed to know Cinemachine |
| `Construct.Weapon` | Core, Combat (currently also `Unity.Cinemachine`; drop it once `CinemachineCameraRig` exists) |
| `Construct.Player` | Core, Input, Combat, Weapon, later Hack (same note about `Unity.Cinemachine`) |
| `Construct.Enemies` | Core, Combat, later Hack |
| `Construct.UI` | Core (listens to event channels only) |
| `Construct.Hack` *(to create)* | Core, Combat — owns `IHackable`, since it exposes `AbilityData` |
| `Construct.Game` *(to create)* | Core, Combat, Enemies — arenas, waves, programs, grades, game flow |

Dependencies flow one way: `Core` knows nothing, nothing depends on `UI`.
Full ordered phase list and class diagrams: artifact "Construct Blueprint" (https://claude.ai/artifact/FHNH5kaeMyXfUU9ivj1rET).

### Main systems (planned)
| System | Responsibility |
|---|---|
| `PlayerStateMachine` (via `PlayerController`) | States: Locomotion, Attack, Airborne, Dodge, Aim, Throw, Hack, Hitstun, Dead. `IsArmed` flag blocks attacks when the sword isn't in hand |
| `ComboController` | Attack chains, input buffer, cancel windows (`TryAttack(type)`, `OpenCancelWindow()`) |
| `SwordController` | Sword states `InHand, Thrown, Returning, Planted`; trajectory, parenting to enemy bone when planted, return curve, offensive recall damage; `OnPlanted` event |
| `AimSystem` | Aim Cinemachine camera, reticle & color, aim assist, aim slow-mo, range raycast. `BeginAim()`, `EndAim()`, `Evaluate(): AimResult`, `OnReticleChanged` |
| `HitboxSystem` | Hitbox/Hurtbox enabled by Animation Events, list of already-hit targets per swing |
| `Health` / `Integrity` | Reusable components, events `OnDamaged`, `OnBreach`, `OnDeath` |
| `LockOnSystem` | Target selection, stick target switching, target for the Cinemachine camera |
| `HackSystem` | Hack via planted sword (channeling, range, interruption), hand→sword data stream, Exploit/Execute choice |
| `AbilitySlots` | 2 slots + Kernel, uses, Charge cost, blade color |
| `AbilityData` (SO) | Name, color, icon, cost, uses, effect prefab, animation |
| `EnemyData` (SO) | Stats (MaxHealth, MaxIntegrity, Speed, AttackRange), given ability (StolenAbility), AI profile |
| `EnemyBrain` | FSM: Idle, Chase, Circle, Telegraph, Attack, Breach, Dead — reuses the same `StateMachine` |
| `AttackTokenManager` | Limits simultaneous attackers (lives on `_Systems`) |
| `HitFeedback` | Hitstop, Cinemachine Impulse, hit flash, particles |
| `GameEvents` (SO) | Event channels to decouple UI, audio, gameplay |

### Combat flow
A hit always follows the same path: attacker's `Hitbox` detects a `Hurtbox` → passes a `HitInfo` to the target's `Combatant` → `Combatant` splits damage between `Health` and `Integrity`. `HitInfo` is a `struct` passed with `in` (no allocation per hit). New hittable entity type = add a `Combatant` + `Hurtbox`es, nothing else.

### Communication
- **C# events on components** (`Health.OnDamaged`, `Integrity.OnBreach`, `SwordController.OnPlanted`) — local communication within an entity or between systems that already know each other.
- **SO event channels** (`VoidEventChannel`, `FloatEventChannel`, `HitEventChannel`) — global. Gameplay publishes; UI, audio, camera subscribe.
- Example: KERNEL takes a hit → `Health.OnDamaged` → `PlayerController` relays into the `PlayerHealthChanged` channel → HUD health bar listens. The HUD never knows `PlayerController`.
- **Rule:** subscribe in `OnEnable`, unsubscribe in `OnDisable`, no exceptions. (States: subscribe in `Enter`, unsubscribe in `Exit`.)

### Camera decision (agreed 2026-10-19)
Keep **Cinemachine 3** for Construct (industry standard, actively maintained, saves weeks), but **isolate it**: gameplay never talks to Cinemachine directly. It talks to a small interface owned by the project, e.g.
```csharp
public interface ICameraRig
{
    void SetLockTarget(ITargetable target);
    void ClearLockTarget();
}
```
Only `Construct.Cameras` knows Cinemachine exists; `CinemachineCameraRig` goes there. Create it during **lock-on piece 3**, with `BeginAim()` / `EndAim()` added for the aim camera. A hand-written camera will be done separately in his C++/SDL3 project.

No exception anymore: the orbit is driven by `PlayerCameraInput`, which reads `InputReader.Look` like everything else. Camera shake also stays out of gameplay: `HitFeedback` raises a `CameraImpulse` channel and a listener in `Cameras` plays the Cinemachine impulse.

---

## 4. Input setup

`Scripts/Input/GameControls.inputactions` — Control Schemes: `Gamepad` (Gamepad), `KeyboardMouse` (Keyboard + Mouse). Action map: **`Gameplay`**.

| Action | Type | Gamepad | Keyboard / mouse |
|---|---|---|---|
| `Move` | Value, Vector2 | Left Stick | WASD (Up/Down/Left/Right composite) |
| `Look` | Value, Vector2 | Right Stick | Mouse Delta |
| `Jump` | Button | Button South (A) | Space |
| `LightAttack` | Button | Button West (X) | Left Button |
| `HeavyAttack` | Button | Button North (Y) | Right Button |
| `Dodge` | Button | Button East (B) | Left Shift |
| `LockOn` | Button | Right Stick Press (R3) | Middle Button |
| `Throw` | Button | Left Trigger (LT) | E |
| `Fire` | Button | Right Trigger (RT) | Left Button |

- Left click is bound to both `LightAttack` and `Fire` on purpose: the Aim state listens to `Fire`, other states to `LightAttack`.
- Ability actions (Q/R, LB combos) not added yet — phase 2.
- Generated C# class: name `GameControls`, namespace `Construct.Input`. Never edit the generated file by hand.
- Mouse vs stick sensitivity is handled in `PlayerCameraInput` (mouse = per pixel, stick = per second), using `InputReader.IsLookFromPointer`. No Scale Vector 2 processors needed.

---

## 5. Code conventions

- Code, names, comments in English. Namespace `Construct.<Feature>`.
- `[SerializeField] private` instead of public fields; expose read-only via properties if needed.
- `PascalCase` for classes/methods/properties, **`_camelCase` for private fields**, `camelCase` for locals.
- Animator parameters via `static readonly int` (`Animator.StringToHash`), never hard-coded strings.
- No `GetComponent` in `Update` — cache in `Awake`.
- No `Find` / `FindObjectOfType` in gameplay code.
- One file = one class, file name = class name.
- Tunable values in a ScriptableObject or serialized field, never hard-coded.
- States receive `deltaTime` as a parameter (lets us slow only the player, e.g. while aiming).
- When a state calls `ChangeState` inside `Tick`, it must `return` immediately after.
- States are created once in `PlayerController.Awake` and reused (no `new` per transition).
- One commit per small working step, clear English message.

---

## 6. Current implementation state

### Done and confirmed in the repo (as of 2026-10-21)
- Unity 6 URP project, Git + LFS (`.gitignore` from github/gitignore Unity template, `.gitattributes` via `git lfs track` for `*.fbx *.png *.wav *.psd *.anim *.blend`), Asset Serialization = Force Text, pushed to GitHub.
- Packages: Input System, Cinemachine 3, ProBuilder, AI Navigation.
- `_Project` folder structure + assembly definitions (section 3). `Construct.Enemies` and `Construct.UI` are still empty (Unity warns about it, harmless).
- `IState`, `StateMachine`.
- `GameControls` asset + generated class, `InputReader` SO + asset in `_Project/Data/InputReader.asset`.
- `PlayerMotor`, `PlayerState`, `LocomotionState`, `PlayerController` — camera-relative movement and jump work. Namespace typo `Contruct.Player` fixed on 2026-10-21.
- `CursorLock` + `_Systems` GameObject.
- Lock-on piece 1: `ITargetable`, `TrainingDummy` (field renamed to `_aimPoint` with `[FormerlySerializedAs("aimPoint")]`), `Targetable` layer (6), `TrainingDummy` prefab, 3 instances in Sandbox.
- Orbit camera reworked on 2026-10-20 (details below): `PlayerCameraInput`, `CameraProximityHider`, `Player` layer (7).

### Scene: `Sandbox`
```
Player  (tag Player, layer Player)  ← CharacterController (Center 0,1,0 / Height 2 / Radius 0.4), PlayerMotor, PlayerController,
│                                     LockOnSystem, PlayerAnimator
├── Model                           ← HumanM_Model (Kevin Iglesias pack, Humanoid), KernelMAT (white URP Lit),
│                                     Animator: Kernel.controller, no root motion. No colliders anywhere under Player.
└── CameraTarget                    ← empty, local (0, 1.5, 0)
Main Camera                         ← CinemachineBrain
CameraRig                           ← CinemachineCameraRig (implements ICameraRig)
├── PlayerCamera                    ← orbit: CinemachineCamera (priority 10, Tracking Target = CameraTarget, Blend Hint Inherit Position),
│                                     CinemachineOrbitalFollow, CinemachineRotationComposer,
│                                     PlayerCameraInput, CinemachineDeoccluder, CameraProximityHider
├── LockOnCamera                    ← CinemachineCamera (priority 20, disabled until a lock; LookAt = target AimPoint),
│                                     CinemachineOrbitalFollow (Sphere, radius 4.5, vertical 12°, yaw driven by the rig),
│                                     CinemachineRotationComposer (Screen Y 0.05, damping 0.5), CinemachineDeoccluder
├── AimCamera                       ← CinemachineCamera (priority 30, FOV 50, disabled until aiming; Follow = AimPivot),
│                                     CinemachineThirdPersonFollow (shoulder 0.45/0.15, arm 0.2, distance 2.2, obstacles on Default),
│                                     AimCameraInput (mouse 0.1 °/px, stick 160/110 °/s, pitch −40..50, unscaled time)
└── AimPivot                        ← empty, positioned on CameraTarget and rotated by AimCameraInput
HUD                                 ← Canvas (Screen Space Overlay, scale with 1920×1080)
└── Reticle                         ← Image (built-in Knob sprite, 12 px), ReticleView (listens to ReticleStateChannel)
_Systems                            ← CursorLock, SlowMotion (only writer of Time.timeScale) (later: AttackTokenManager)
TrainingDummy ×3                    ← prefab instances, layer Targetable
```
`PlayerController` fields: Input = `InputReader` asset, Camera Transform = Main Camera, Camera Rig Behaviour = CameraRig. `Player` also has `LockOnSystem` (View = Main Camera, Target Mask = Targetable, Obstacle Mask = Default). Main Camera's brain: default blend Ease In Out 0.35 s, custom blends `Data/CameraBlends.asset` (0.15 s in and out of AimCamera).
Aim (phase 04, piece 1): `AimState` entered from Locomotion when Throw is held ≥ 0.2 s (a shorter tap is reserved for the quick throw), 50% speed, faces camera forward, released = back to Locomotion. Lock-on target switching is disabled while aiming. Animator param `IsStrafing` (was `IsLockedOn`) = lock-on or aiming.
Aim piece 2: `AimSystem` on Player (Weapon) raycasts from the camera center (mask Default + Targetable), range 8 m measured from CameraTarget (the throw origin until the sword hand exists), blocked by Default. Result `AimResult` (struct). It raises `Data/Events/ReticleStateChannel` only when the state changes. First event channel of the project: `EventChannel<T>` in Core + one concrete subclass per payload. `ReticleState` (Hidden / Neutral / Enemy / Breached) lives in Core so Weapon and UI share it. Breached is unused until phase 09.
`Construct.Weapon` and `Construct.Player` no longer reference Cinemachine; `Construct.UI` references `UnityEngine.UI`.
Aim piece 3: `SlowMotion` (Core, on `_Systems`) owns `Time.timeScale`: `Request(owner, scale, realSeconds)` / `Release(owner)`, slowest request wins, also scales `fixedDeltaTime`. Hitstop, Perfect Dodge and the pause menu must go through it too. `AimSystem` requests 0.7 for 1.5 s on `BeginAim` and releases on `EndAim`; it also picks `AssistTarget` (Targetable within 8 m, ≤ 5° from screen center, line of sight). `AimState` passes it to `ICameraRig.SetAimAssistTarget`; `AimCameraInput` eases yaw/pitch toward it (exponential, per second: mouse 2, gamepad 8, unscaled time). The whole world, KERNEL included, slows during the aim slow-mo; only the camera stays at full speed.
Every child of `Player` stays on the `Player` layer, so the Deoccluder (Collide Against = `Default` only) never treats the player as an obstacle.

Current camera settings:
- `CinemachineOrbitalFollow`: Orbit Style **Three Ring** (the vertical axis is normalized, so range is **0..1**, value/center 0.5). Rings: Top r 2 / h 3.5, Center r 4.5 / h 1, Bottom r 2.5 / h −0.8. Binding World Space, Position Damping (0.1, 0.3, 0.1). Horizontal Axis Wrap on.
- `CinemachineRotationComposer`: Target Offset Y 0.3, Screen Position Y −0.1, Damping (0.3, 0.3), Lookahead off.
- `CinemachineCamera` lens: FOV 60, Near Clip 0.1.
- `CinemachineDeoccluder`: Collide Against `Default`, Ignore Tag `Player`, Camera Radius 0.25, Smoothing Time 0.2, Damping 0.4, Damping When Occluded 0.1.
- `PlayerCameraInput` (replaces `CinemachineInputAxisController`): mouse 0.15 °/px yaw and 0.002 orbit/px; stick 200 °/s and 1.2 orbit/s; Invert Y off; ignores the mouse while the cursor is unlocked.
- `CameraProximityHider`: target = CameraTarget, renderers = Visual + Sphere, switches them to shadows-only under 1 m.

### Working with the live Editor
- The Unity MCP server may not be loaded. The `unity` CLI drives the open Editor instead: `unity status`, then `unity command eval '<C#>' --result-only` (run it from Bash, because PowerShell 5.1 strips the inner quotes).
- **Never hand-edit `.unity` / `.prefab` YAML while the Editor has it open.** Unity saves its in-memory copy over the file and the edits are lost. Make scene changes through an eval (with `Undo` + `EditorSceneManager.SaveScene`).
- New `.cs` files need `AssetDatabase.Refresh()` before Unity compiles them.
- Frédéric makes his own commits. Give him a suggested commit message, never commit for him.

### Code already written

#### `Scripts/Core/IState.cs`
```csharp
namespace Construct.Core
{
    public interface IState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
```

#### `Scripts/Core/StateMachine.cs`
```csharp
using System;

namespace Construct.Core
{
    public sealed class StateMachine
    {
        public IState Current { get; private set; }

        public event Action<IState, IState> StateChanged;

        public void ChangeState(IState next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (next == Current)
                return;

            IState previous = Current;

            Current?.Exit();
            Current = next;
            Current.Enter();

            StateChanged?.Invoke(previous, next);
        }

        public void Tick(float deltaTime)
        {
            Current?.Tick(deltaTime);
        }
    }
}
```

#### `Scripts/Input/InputReader.cs`
```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Construct.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Construct/Input Reader")]
    public sealed class InputReader : ScriptableObject, GameControls.IGameplayActions
    {
        // Continuous values, read every frame by the states
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool IsLookFromPointer { get; private set; }
        public bool IsThrowHeld { get; private set; }

        // One-shot button events
        public event Action JumpPressed;
        public event Action LightAttackPressed;
        public event Action HeavyAttackPressed;
        public event Action DodgePressed;
        public event Action LockOnPressed;
        public event Action ThrowPressed;
        public event Action ThrowReleased;
        public event Action FirePressed;

        private GameControls _controls;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new GameControls();
                _controls.Gameplay.SetCallbacks(this);
            }
        }

        private void OnDisable()
        {
            DisableGameplay();
        }

        public void EnableGameplay() => _controls.Gameplay.Enable();
        public void DisableGameplay() => _controls?.Gameplay.Disable();

        public void OnMove(InputAction.CallbackContext context) => Move = context.ReadValue<Vector2>();
        public void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
            IsLookFromPointer = context.control?.device is Pointer;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) JumpPressed?.Invoke();
        }

        public void OnLightAttack(InputAction.CallbackContext context)
        {
            if (context.performed) LightAttackPressed?.Invoke();
        }

        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.performed) HeavyAttackPressed?.Invoke();
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            if (context.performed) DodgePressed?.Invoke();
        }

        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (context.performed) LockOnPressed?.Invoke();
        }

        public void OnThrow(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                IsThrowHeld = true;
                ThrowPressed?.Invoke();
            }
            else if (context.canceled)
            {
                IsThrowHeld = false;
                ThrowReleased?.Invoke();
            }
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed) FirePressed?.Invoke();
        }
    }
}
```
Notes: continuous values are properties; buttons are events. `Throw` exposes press, release and `IsThrowHeld` so states can distinguish tap (quick throw / recall) from hold (aim / hack) — that decision belongs to player states, not the reader. If a `NullReferenceException` appears in `EnableGameplay` (SO `OnEnable` ordering), add a lazy init guard there.

#### `Scripts/Input/CursorLock.cs`
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace Construct.Input
{
    public sealed class CursorLock : MonoBehaviour
    {
        private void Start()
        {
            Lock();
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Unlock();
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
                     && Cursor.lockState != CursorLockMode.Locked)
                Lock();
        }

        private static void Lock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
```
Temporary editor comfort script; the pause menu (week 5) will own the cursor.

#### `Scripts/Player/PlayerMotor.cs`
```csharp
using UnityEngine;

namespace Construct.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 40f;
        [SerializeField] private float _rotationSpeed = 720f;

        [Header("Jump & Gravity")]
        [SerializeField] private float _jumpHeight = 1.4f;
        [SerializeField] private float _gravity = -25f;
        [SerializeField] private float _groundedStickForce = -2f;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        public bool IsGrounded => _controller.isGrounded;
        public Vector3 Velocity => _horizontalVelocity + Vector3.up * _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        // direction is in world space; its length (0 to 1) scales the speed
        public void Move(Vector3 direction, float speedMultiplier, float deltaTime)
        {
            direction.y = 0f;
            direction = Vector3.ClampMagnitude(direction, 1f);

            Vector3 targetVelocity = direction * (_moveSpeed * speedMultiplier);
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, targetVelocity, _acceleration * deltaTime);

            ApplyGravity(deltaTime);

            Vector3 motion = _horizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(motion * deltaTime);
        }

        public void FaceDirection(Vector3 direction, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, _rotationSpeed * deltaTime);
        }

        public void Jump()
        {
            if (!IsGrounded)
                return;

            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        private void ApplyGravity(float deltaTime)
        {
            if (IsGrounded && _verticalVelocity < 0f)
                _verticalVelocity = _groundedStickForce;
            else
                _verticalVelocity += _gravity * deltaTime;
        }
    }
}
```
Rule: `Move` must be called **every frame**, even with zero direction, because it applies gravity. `speedMultiplier` lets the Aim state move at 50% without the motor knowing about aiming.

#### `Scripts/Player/PlayerState.cs`
```csharp
using Construct.Core;

namespace Construct.Player
{
    public abstract class PlayerState : IState
    {
        protected PlayerController Ctx { get; }

        protected PlayerState(PlayerController ctx)
        {
            Ctx = ctx;
        }

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() { }
    }
}
```

#### `Scripts/Player/States/LocomotionState.cs`
```csharp
using UnityEngine;

namespace Construct.Player
{
    public sealed class LocomotionState : PlayerState
    {
        public LocomotionState(PlayerController ctx) : base(ctx) { }

        public override void Enter()
        {
            Ctx.Input.JumpPressed += OnJumpPressed;
        }

        public override void Exit()
        {
            Ctx.Input.JumpPressed -= OnJumpPressed;
        }

        public override void Tick(float deltaTime)
        {
            Vector3 moveDirection = Ctx.GetCameraRelativeMove();

            Ctx.Motor.Move(moveDirection, 1f, deltaTime);
            Ctx.Motor.FaceDirection(moveDirection, deltaTime);
        }

        private void OnJumpPressed()
        {
            Ctx.Motor.Jump();
        }
    }
}
```
Jump lives here temporarily; it will move to an `AirborneState` (aerial combo) later.

#### `Scripts/Player/PlayerController.cs`
```csharp
using Construct.Core;
using Construct.Input;
using UnityEngine;

namespace Construct.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private Transform _cameraTransform;

        public InputReader Input => _input;
        public PlayerMotor Motor { get; private set; }
        public StateMachine States { get; private set; }

        // States, created once and reused
        public LocomotionState Locomotion { get; private set; }

        private void Awake()
        {
            Motor = GetComponent<PlayerMotor>();
            States = new StateMachine();

            Locomotion = new LocomotionState(this);
        }

        private void OnEnable()
        {
            _input.EnableGameplay();
        }

        private void OnDisable()
        {
            _input.DisableGameplay();
        }

        private void Start()
        {
            States.ChangeState(Locomotion);
        }

        private void Update()
        {
            States.Tick(Time.deltaTime);
        }

        public Vector3 GetCameraRelativeMove()
        {
            Vector2 input = _input.Move;

            Vector3 forward = _cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = _cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return forward * input.y + right * input.x;
        }
    }
}
```
The controller is a coordinator only: creates pieces, enables input, ticks the state machine. All behavior lives in states.

#### `Scripts/Core/ITargetable.cs`
```csharp
using UnityEngine;

namespace Construct.Core
{
    public interface ITargetable
    {
        Transform AimPoint { get; }
        bool IsAlive { get; }
    }
}
```

#### `Scripts/Combat/TrainingDummy.cs`
```csharp
using Construct.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Construct.Combat
{
    public sealed class TrainingDummy : MonoBehaviour, ITargetable
    {
        // FormerlySerializedAs keeps the reference already set on the prefab
        [FormerlySerializedAs("aimPoint")]
        [SerializeField] private Transform _aimPoint;

        public Transform AimPoint => _aimPoint != null ? _aimPoint : transform;
        public bool IsAlive => isActiveAndEnabled;
    }
}
```
Scene setup: Capsule named `TrainingDummy` on layer **`Targetable`** (keep its CapsuleCollider — needed by `Physics.OverlapSphere`), child empty `AimPoint` at local Y ≈ 0.5, assigned in the field, made into a prefab, 3 instances at different distances/angles around the player, different material color than KERNEL. Disabling a dummy at runtime = "dead" (useful to test auto-release of lock).

#### `Scripts/Camera/` (assembly `Construct.Cameras`)
`PlayerCameraInput` and `CameraProximityHider` — read them in the repo. Settings in section 6.

---

## 7. Next steps

### Animations (phase 03)
- Source: **Kevin Iglesias "Human Animations"** in `Assets/ThirdParty/` (git-ignored, Asset Store license). Already Humanoid with one shared avatar `HumanM_ModelAvatar`, in-place loops. Use the `HumanM@` (male) clips.
- `Assets/_Project/Animations/Player/Kernel.controller`: parameters `Speed`, `MoveX`, `MoveZ`, `IsLockedOn`. States `Locomotion` (1D: Idle01 / Walk01_Forward 0.35 / Run01_Forward 1) and `Strafe` (2D freeform: Walk01_* ring at 0.35, Run01_* ring at 1), 0.15 s transitions on `IsLockedOn`.
- `PlayerAnimator` reads `PlayerMotor.Velocity / MaxSpeed` in local space; nothing else touches the Animator.
- The pack covers: 8-dir walk/run, idle, 1H attacks ×5 (L/R), 2H attacks ×4, combat idle, Dodge01, CombatDamage01/02, CombatDeath01–04, Stun01 (good for Breach).
- Missing, get from Mixamo later: jump (up / fall / land), sword throw + catch, aerial attacks + smash, launcher, hack pose.

### Lock-on — done (2026-10-21)
`LockOnSystem` (scoring, toggle, auto-release, `TrySwitch`), `LookFlickDetector`, strafe in `LocomotionState`, `ICameraRig` + `CinemachineCameraRig` with the lock camera. The original plan is kept below for reference. Next: phase 03 (mannequin + animations), see the Blueprint.

1. **Piece 1 — `ITargetable` + `TrainingDummy`** — done.
2. **Piece 2 — `LockOnSystem`** (`Scripts/Weapon/LockOnSystem.cs`, namespace `Construct.Weapon`):
   - Toggle on `InputReader.LockOnPressed`.
   - Find candidates with `Physics.OverlapSphere` (or `OverlapSphereNonAlloc` with a cached buffer) filtered by the `Targetable` layer mask; pick the best by distance + angle from camera forward; optional line-of-sight check.
   - Switch target with right stick flick (`InputReader.Look`) while locked: threshold + cooldown/reset when stick returns to center; choose nearest candidate in that screen direction.
   - Auto-release when target `!IsAlive` or beyond a max range.
   - Expose `CurrentTarget` and an event `TargetChanged(ITargetable)`.
   - While locked, player should face the target (Locomotion uses target direction for `FaceDirection`, strafe movement).
3. **Piece 3 — camera framing through `ICameraRig`**: create `ICameraRig` (`SetLockTarget`, `ClearLockTarget`) and `CinemachineCameraRig` (the only Cinemachine-aware class). Likely approach: a second `CinemachineCamera` for lock-on (follow player, look at target / group framing) with higher priority when locked, blend between orbit and lock cameras. Recenter orbit behind player when unlocking.

Then a simple target reticle above the locked dummy (UI comes later via event channel, so a temporary debug gizmo is fine).

### Rest of week 1
- Free aim (`AimSystem`, aim Cinemachine camera, reticle color, 70% slow-mo, aim assist) — prototype early, it's a listed risk.
- Light combo + `ComboController` + `InputBuffer`.
- **Choose the combat animation pack before coding combos** (risk #1), swap the capsule `Visual` for the mannequin + Animator.

### Week 2 (gate: "Combat locked")
Hitbox/Hurtbox + `HitInfo` + `Combatant`/`Health`/`Integrity`, `HitFeedback` (hitstop, impulse, flash), Dodge + Perfect Dodge, first enemy **Ping** with `EnemyBrain` FSM + `AttackTokenManager`, Breach. One fun arena with a Ping and a hack = gate passed.

Then follow the production plan (section 2).

---

## 8. Related context
- Design docs (claude.ai, editable): *Construct — Game Design Document* and *Construct — Architecture du code* — this file summarizes both; class diagrams there show the same classes as section 3.
- Separate project, do not mix: *L'Art des Glyphes* (another Unity 6 RPG). Don't reuse its code/assets here.
- Future: his ASP.NET Core / PostgreSQL leaderboard API could plug into end-of-program grades (post-1.0, out of scope).
