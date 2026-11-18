using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Tactics/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Color unitColor = Color.white;
    public Sprite portrait;
    public int maxHealth = 100;
    public int attackDamage = 25;
    public int initiative = 10;
    public bool canAttackBackRow = false; // True für Bogenschützen & Magier
}