# Project Overview
- **Game Title**: SAO: Ordinal Scale - AR Wave Combat
- **High-Level Concept**: An Augmented / Mixed Reality wave survival game inspired by *Sword Art Online: Ordinal Scale*, where players scan their physical room to establish combat boundaries, equip legendary swords from an AR holographic menu or shoulder sheaths, and defend against waves of fantasy monsters spawning through walls and ceilings.
- **Players**: Single Player (XR / AR First-Person)
- **Inspiration / Reference Games**: *Sword Art Online: Ordinal Scale*, *Until You Fall*, *Gorn*, *Superhot VR*
- **Tone / Art Direction**: Futuristic AR holographic interface (SAO dark-blue and neon cyan aesthetic) blended with realistic monster encounters in passthrough MR.
- **Target Platform**: Android (Meta Quest 3 / Quest Pro / Quest 2 via OpenXR & Meta XR SDK) + Unity Editor PlayMode simulation.
- **Screen Orientation / Resolution**: XR Stereoscopic (Per-eye native resolution, 90Hz/120Hz refresh).
- **Render Pipeline**: Built-in Render Pipeline (Standard shaders & UI Toolkit).

---

# Game Mechanics

## Core Gameplay Loop
1. **Room Calibration & Scanning**: The player launches into passthrough AR. The game scans room geometry (walls, ceiling, floor) using Meta XR Scene SDK (with Editor mock-room fallback).
2. **Main Menu ("LINK START")**: Once room calibration is complete, the SAO Main Menu appears floating in front of the player with the title and "LINK START" button.
3. **Weapon Selection & Holstering**: The player opens the AR Sword Carousel (pressing `M`/`Space` in Editor or XR Controller Primary Button `A`/`X`), grabs a sword (or dual-wields two), and can sheath extra blades onto left/right shoulder slots.
4. **Wave Combat**: Enemies (Beholders and Chest Monsters) spawn dynamically from scanned wall anchors and ceiling breaches. Enemies pathfind toward the player while avoiding room obstacles.
5. **Active Melee Combat**: The player physically slashes enemies. Each swing connects via blade trigger colliders, consuming player stamina and inflicting weapon-specific damage. Enemies counter-attack if within melee range.
6. **Wave Progression & Score**: Defeating all enemies clears the wave, increments the wave counter, awards score, and initiates the next wave with increased enemy count.
7. **Game Over & Retry**: If player HP reaches 0, the Game Over screen displays final score and highest wave achieved with a "RETRY" button to restart the session.

## Controls and Input Methods
- **XR Controllers**:
  - **Grip Button**: Grab sword from holographic menu / shoulder holster / world; release or throw sword.
  - **Primary Button (A / X)**: Toggle holographic Sword Carousel menu.
  - **Trigger / Point**: Interact with UI Toolkit buttons (LINK START, RETRY).
- **XR Hand Tracking (Meta Comprehensive Interaction Rig)**:
  - **Palm Grab / Pinch**: Grab sword handle, unsheath from shoulder.
  - **Index Poke / Ray Pinch**: Press UI menu buttons.
- **Keyboard & Mouse (Unity Editor Fallback)**:
  - **M Key / Spacebar**: Toggle Sword Carousel menu.
  - **Mouse Click / Ray**: Interact with UI and grab test objects.

---

# UI

## Screen Flow & Wireframes

```
+-------------------------------------------------------------+
|                     1. SCANNING SCREEN                      |
|                                                             |
|              [ Room Scanning in Progress... ]               |
|               "Please look around the room."                |
|                    [ Scan Progress Bar ]                    |
+-------------------------------------------------------------+
                              |
                              v (SceneModelLoaded)
+-------------------------------------------------------------+
|                     2. MAIN MENU SCREEN                     |
|                                                             |
|                    SAO: ORDINAL SCALE                       |
|               << AR Mixed Reality Combat >>                 |
|                                                             |
|                     [  LINK START  ]                        |
|                                                             |
+-------------------------------------------------------------+
                              |
                              v (PlayButton.clicked)
+-------------------------------------------------------------+
|                      3. IN-GAME HUD                         |
|                                                             |
|  [HP] [====================]      Wave: 1                   |
|  [SP] [==============      ]      Enemies Remaining: 3      |
|                                   Score: 1200               |
+-------------------------------------------------------------+
                              |
                              v (Player HP <= 0)
+-------------------------------------------------------------+
|                    4. GAME OVER SCREEN                      |
|                                                             |
|                        GAME OVER                            |
|                       Score: 3500                           |
|                         Wave: 4                             |
|                                                             |
|                        [ RETRY ]                            |
+-------------------------------------------------------------+
```

## Styling & Theme (`Assets/UI/SAOStyles.uss`)
- **Theme**: SAO high-tech interface with semi-transparent dark backgrounds (`rgba(10, 15, 26, 0.85)`), electric cyan borders (`#00f0ff`), white bold typography, and distinct health red (`#e74c3c`) / stamina green (`#2ecc71`) bar indicators.
- **World-Space Placement**: The `UIDocument` is rendered in world-space floating 1.5–2.0 meters directly in front of the player's view (`CenterEyeAnchor`) with auto-billboarding orientation.

---

# Key Asset & Context

### 1. ScriptableObjects & Weapon Assets
- **`Assets/Data/Swords/`**:
  - `Sword_Elucidator.asset` (Sword A: High damage 35, Stamina cost 25, Dark cyan theme)
  - `Sword_DarkRepulser.asset` (Sword B: Fast swing, Damage 28, Stamina cost 18, Aquamarine theme)
  - `Sword_LambentLight.asset` (Sword C: Rapier style, Damage 22, Stamina cost 12, Gold/White theme)
  - `Sword_Liberator.asset` (Sword D: Heavy broadsword, Damage 45, Stamina cost 35, Red/Silver theme)
  - `Sword_AnnealBlade.asset` (Sword E: Balanced starter, Damage 20, Stamina cost 15, Blue theme)
- **`Assets/Prefabs/Weapons/`**:
  - `Prefab_Sword_A.prefab` through `Prefab_Sword_E.prefab` configured with `MeshFilter`, `MeshRenderer`, `BoxCollider`/`CapsuleCollider` (blade trigger), `Rigidbody` (kinematic in hand), `SwordInteraction`, and `SwordDamageTrigger`.

### 2. Enemy Assets
- **`Assets/RPGMonsterPartnersPBRPolyart/Prefabs/Character/`**:
  - `BeholderPBRDefault.prefab` (Floating eye enemy, ranged/melee bite, `EnemyAI`, `EnemyStats`, tag `"Enemy"`)
  - `ChestMonsterPBRDefault.prefab` (Floor/ground enemy, jump attack, `EnemyAI`, `EnemyStats`, tag `"Enemy"`)

### 3. UI Assets & Documents
- `Assets/UI/ScanningScreen.uxml`: Room calibration screen
- `Assets/UI/MainMenuScreen.uxml`: "SAO: Ordinal Scale" title + "LINK START" button
- `Assets/UI/HUDScreen.uxml`: Player HP/SP bars + Wave & Score telemetry
- `Assets/UI/GameOverScreen.uxml`: Game over summary + Retry button
- `Assets/UI/SAOStyles.uss`: Neon-cyan SAO stylesheet

### 4. Core Scripts & Enhancements
- `Assets/Scripts/ARGameManager.cs`: Game state orchestrator (`Scanning` -> `MainMenu` -> `Gameplay` -> `GameOver`), wave progression, score accumulation.
- `Assets/Scripts/ARSceneManager.cs`: Meta XR Scene plane categorization + **Editor Mock Room Fallback** (spawns 4 walls, floor, ceiling when running in Unity Editor).
- `Assets/Scripts/EnemySpawnManager.cs`: Spawns enemies from random walls/ceilings with entry hole FX.
- `Assets/Scripts/EnemyAI.cs` & `EnemyStats.cs`: Chases player camera, attack triggers, hit detection, HP billboard canvas.
- `Assets/Scripts/SwordInteraction.cs`: Grip grab, shoulder holster sheathing/unsheathing, throwing mechanics.
- `Assets/Scripts/SwordMenuManager.cs`: Spawns curved fan of sword options in front of player.
- `Assets/Scripts/PlayerStats.cs`: HP, SP regen/drain, damage processing, and HUD synchronization.
- `Assets/Scripts/UIManager.cs`: Manages `UIDocument` template swaps and button event binding.

---

# Implementation Steps

### Step 1: Create Sword Data Assets and Sword Prefabs
- **Description**:
  1. Create 5 `SwordData` ScriptableObject assets in `Assets/Data/Swords/` (`Sword_Elucidator`, `Sword_DarkRepulser`, `Sword_LambentLight`, `Sword_Liberator`, `Sword_AnnealBlade`) referencing the corresponding FBX models from `Assets/weapon pack/fbx(unity)/`.
  2. Create 5 prefab variants in `Assets/Prefabs/Weapons/` with blade trigger colliders, `Rigidbody`, `SwordInteraction`, and `SwordDamageTrigger` attached.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Configure Enemy Prefabs (Beholder & Chest Monster)
- **Description**:
  1. Inspect and configure `BeholderPBRDefault.prefab` and `ChestMonsterPBRDefault.prefab`.
  2. Ensure tag `"Enemy"` is set, attach `EnemyAI` and `EnemyStats`, configure hit colliders and world-space health bar billboard canvas.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 3: Enhance ARSceneManager with Editor Mock Room Simulation
- **Description**:
  1. Update `ARSceneManager.cs` so that if running in the Unity Editor or if `OVRSceneManager` has no room data, it automatically creates a mock room (4 walls, ceiling, floor with proper dimensions and `OVRScenePlane` / mock anchors) and triggers `OnSceneLoaded`.
  2. This guarantees instant seamless testing without requiring physical Quest headset deployment every iteration.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 4: Enhance UIManager and SAO UI Toolkit Screens
- **Description**:
  1. Refine `MainMenuScreen.uxml`, `HUDScreen.uxml`, `ScanningScreen.uxml`, and `GameOverScreen.uxml` with SAO visual design and responsive layouts.
  2. Update `SAOStyles.uss` with glowing borders, hover states, and health/stamina fill animations.
  3. Ensure `UIManager.cs` dynamically binds to `PlayButton` ("LINK START") to call `ARGameManager.Instance.StartGame()`, `RetryButton` to restart the game, and continuously updates the HUD from `PlayerStats`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 5: Wire Sword System & Inventory into Scene
- **Description**:
  1. Update `SwordMenuManager` on `SwordManager` GameObject in `SampleScene.unity` with the 5 created `SwordData` assets.
  2. Assign camera (`CenterEyeAnchor`), left hand anchor (`LeftHandAnchor`), and right hand anchor (`RightHandAnchor`).
  3. Add `PlayerInventoryManager` with shoulder holster equipment slots (`EquipmentSlot`).
  4. Ensure sword damage calculation reads `SwordData.damage` and updates `PlayerStats`.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 6: Assemble Core Managers in Scene and Connect Gameplay Loop
- **Description**:
  1. Instantiate and configure `ARGameManager`, `ARSceneManager`, `EnemySpawnManager`, `UIManager` (with `UIDocument`), and `PlayerStats` in `SampleScene.unity`.
  2. Link references between `ARGameManager`, `EnemySpawnManager`, `ARSceneManager`, and `UIManager`.
  3. Set enemy prefabs array in `EnemySpawnManager` to the configured Beholder and Chest Monster prefabs.
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 3, Step 4, Step 5
- **Parallelizable**: No

### Step 7: System Sanity Check & PlayMode Verification
- **Description**:
  1. Run `SAOSystemSanityCheck.cs` in PlayMode to verify 0 null reference warnings.
  2. Test the full loop:
     - Scanning -> Main Menu appears with "SAO: Ordinal Scale" & "LINK START".
     - Press "LINK START" -> Wave 1 begins, HUD displays HP, SP, Wave: 1, Enemies: 3.
     - Press `M` / Controller Primary button -> Sword menu appears, grab sword, slash enemy -> Enemy takes damage, dies, score increases.
     - Clear wave -> Wave 2 triggers with scaled enemy count.
     - Player HP depletion -> Game Over screen with final score and Retry button.
- **Assigned role**: developer
- **Dependencies**: Step 6
- **Parallelizable**: No

---

# Verification & Testing

1. **Room Setup Verification**:
   - In Editor PlayMode: Verify mock room walls and ceiling are created and enemy spawn points are valid.
   - On Quest Device: Verify `OVRSceneManager` scans physical room surfaces and debug planes highlight scanned walls/ceiling.
2. **Main Menu & UI Flow Verification**:
   - Verify Main Menu displays "SAO: Ordinal Scale" and "LINK START" button.
   - Click "LINK START": Check state transition from `MainMenu` to `Gameplay`, HUD becomes active and shows HP=100%, SP=100%, Wave=1.
3. **Sword System 100% Verification**:
   - Press `M` / `Space` / XR Primary Button: Verify 5 swords fan out in front of the player.
   - Grab sword: Verify sword attaches to hand anchor.
   - Shoulder Sheath: Move sword near shoulder -> verifies sword snaps into shoulder holster.
   - Slash Enemy: Collision triggers hit sound/log, reduces enemy HP, consumes player stamina.
4. **Wave Combat & Score Loop**:
   - Verify Beholders and Chest Monsters spawn out of walls/ceiling.
   - Verify enemies walk towards player and execute attacks.
   - Defeating all wave enemies triggers `OnWaveCleared` and starts Wave 2 after `waveInterval` (10s).
   - Let player take damage until HP reaches 0 -> verify Game Over screen displays accurate final score and wave.
   - Click "RETRY" -> scene reloads or restarts cleanly.
