using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxEnemyBehavior : GenericGravityEntityBehaviour, EntityAIInterface
{
    public bool currentlyAttacking { get; set; }

    public bool ableToAttack { get; set; }

    public int aggroLevel { get; set; }

    private GameObject player;

    public int DetermineAggroLevel(GameObject entityAggroedOn)
    {
        return 0;
    }

    public void MoveUnaggroed()
    {
        
    }

    public void MoveAggroed(int aggroLevel, GameObject entityAggroedOn)
    {

    }

    public void Attack(int aggroLevel, GameObject entityAggroedOn)
    {

    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        player = GameObject.FindGameObjectWithTag("Player");
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        EntityAI.ProgressAI(currentlyAttacking, ableToAttack, aggroLevel, player, DetermineAggroLevel, MoveUnaggroed, MoveAggroed, Attack);
    }
}
