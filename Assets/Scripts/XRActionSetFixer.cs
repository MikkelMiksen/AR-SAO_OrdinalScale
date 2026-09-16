using UnityEngine;
using UnityEngine.InputSystem;

public class XRActionSetFixer : MonoBehaviour
{
    public InputActionAsset inputActions;

    void Start()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            Debug.Log("[SAO] OpenXR ActionSet manually enabled.");
        }
        else
        {
            Debug.LogWarning("[SAO] No Input Action Asset assigned to Fixer.");
        }
    }

    // Force attachment if OpenXR is active
    void OnEnable()
    {
        if (inputActions != null) inputActions.Enable();
    }
}
