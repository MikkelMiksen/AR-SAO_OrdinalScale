using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundToPlayer : MonoBehaviour
{
    [Tooltip("The player or camera object to follow. If null, will try to find the Main Camera.")]
    public GameObject player;
    
    [Tooltip("Fixed height of the ground. In Meta Quest 'Floor Level' tracking, this should typically be 0.")]
    public float floorHeight = 0f;

    [Tooltip("Physics Material for the ground.")]
    public PhysicsMaterial floorPhysicsMaterial;

    [Tooltip("Scale of the ground plane. A large scale allows walking further away from the player center.")]
    public float groundScale = 1000f;

    private void Start()
    {
        // Ensure we have a large collider to catch the player
        var collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.convex = false;
            if (floorPhysicsMaterial != null) collider.material = floorPhysicsMaterial;
        }

        transform.localScale = new Vector3(groundScale, 1f, groundScale);
        transform.position = new Vector3(0, floorHeight, 0);
        
        // Ensure the mesh is oriented correctly (upwards)
        transform.rotation = Quaternion.identity;

        // Try to find the PlayerController and disable its gravity factor
        var playerController = GameObject.Find("PlayerController");
        if (playerController != null)
        {
             // We use reflection or a generic approach if we don't have the Meta type explicitly
             var components = playerController.GetComponents<MonoBehaviour>();
             foreach(var comp in components)
             {
                 if(comp.GetType().Name == "PlayerController")
                 {
                     var field = comp.GetType().GetField("_gravityFactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                     if(field != null) field.SetValue(comp, 0f);
                     Debug.Log("[SAO] Disabled gravity on PlayerController.");
                 }
             }
        }
    }

    private void Update()
    {
        AlignToPlayer();
        CheckForARFloor();
    }

    private void CheckForARFloor()
    {
        var arSceneManager = FindObjectOfType<ARSceneManager>();
        if (arSceneManager != null)
        {
            arSceneManager.UpdateFloorAlignment(this.transform);
        }
    }

    /// <summary>
    /// Aligns the ground plane to the player's X and Z position while keeping it at the specified floor height.
    /// </summary>
    public void AlignToPlayer()
    {
        // Keep the floor under the player even if they teleport
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            pos.y = floorHeight;
            transform.position = pos;
        }
        else
        {
            if (Camera.main != null) player = Camera.main.gameObject;
        }
    }
}
