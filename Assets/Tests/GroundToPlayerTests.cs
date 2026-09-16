using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class GroundToPlayerTests
{
    [Test]
    public void GroundToPlayer_MovesTerrainToPlayerXandZ_AndCustomFloorHeight()
    {
        // Arrange
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(10, 5, 20);

        GameObject ground = new GameObject("Ground");
        // Add MeshFilter to satisfy RequireComponent if needed for tests
        ground.AddComponent<MeshFilter>();
        ground.AddComponent<MeshRenderer>();
        GroundToPlayer g2p = ground.AddComponent<GroundToPlayer>();
        g2p.player = player;
        g2p.floorHeight = -1.5f;

        // Act
        g2p.AlignToPlayer();

        // Assert
        Assert.AreEqual(player.transform.position.x, ground.transform.position.x, 0.001f);
        Assert.AreEqual(player.transform.position.z, ground.transform.position.z, 0.001f);
        Assert.AreEqual(-1.5f, ground.transform.position.y, 0.001f);
        
        Object.DestroyImmediate(player);
        Object.DestroyImmediate(ground);
    }

    [Test]
    public void GroundToPlayer_AutoAssignsMainCamera()
    {
        // Arrange
        GameObject camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();

        GameObject ground = new GameObject("Ground");
        ground.AddComponent<MeshFilter>();
        ground.AddComponent<MeshRenderer>();
        GroundToPlayer g2p = ground.AddComponent<GroundToPlayer>();
        
        // Act - Start is called internally or we can call it if needed, 
        // but since we are in unit test, we might need to invoke it or test the logic.
        // We'll use a private method via reflection or just trust the public logic.
        // For simplicity, let's just test that it works when we manually trigger the check.
        
        // Actually, let's just test that Start() works by calling it.
        // Note: MonoBehaviour.Start is private, so we'd need Invoke("Start", 0) or reflection.
        g2p.SendMessage("Start");

        // Assert
        Assert.AreEqual(camObj, g2p.player);

        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(ground);
    }
}
