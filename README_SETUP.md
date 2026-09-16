### AR SAO: Ordinal Scale - Setup Guide

This guide details how to set up the AR Room Scanning, Scene Recreation, Furniture Inclusion, and UI for the project.

#### 1. Room Scanning & Scene Recreation
The project uses the **Meta XR Scene SDK** to scan and recreate your environment.

- **ARSceneManager**: This is the core component. Ensure it is attached to a GameObject in your scene (or the `[SAO_Initializer]` prefab).
- **Categorization**: The system automatically identifies `Floor`, `Ceiling`, and `WallFace`. 
- **Furniture (Tables, Desks, etc.)**: We have updated `ARSceneManager.cs` to explicitly handle classifications beyond walls and floors. Any anchor classified as a `Desk`, `Table`, `Couch`, etc., will be added to the `Furniture` list.
- **Physics**: 
  - The `Floor` is automatically assigned a `PhysicsMaterial` (e.g., `BouncyFloor`) to ensure proper player/enemy interaction.
  - Furniture items automatically receive a `BoxCollider` if they don't have one, ensuring they aren't ignored during gameplay (enemies will navigate around them or players can interact with them).

#### 2. Anchors & Alignment
- **Floor Anchor**: The `ARSceneManager` identifies the floor level. The `GroundToPlayer` script uses this to ensure the virtual ground always aligns with your real-world floor.
- **Wall & Ceiling Anchors**: These are used as spawn points for enemies.

#### 3. Procedural Enemy Spawning
Enemies are spawned procedurally based on the scanned geometry:
- **Spawn Logic**: `EnemySpawnManager` selects a random point on a scanned `Wall` or `Ceiling`.
- **Holes/Portals**: Before an enemy appears, a "Wall Hole" or "Ceiling Breakthrough" effect is instantiated at the spawn point.
- **Variety**: You can assign multiple enemy prefabs to the `EnemySpawnManager` to vary the challenge.

#### 4. Game Flow & State Transitions
Transitions are managed by `ARGameManager`:
1.  **Scanning State**: Triggered on start. The Meta Scene SDK prompt will appear.
2.  **Main Menu State**: Once the scene model is loaded, the UI switches to the Main Menu.
3.  **Gameplay State**: Triggered by "Link Start" (Play button). This starts the wave system.
4.  **GameOver State**: Triggered when player health reaches zero.

#### 5. Headset-Following UI
The `UIManager` handles all UI displays (HUD, Menus) using Unity's **UI Toolkit**.
- **Following Movement**: The UI GameObject uses a `PositionCanvas` method in its `Update` loop to smoothly `Lerp` towards a position in front of the player's headset (`Camera.main`). 
- **Orientation**: The UI always faces the player, ensuring it is readable regardless of where they move in the room.

### How to use:
1.  Open the `SampleScene`.
2.  Ensure the `OVRCameraRig` is present.
3.  Assign your Enemy Prefabs to the `EnemySpawnManager` on the `[SAO_GameManager]` object.
4.  Build and Run on your Meta Quest device.
5.  Follow the on-screen instructions to scan your room!
