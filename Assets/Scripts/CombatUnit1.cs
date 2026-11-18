using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public UnitData data;
    public int currentHealth;
    public bool isPlayerTeam;
    public bool isFrontRow = true;

    private BattleManager battleManager;
    private HealthBar healthBar;

    public void Initialize(BattleManager manager)
    {
        battleManager = manager;
        if (data == null) return;
        currentHealth = data.maxHealth;

        // Lebensbalken automatisch hinzufügen und einrichten
        healthBar = gameObject.AddComponent<HealthBar>();
        healthBar.Setup(data.maxHealth);

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (data.portrait != null) sr.sprite = data.portrait;
            sr.color = data.unitColor;
        }
    }

    void OnMouseDown()
    {
        if (battleManager != null && !IsDead())
        {
            battleManager.OnUnitClicked(this);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (healthBar != null) healthBar.UpdateHealth(currentHealth);

        Debug.Log($"{data.unitName} erleidet {amount} Schaden! Verbleibende HP: {currentHealth}");

        if (IsDead())
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        }
    }

    public bool IsDead() => currentHealth <= 0;
}