using UnityEngine;

public class EquipmentSlot : MonoBehaviour
{
    public enum SlotType { ShoulderLeft, ShoulderRight }
    public SlotType slotType;
    public SwordInteraction slottedSword;
    public Transform playerCamera;
    
    public Vector3 offset = new Vector3(0.25f, -0.3f, -0.2f); // Default for right shoulder

    void Start()
    {
        if (playerCamera == null && PlayerInventoryManager.Instance != null)
        {
            playerCamera = PlayerInventoryManager.Instance.playerCamera;
        }
        if (playerCamera == null) playerCamera = Camera.main?.transform;
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Position slot relative to camera
        Vector3 shoulderOffset = playerCamera.right * (slotType == SlotType.ShoulderRight ? offset.x : -offset.x);
        shoulderOffset += playerCamera.up * offset.y;
        shoulderOffset += playerCamera.forward * offset.z;

        transform.position = playerCamera.position + shoulderOffset;
        // Keep rotation aligned with player body (simplified as camera Y rotation)
        transform.rotation = Quaternion.Euler(0, playerCamera.eulerAngles.y, 0);
    }
}
