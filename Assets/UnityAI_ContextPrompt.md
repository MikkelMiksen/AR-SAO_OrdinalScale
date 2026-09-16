### Unity AI Context Prompt: AR SAO Ordinal Scale Project

**General Idea:**
Build an AR "First Encounter" style hack-and-slash game (Sword Art Online: Ordinal Scale themed) for Meta Quest. The game uses real-world room scanning to spawn monsters from walls and ceilings. The player fights waves of monsters using physical swords with health/stamina mechanics.

**3-Step Game Loop:**
1.  **Scanning Phase:** Use Meta XR Scene SDK to scan the room. Surfaces (Walls, Ceilings, Floors) must be identified. Visual debug planes should appear on these surfaces during scanning.
2.  **Main Menu Phase:** Once scanning is complete, a VR world-space UI appears with a "Play" button.
3.  **Gameplay Phase:** Start a wave-based combat system. Monsters spawn from holes in walls or breakthroughs in the ceiling. The player uses swords to defeat them.

---

**Scene Hierarchy & Object Placement Instructions:**

1.  **Project Initialization:**
    - Place the `ProjectInitializer` script on a new GameObject named `[SAO_System]`.
    - Assign the required manager prefabs (`ARGameManager`, `ARSceneManager`, `UIManager`, `SwordInitializer`) to the `ProjectInitializer` component.

2.  **AR Setup (ARSceneManager):**
    - Ensure `OVRSceneManager` is present in the scene (or instantiated by `ARSceneManager`).
    - Configure `ARSceneManager` with debug materials for Walls, Ceilings, and Floors.
    - Set up `OVRSceneManager` to include `WallFace`, `Ceiling`, and `Floor` classifications.

3.  **UI Setup (UIManager):**
    - The `UIManager` uses Unity UI Toolkit (`UIDocument`).
    - Attach a `UIDocument` to the `UIManager` GameObject.
    - Set the `UIDocument`'s Panel Settings to a World Space configuration suitable for VR.
    - Assign the UXML templates for `ScanningScreen`, `MainMenuScreen`, `HUDScreen`, and `GameOverScreen`.
    - Apply the `SAOStyles.uss` stylesheet to these templates.

4.  **Combat & Spawning (EnemySpawnManager):**
    - Attach `EnemySpawnManager` to the `ARGameManager` or a separate `[Spawning]` GameObject.
    - Link `ARSceneManager` to the `EnemySpawnManager`.
    - Provide an array of `enemyPrefabs`. These prefabs should have `MeshColliders` and `Rigidbody` (set to Kinematic or handled by AI).
    - Provide `wallHolePrefab` and `ceilingBreakthroughPrefab` for visual spawning effects.

5.  **Player & Environment:**
    - The player needs a `PlayerStats` component.
    - Ensure the player has colliders with the `NonBouncyPlayer` physics material.
    - The scanned floor must have the `BouncyFloor` physics material applied (handled by `ARSceneManager`).

---

**Script Integration & Logic:**

- **ARGameManager:** The brain of the game. Transitions states: `Scanning` -> `MainMenu` -> `Gameplay` -> `GameOver`. It triggers wave spawning via `EnemySpawnManager` and UI updates via `UIManager`.
- **EnemyAI:** Monsters should move towards the player (`Camera.main.transform`) while avoiding obstacles. Use physics overlap checks against scanned room meshes (Layer "Default" or "Room").
- **Sword Mechanics:** Swords must have `SwordDamageTrigger` to deal damage to enemies (Tag: "Enemy") and `SwordInteraction` for physics-based throwing/bouncing.
- **HUD Updates:** `PlayerStats` automatically pushes health, stamina, and score updates to the `UIManager`'s HUD screen using the `UpdateHUD` method.

**Missing Elements to Build/Handle:**
- **Visuals:** Create simple "hole" prefabs (e.g., dark decals or cracked mesh) for enemy entry points.
- **UI Design:** Ensure UXML files have elements named `HealthBarFill`, `StaminaBarFill`, `WaveText`, `EnemyCountText`, `ScoreText`, `PlayButton`, `FinalScoreText`, `FinalWaveText`, and `RetryButton` to match `UIManager` queries.
- **Monster Placement:** `EnemySpawnManager` handles logic for picking points on scanned walls/ceilings; ensure the monster prefabs are correctly oriented (facing the room).