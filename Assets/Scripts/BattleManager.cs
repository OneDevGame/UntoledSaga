using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Busy, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public BattleState state;
    public List<CombatUnit> allUnits = new List<CombatUnit>();
    private Queue<CombatUnit> turnQueue = new Queue<CombatUnit>();
    private CombatUnit activeUnit;

    void Start()
    {
        state = BattleState.Start;
        foreach (var unit in allUnits)
        {
            unit.Initialize(this);
        }
        SetupTurnOrder();
        NextTurn();
    }

    void SetupTurnOrder()
    {
        var sortedUnits = allUnits
            .Where(u => !u.IsDead())
            .OrderByDescending(u => u.data.initiative)
            .ToList();

        turnQueue = new Queue<CombatUnit>(sortedUnits);
    }

    public void NextTurn()
    {
        if (turnQueue.Count == 0)
        {
            SetupTurnOrder();
        }

        activeUnit = turnQueue.Dequeue();

        if (activeUnit.IsDead())
        {
            NextTurn();
            return;
        }

        if (activeUnit.isPlayerTeam)
        {
            state = BattleState.PlayerTurn;
            Debug.Log($"DEIN ZUG: {activeUnit.data.unitName} ist dran! Klicke auf einen gültigen Gegner.");
        }
        else
        {
            state = BattleState.EnemyTurn;
            StartCoroutine(ExecuteEnemyAI());
        }
    }

    public void OnUnitClicked(CombatUnit clickedUnit)
    {
        if (state != BattleState.PlayerTurn) return;
        if (clickedUnit.isPlayerTeam) return;

        if (CanAttackTarget(activeUnit, clickedUnit))
        {
            StartCoroutine(PerformAttack(activeUnit, clickedUnit));
        }
        else
        {
            Debug.Log($"UNGÜLTIGES ZIEL: {activeUnit.data.unitName} muss zuerst die vordere Reihe besiegen!");
        }
    }

    // Führt den Angriff aus: Ausfallschritt nach vorne + Attack-Trigger + Treffer
    IEnumerator PerformAttack(CombatUnit attacker, CombatUnit target)
    {
        state = BattleState.Busy;

        Vector3 startPos = attacker.transform.position;
        float direction = attacker.isPlayerTeam ? 0.6f : -0.6f; // Held springt nach rechts, Gegner nach links
        Vector3 attackPos = startPos + new Vector3(direction, 0, 0);

        // 1. Kleiner Sprung nach vorne & Angriffs-Animation starten (falls vorhanden)
        attacker.transform.position = attackPos;
        var anim = attacker.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Attack");

        var boneAnim = attacker.GetComponent<DwarfBoneAnimator>();
        if (boneAnim != null) boneAnim.PlayAttackSwing();

        Debug.Log($"{attacker.data.unitName} greift {target.data.unitName} an!");
        yield return new WaitForSeconds(0.35f);

        // 2. Schaden zufügen (löst beim Ziel automatisch Hurt oder Death aus)
        target.TakeDamage(attacker.data.attackDamage);

        yield return new WaitForSeconds(0.35f);

        // 3. Zurück auf die Ausgangsposition springen
        attacker.transform.position = startPos;
        yield return new WaitForSeconds(0.2f);

        CheckBattleEnd();
    }

    bool CanAttackTarget(CombatUnit attacker, CombatUnit target)
    {
        if (attacker.data.canAttackBackRow) return true;
        if (target.isFrontRow) return true;

        bool frontRowAlive = allUnits.Any(u => u.isPlayerTeam == target.isPlayerTeam && u.isFrontRow && !u.IsDead());
        return !frontRowAlive;
    }

    IEnumerator ExecuteEnemyAI()
    {
        yield return new WaitForSeconds(0.5f);

        var validTargets = allUnits
            .Where(u => u.isPlayerTeam && !u.IsDead() && CanAttackTarget(activeUnit, u))
            .ToList();

        if (validTargets.Count > 0)
        {
            CombatUnit target = validTargets[Random.Range(0, validTargets.Count)];
            yield return StartCoroutine(PerformAttack(activeUnit, target));
        }
        else
        {
            CheckBattleEnd();
        }
    }

    public void CheckBattleEnd()
    {
        bool playersAlive = allUnits.Any(u => u.isPlayerTeam && !u.IsDead());
        bool enemiesAlive = allUnits.Any(u => !u.isPlayerTeam && !u.IsDead());

        if (!enemiesAlive)
        {
            state = BattleState.Won;
            Debug.Log("SIEG! Alle Gegner wurden besiegt!");
        }
        else if (!playersAlive)
        {
            state = BattleState.Lost;
            Debug.Log("NIEDERLAGE! Deine Einheiten sind gefallen.");
        }
        else
        {
            NextTurn();
        }
    }
}