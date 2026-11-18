using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }

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
            ExecuteEnemyAI();
        }
    }

    public void OnUnitClicked(CombatUnit clickedUnit)
    {
        if (state != BattleState.PlayerTurn) return;
        if (clickedUnit.isPlayerTeam) return; // Eigene Einheiten nicht angreifen

        if (CanAttackTarget(activeUnit, clickedUnit))
        {
            Debug.Log($"{activeUnit.data.unitName} greift {clickedUnit.data.unitName} an!");
            clickedUnit.TakeDamage(activeUnit.data.attackDamage);
            CheckBattleEnd();
        }
        else
        {
            Debug.Log($"UNGÜLTIGES ZIEL: {activeUnit.data.unitName} muss zuerst die vordere Reihe besiegen!");
        }
    }

    bool CanAttackTarget(CombatUnit attacker, CombatUnit target)
    {
        if (attacker.data.canAttackBackRow) return true; // Fernkämpfer dürfen immer angreifen
        if (target.isFrontRow) return true; // Vordere Reihe ist immer angreifbar

        // Prüfen, ob im Team des Ziels noch jemand in der vorderen Reihe lebt
        bool frontRowAlive = allUnits.Any(u => u.isPlayerTeam == target.isPlayerTeam && u.isFrontRow && !u.IsDead());
        return !frontRowAlive;
    }

    void ExecuteEnemyAI()
    {
        var validTargets = allUnits
            .Where(u => u.isPlayerTeam && !u.IsDead() && CanAttackTarget(activeUnit, u))
            .ToList();

        if (validTargets.Count > 0)
        {
            CombatUnit target = validTargets[Random.Range(0, validTargets.Count)];
            Debug.Log($"GEGNER: {activeUnit.data.unitName} greift {target.data.unitName} an!");
            target.TakeDamage(activeUnit.data.attackDamage);
        }
        CheckBattleEnd();
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