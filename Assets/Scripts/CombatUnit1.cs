using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public UnitData data;
    public int currentHealth;
    public bool isPlayerTeam;
    public bool isFrontRow = true;

    private BattleManager battleManager;
    private HealthBar healthBar;
    private Animator animator;

    public void Initialize(BattleManager manager)
    {
        battleManager = manager;
        animator = GetComponent<Animator>(); // Holt sich deinen visuellen Animator
        if (data == null) return;
        currentHealth = data.maxHealth;

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
            // Spielt die Sterbe-Animation ab und färbt die Figur leicht grau
            if (animator != null) animator.SetTrigger("Death");
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
        else
        {
            // Spielt die Hurt-Animation ab
            if (animator != null) animator.SetTrigger("Hurt");

            // Lässt den Zwerg seinen Schild zur Abwehr nach vorne ziehen!
            var boneAnim = GetComponent<DwarfBoneAnimator>();
            if (boneAnim != null) boneAnim.PlayShieldBlock();
        }

    }

    public bool IsDead() => currentHealth <= 0;
}