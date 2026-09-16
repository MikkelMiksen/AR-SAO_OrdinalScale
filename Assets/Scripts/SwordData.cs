using UnityEngine;

[CreateAssetMenu(fileName = "NewSwordData", menuName = "SAO/Sword Data")]
public class SwordData : ScriptableObject
{
    public string swordName;
    public GameObject prefab;
    public float damage = 25f;
    public float staminaCost = 20f;
    
    [Header("Visuals")]
    public Sprite icon;
    public Color themeColor = Color.cyan;
}
