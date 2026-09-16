using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages AR room scanning and provides access to walls, ceilings, and floors.
/// Uses Meta XR Scene SDK with fallback to an Editor Mock Room for development.
/// </summary>
public class ARSceneManager : MonoBehaviour
{
    [Header("Meta Scene SDK")]
    public OVRSceneManager ovrSceneManager;

    [Header("Settings")]
    public bool scanOnStart = true;
    public bool showVisualDebugger = true;
    public bool enableEditorMockRoom = true;
    public Vector3 mockRoomDimensions = new Vector3(5f, 2.6f, 5f);
    public Material wallDebugMaterial;
    public Material ceilingDebugMaterial;
    public Material floorDebugMaterial;
    public PhysicsMaterial floorPhysicsMaterial;

    private List<OVRScenePlane> _walls = new List<OVRScenePlane>();
    private List<OVRScenePlane> _ceilings = new List<OVRScenePlane>();
    private List<OVRScenePlane> _floors = new List<OVRScenePlane>();
    private List<OVRSceneAnchor> _furniture = new List<OVRSceneAnchor>();

    private List<Transform> _mockWalls = new List<Transform>();
    private List<Transform> _mockCeilings = new List<Transform>();
    private List<Transform> _mockFloors = new List<Transform>();
    private GameObject _mockRoomRoot;

    public List<OVRScenePlane> Walls => _walls;
    public List<OVRScenePlane> Ceilings => _ceilings;
    public List<OVRScenePlane> Floors => _floors;
    public List<OVRSceneAnchor> Furniture => _furniture;

    public System.Action OnSceneLoaded;

    private void Awake()
    {
        if (ovrSceneManager == null)
            ovrSceneManager = GetComponent<OVRSceneManager>();

        if (ovrSceneManager != null)
        {
            ovrSceneManager.SceneModelLoadedSuccessfully += HandleSceneLoaded;
        }
    }

    private void Start()
    {
        if (scanOnStart)
        {
            StartRoomScan();
        }
    }

    public void StartRoomScan()
    {
        Debug.Log("[SAO] Starting Room Scan...");

        #if UNITY_EDITOR
        if (enableEditorMockRoom)
        {
            StartCoroutine(SimulateEditorScanRoutine());
            return;
        }
        #endif

        if (ovrSceneManager != null)
        {
            ovrSceneManager.LoadSceneModel();
        }
        else
        {
            StartCoroutine(SimulateEditorScanRoutine());
        }
    }

    private IEnumerator SimulateEditorScanRoutine()
    {
        Debug.Log("[SAO] Generating Editor Mock Room...");
        yield return new WaitForSeconds(1.0f);

        CreateMockRoom();
        HandleSceneLoaded();
    }

    private void CreateMockRoom()
    {
        if (_mockRoomRoot != null) Destroy(_mockRoomRoot);
        _mockRoomRoot = new GameObject("MockRoom");

        _mockWalls.Clear();
        _mockCeilings.Clear();
        _mockFloors.Clear();

        Vector3 center = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        center.y = 0; // floor level at y = 0

        float w = mockRoomDimensions.x;
        float h = mockRoomDimensions.y;
        float d = mockRoomDimensions.z;

        // Floor
        GameObject floor = CreateMockPlane("MockFloor", center + new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), new Vector2(w, d), floorDebugMaterial);
        floor.transform.SetParent(_mockRoomRoot.transform);
        _mockFloors.Add(floor.transform);

        // Ceiling
        GameObject ceiling = CreateMockPlane("MockCeiling", center + new Vector3(0, h, 0), Quaternion.Euler(-90, 0, 0), new Vector2(w, d), ceilingDebugMaterial);
        ceiling.transform.SetParent(_mockRoomRoot.transform);
        _mockCeilings.Add(ceiling.transform);

        // North Wall
        GameObject wallN = CreateMockPlane("MockWall_North", center + new Vector3(0, h * 0.5f, d * 0.5f), Quaternion.Euler(0, 180, 0), new Vector2(w, h), wallDebugMaterial);
        wallN.transform.SetParent(_mockRoomRoot.transform);
        _mockWalls.Add(wallN.transform);

        // South Wall
        GameObject wallS = CreateMockPlane("MockWall_South", center + new Vector3(0, h * 0.5f, -d * 0.5f), Quaternion.Euler(0, 0, 0), new Vector2(w, h), wallDebugMaterial);
        wallS.transform.SetParent(_mockRoomRoot.transform);
        _mockWalls.Add(wallS.transform);

        // East Wall
        GameObject wallE = CreateMockPlane("MockWall_East", center + new Vector3(w * 0.5f, h * 0.5f, 0), Quaternion.Euler(0, -90, 0), new Vector2(d, h), wallDebugMaterial);
        wallE.transform.SetParent(_mockRoomRoot.transform);
        _mockWalls.Add(wallE.transform);

        // West Wall
        GameObject wallW = CreateMockPlane("MockWall_West", center + new Vector3(-w * 0.5f, h * 0.5f, 0), Quaternion.Euler(0, 90, 0), new Vector2(d, h), wallDebugMaterial);
        wallW.transform.SetParent(_mockRoomRoot.transform);
        _mockWalls.Add(wallW.transform);

        Debug.Log($"[SAO] Mock Room Created with 4 Walls, 1 Floor, 1 Ceiling. Size: {w}x{d}x{h}");
    }

    private GameObject CreateMockPlane(string planeName, Vector3 position, Quaternion rotation, Vector2 size, Material mat)
    {
        GameObject planeObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        planeObj.name = planeName;
        planeObj.transform.position = position;
        planeObj.transform.rotation = rotation;
        planeObj.transform.localScale = new Vector3(size.x, size.y, 1f);

        var mc = planeObj.GetComponent<MeshCollider>();
        if (mc != null) Destroy(mc);

        var box = planeObj.AddComponent<BoxCollider>();
        box.size = new Vector3(1f, 1f, 0.05f);
        box.isTrigger = true;

        var renderer = planeObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            if (mat != null)
            {
                renderer.material = mat;
            }
            else
            {
                renderer.enabled = showVisualDebugger;
            }
        }

        return planeObj;
    }

    private void HandleSceneLoaded()
    {
        Debug.Log("[SAO] Scene Model / Mock Room Loaded Successfully.");
        CategorizeSceneAnchors();
        OnSceneLoaded?.Invoke();
    }

    private void CategorizeSceneAnchors()
    {
        _walls.Clear();
        _ceilings.Clear();
        _floors.Clear();
        _furniture.Clear();

        OVRSceneAnchor[] anchors = FindObjectsOfType<OVRSceneAnchor>();
        foreach (var anchor in anchors)
        {
            var plane = anchor.GetComponent<OVRScenePlane>();
            var volume = anchor.GetComponent<OVRSceneVolume>();
            var classification = anchor.GetComponent<OVRSemanticClassification>();

            if (classification != null)
            {
                if (classification.Contains(OVRSceneManager.Classification.WallFace))
                {
                    if (plane != null)
                    {
                        _walls.Add(plane);
                        if (showVisualDebugger) CreateDebugPlane(plane, wallDebugMaterial);
                    }
                }
                else if (classification.Contains(OVRSceneManager.Classification.Ceiling))
                {
                    if (plane != null)
                    {
                        _ceilings.Add(plane);
                        if (showVisualDebugger) CreateDebugPlane(plane, ceilingDebugMaterial);
                    }
                }
                else if (classification.Contains(OVRSceneManager.Classification.Floor))
                {
                    if (plane != null)
                    {
                        _floors.Add(plane);
                        if (floorPhysicsMaterial != null)
                        {
                            var collider = plane.GetComponent<MeshCollider>();
                            if (collider != null) collider.material = floorPhysicsMaterial;
                        }
                        if (showVisualDebugger) CreateDebugPlane(plane, floorDebugMaterial);
                    }
                }
                else
                {
                    // Everything else (Tables, Desks, Couches, etc.)
                    _furniture.Add(anchor);
                    Debug.Log($"[SAO] Furniture detected: {anchor.name} classified as {string.Join(", ", classification.Labels)}");
                    
                    // Add collider if missing to ensure they aren't ignored by physics/raycasts
                    if (anchor.GetComponent<Collider>() == null)
                    {
                        if (volume != null) anchor.gameObject.AddComponent<BoxCollider>();
                        else if (plane != null) anchor.gameObject.AddComponent<BoxCollider>().size = new Vector3(plane.Dimensions.x, plane.Dimensions.y, 0.01f);
                    }
                }
            }
        }

        Debug.Log($"[SAO] Categorized: {_walls.Count} Walls, {_ceilings.Count} Ceilings, {_floors.Count} Floors, {_furniture.Count} Furniture items.");
    }

    private void CreateDebugPlane(OVRScenePlane plane, Material material)
    {
        GameObject debugObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        debugObj.name = "DebugPlane_" + plane.gameObject.name;
        debugObj.transform.SetParent(plane.transform, false);
        debugObj.transform.localScale = new Vector3(plane.Dimensions.x, plane.Dimensions.y, 1f);
        
        var renderer = debugObj.GetComponent<MeshRenderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }

        var collider = debugObj.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    public Vector3 GetRandomPointOnWall()
    {
        if (_walls.Count > 0)
        {
            var wall = _walls[Random.Range(0, _walls.Count)];
            return GetRandomPointOnPlane(wall);
        }

        if (_mockWalls.Count > 0)
        {
            var mockWall = _mockWalls[Random.Range(0, _mockWalls.Count)];
            Vector3 localScale = mockWall.localScale;
            float rx = Random.Range(-localScale.x * 0.4f, localScale.x * 0.4f);
            float ry = Random.Range(-localScale.y * 0.4f, localScale.y * 0.4f);
            return mockWall.TransformPoint(new Vector3(rx, ry, 0));
        }

        Vector3 fallback = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        Vector2 circle = Random.insideUnitCircle.normalized * 2.5f;
        return fallback + new Vector3(circle.x, Random.Range(0.5f, 1.8f), circle.y);
    }

    public Vector3 GetRandomPointOnCeiling()
    {
        if (_ceilings.Count > 0)
        {
            var ceiling = _ceilings[Random.Range(0, _ceilings.Count)];
            return GetRandomPointOnPlane(ceiling);
        }

        if (_mockCeilings.Count > 0)
        {
            var mockCeiling = _mockCeilings[Random.Range(0, _mockCeilings.Count)];
            Vector3 localScale = mockCeiling.localScale;
            float rx = Random.Range(-localScale.x * 0.4f, localScale.x * 0.4f);
            float ry = Random.Range(-localScale.y * 0.4f, localScale.y * 0.4f);
            return mockCeiling.TransformPoint(new Vector3(rx, ry, 0));
        }

        Vector3 fallback = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        Vector2 circle = Random.insideUnitCircle * 2.0f;
        return fallback + new Vector3(circle.x, 2.4f, circle.y);
    }

    private Vector3 GetRandomPointOnPlane(OVRScenePlane plane)
    {
        Vector2 size = plane.Dimensions;
        float randomX = Random.Range(-size.x / 2f, size.x / 2f);
        float randomY = Random.Range(-size.y / 2f, size.y / 2f);
        return plane.transform.TransformPoint(new Vector3(randomX, randomY, 0));
    }

    public void UpdateFloorAlignment(Transform groundTransform)
    {
        if (_floors.Count > 0)
        {
            float floorY = _floors[0].transform.position.y;
            Vector3 pos = groundTransform.position;
            pos.y = floorY;
            groundTransform.position = pos;
        }
        else if (_mockFloors.Count > 0)
        {
            float floorY = _mockFloors[0].position.y;
            Vector3 pos = groundTransform.position;
            pos.y = floorY;
            groundTransform.position = pos;
        }
    }
}
