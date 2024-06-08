using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public interface EntityAIInterface
{
    // This variable is used to check whether the entity is currently in an attack
    public bool currentlyAttacking { get; set;}

    // This variable is used to check whether the entity is currently able to attack
    public bool ableToAttack { get; set; }

    // This variable is used to check whether the entity has been aggroed
    // Level 0 is unaggroed, and higher values refer to different states the entity is in
    public int aggroLevel { get; set; }

    // Determines what the current aggro level is based on information on other entities
    public int DetermineAggroLevel(GameObject entityAggroedOn);

    // Determines how the entity will move when not aggroed
    public void MoveUnaggroed();

    // Determines how the entity will move when aggroed
    public void MoveAggroed(int aggroLevel, GameObject entityAggroedOn);

    // Determines how the entity will attack
    public void Attack(int aggroLevel, GameObject entityAggroedOn);
}

public static class EntityAIBehaviour
{
    public static void ProgressAI(bool currentlyAttacking, bool ableToAttack, int aggroLevel, GameObject entityAggroedOn, Func<GameObject, int> DetermineAggroLevel, Action MoveUnaggroed, Action<int, GameObject> MoveAggroed, Action<int, GameObject> Attack)
    {
        // If currently not doing an attack
        if (!currentlyAttacking)
        {
            // Find the aggro level
            aggroLevel = DetermineAggroLevel(entityAggroedOn);

            // If not aggroed
            if (aggroLevel == 0)
            {
                MoveUnaggroed();
            }
            // If aggroed
            else
            {
                // If able to attack
                if (ableToAttack)
                {
                    Attack(aggroLevel, entityAggroedOn);
                }
                else
                {
                    MoveAggroed(aggroLevel, entityAggroedOn);
                }
            }
        }
    }
}