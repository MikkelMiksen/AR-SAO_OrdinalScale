using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SwordInteraction : MonoBehaviour
{
    [Header("Sword Data")]
    public SwordData swordData;

    [Header("SAO Settings")]
    [Tooltip("The hand that this sword should be equipped to.")]
    public Transform leftHand;
    public Transform rightHand;

    [Tooltip("Distance to trigger equip.")]
    public float grabDistance = 0.2f;
    public float sheathDistance = 0.2f;

    [Tooltip("If true, pressing Grip once grabs/places, instead of holding it.")]
    public bool useToggleGrab = true;

    [Tooltip("Offset when equipped in hand.")]
    public Vector3 equipPositionOffset = new Vector3(0, 0, 0.05f);
    public Vector3 equipRotationOffset = new Vector3(0, 90, 0);
    
    [Tooltip("Offset when sheathed on shoulder.")]
    public Vector3 sheathPositionOffset = new Vector3(0, 0.3f, 0);
    public Vector3 sheathRotationOffset = new Vector3(0, 0, 180);

    [Header("Physics Settings")]
    public float throwForceMultiplier = 2.0f;
    private Rigidbody _rb;
    private Vector3 _lastPos;
    private Vector3 _velocity;

    private bool _isEquipped = false;
    private bool _isSheathed = false;
    private Transform _equippedHand = null;
    private EquipmentSlot _currentSlot = null;

    private static SwordInteraction _leftHandSword = null;
    private static SwordInteraction _rightHandSword = null;

    public bool IsEquipped => _isEquipped;
    public bool IsSheathed => _isSheathed;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _lastPos = transform.position;
    }

    void Update()
    {
        HandleInput();
        
        if (_isEquipped)
        {
            _velocity = (transform.position - _lastPos) / Time.deltaTime;
            _lastPos = transform.position;
        }
    }

    void HandleInput()
    {
        #if UNITY_EDITOR
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Transform hand = rightHand != null ? rightHand : (Camera.main != null ? Camera.main.transform : null);
                if (hand != null) OnGripPressed(hand, false);
            }
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                Transform hand = leftHand != null ? leftHand : (Camera.main != null ? Camera.main.transform : null);
                if (hand != null) OnGripPressed(hand, true);
            }
            if (Keyboard.current.gKey.wasPressedThisFrame && _isEquipped)
            {
                Drop();
            }
        }
        #endif

        if (InputSystem.devices.Count == 0) return;

        foreach (var device in InputSystem.devices)
        {
            if (device is UnityEngine.InputSystem.XR.XRController controller)
            {
                var gripButton = controller.TryGetChildControl<ButtonControl>("gripPressed") 
                                 ?? controller.TryGetChildControl<ButtonControl>("grip")
                                 ?? controller.TryGetChildControl<ButtonControl>("trigger");

                if (gripButton == null) continue;

                bool isLeft = controller.name.Contains("Left");
                foreach (var usage in controller.usages)
                {
                    if (usage == UnityEngine.InputSystem.CommonUsages.LeftHand) isLeft = true;
                }
                
                Transform hand = isLeft ? leftHand : rightHand;
                if (hand == null) continue;

                if (gripButton.wasPressedThisFrame)
                {
                    OnGripPressed(hand, isLeft);
                }
            }
        }
    }

    void OnGripPressed(Transform hand, bool isLeft)
    {
        if (_isEquipped && _equippedHand == hand)
        {
            // Check if we are at a shoulder to sheath it.
            EquipmentSlot nearSlot = FindNearSlot(hand.position);
            if (nearSlot != null && nearSlot.slottedSword == null)
            {
                Sheath(nearSlot);
                return;
            }
            
            if (useToggleGrab)
            {
                // Toggle release
                Drop();
            }
            return;
        }

        // 1. Try to grab from world/backpack/menu
        if (!_isEquipped && Vector3.Distance(transform.position, hand.position) < grabDistance)
        {
            if (isLeft && _leftHandSword != null) return;
            if (!isLeft && _rightHandSword != null) return;

            Equip(hand, isLeft);
        }
        // 2. Try to grab from shoulder slot
        else if (_isSheathed && Vector3.Distance(hand.position, transform.position) < grabDistance)
        {
            if (isLeft && _leftHandSword != null) return;
            if (!isLeft && _rightHandSword != null) return;
            
            Unsheath(hand, isLeft);
        }
    }

    public void Drop()
    {
        if (!_isEquipped) return;

        Debug.Log($"[SAO] Sword dropped from {_equippedHand.name}");

        _isEquipped = false;
        if (_equippedHand == leftHand) _leftHandSword = null;
        if (_equippedHand == rightHand) _rightHandSword = null;
        _equippedHand = null;

        transform.SetParent(null);
        
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.linearVelocity = _velocity * throwForceMultiplier;
        }
    }

    EquipmentSlot FindNearSlot(Vector3 position)
    {
        if (PlayerInventoryManager.Instance == null) return null;
        
        float distLeft = Vector3.Distance(position, PlayerInventoryManager.Instance.leftShoulder.transform.position);
        if (distLeft < sheathDistance) return PlayerInventoryManager.Instance.leftShoulder;

        float distRight = Vector3.Distance(position, PlayerInventoryManager.Instance.rightShoulder.transform.position);
        if (distRight < sheathDistance) return PlayerInventoryManager.Instance.rightShoulder;

        return null;
    }

    public void Equip(Transform hand, bool isLeft)
    {
        if (_isSheathed) Unsheath(hand, isLeft);

        _isEquipped = true;
        _equippedHand = hand;
        if (isLeft) _leftHandSword = this; else _rightHandSword = this;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        transform.SetParent(hand);
        transform.localPosition = equipPositionOffset;
        transform.localRotation = Quaternion.Euler(equipRotationOffset);
        
        Debug.Log($"[SAO] Sword equipped to {hand.name}!");
    }

    public void Sheath(EquipmentSlot slot)
    {
        _isEquipped = false;
        if (_equippedHand == leftHand) _leftHandSword = null;
        if (_equippedHand == rightHand) _rightHandSword = null;
        _equippedHand = null;

        _isSheathed = true;
        _currentSlot = slot;
        slot.slottedSword = this;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        transform.SetParent(slot.transform);
        transform.localPosition = sheathPositionOffset;
        transform.localRotation = Quaternion.Euler(sheathRotationOffset);

        Debug.Log($"[SAO] Sword sheathed on {slot.slotType}!");
    }

    public void Unsheath(Transform hand, bool isLeft)
    {
        _isSheathed = false;
        if (_currentSlot != null) _currentSlot.slottedSword = null;
        _currentSlot = null;

        Equip(hand, isLeft);
    }
}
