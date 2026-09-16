using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

public class SwordMenuManager : MonoBehaviour
{
    public SwordData[] swordDataList;
    public Transform playerCamera;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Menu Settings")]
    public float spawnDistance = 0.5f;
    public float menuHeightOffset = -0.2f;

    private bool _menuActive = false;
    private GameObject[] _activeSwords;

    void Start()
    {
        Debug.Log("[SAO] SwordManager Start called.");
        InitializeAnchors();
    }

    public void InitializeAnchors()
    {
        if (playerCamera == null)
        {
            var cam = Camera.main;
            if (cam != null) playerCamera = cam.transform;
            Debug.Log("[SAO] Found main camera: " + (playerCamera != null));
        }

        if (leftHand == null || rightHand == null)
        {
            var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.name == "LeftHandAnchor") leftHand = t;
                if (t.name == "RightHandAnchor") rightHand = t;
            }
        }
        
        Debug.Log($"[SAO] Anchors - Left: {leftHand != null}, Right: {rightHand != null}");
    }

    void Update()
    {
        try 
        {
            // Keyboard Support (New Input System)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.mKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    ToggleMenu(!_menuActive);
                }
            }

            // Meta Quest 3 / XR Controller Support
            if (InputSystem.devices.Count > 0)
            {
                foreach (var device in InputSystem.devices)
                {
                    if (device is UnityEngine.InputSystem.XR.XRController)
                    {
                        var primaryButton = device.TryGetChildControl<ButtonControl>("primaryButton"); // X/A button
                        if (primaryButton != null && primaryButton.wasPressedThisFrame)
                        {
                            Debug.Log($"[SAO] Primary button pressed on {device.name}");
                            ToggleMenu(!_menuActive);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            // Suppress errors during early initialization but log them
            // Debug.LogWarning("[SAO] Input check failed: " + e.Message);
        }
    }

    public void ToggleMenu(bool active)
    {
        if (active)
        {
             SpawnSwordMenu();
        }
        else
        {
            if (_activeSwords != null)
            {
                foreach (var sword in _activeSwords)
                {
                    if (sword != null)
                    {
                         // If not equipped or sheathed, we could destroy or hide
                         // For SAO "Inventory", they should probably go back to backpack if not in hand/shoulder
                         SwordInteraction si = sword.GetComponent<SwordInteraction>();
                         if (si != null && (si.IsEquipped || si.IsSheathed))
                         {
                             // Do not destroy if player is using it
                             continue;
                         }
                         Destroy(sword);
                    }
                }
                _activeSwords = null;
            }
            _menuActive = false;
        }
        Debug.Log($"[SAO] Menu toggled: {active}");
    }

    public void SpawnSwordMenu()
    {
        if (_menuActive)
        {
            ToggleMenu(false);
            return;
        }

        if (swordDataList == null || swordDataList.Length == 0)
        {
            Debug.LogWarning("[SAO] No sword data assigned to Menu Manager.");
            return;
        }

        if (playerCamera == null)
        {
            if (Camera.main != null) playerCamera = Camera.main.transform;
            if (playerCamera == null) return;
        }

        if (leftHand == null || rightHand == null)
        {
            var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.name == "LeftHandAnchor") leftHand = t;
                if (t.name == "RightHandAnchor") rightHand = t;
            }
        }

        _activeSwords = new GameObject[swordDataList.Length];
        
        Vector3 center = playerCamera.position + playerCamera.forward * spawnDistance;
        center.y += menuHeightOffset;

        for (int i = 0; i < swordDataList.Length; i++)
        {
            SwordData data = swordDataList[i];
            if (data == null || data.prefab == null) continue;

            float angle = (i - (swordDataList.Length - 1) / 2f) * 25f;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 offset = rotation * Vector3.forward * 0.35f + rotation * Vector3.right * 0.1f;
            
            GameObject sword = Instantiate(data.prefab, center + offset, rotation * playerCamera.rotation);
            sword.name = $"SAO_{data.swordName}_{i}";
            
            var rb = sword.GetComponent<Rigidbody>();
            if (rb == null) rb = sword.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            SwordInteraction interaction = sword.GetComponent<SwordInteraction>();
            if (interaction == null) interaction = sword.AddComponent<SwordInteraction>();
            interaction.swordData = data;
            interaction.leftHand = leftHand;
            interaction.rightHand = rightHand;

            var damageTrigger = sword.GetComponent<SwordDamageTrigger>();
            if (damageTrigger == null) sword.AddComponent<SwordDamageTrigger>();
            
            sword.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleUp(sword.transform));

            _activeSwords[i] = sword;
        }
        _menuActive = true;
    }

    System.Collections.IEnumerator ScaleUp(Transform t)
    {
        float elapsed = 0;
        float duration = 0.5f;
        Vector3 targetScale = new Vector3(0.7f, 0.7f, 0.7f);
        while (elapsed < duration)
        {
            if (t == null) yield break;
            t.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (t != null) t.localScale = targetScale;
    }
}
