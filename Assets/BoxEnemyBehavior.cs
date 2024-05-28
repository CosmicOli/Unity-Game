using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxEnemyBehavior : GenericGravityEntityBehaviour, EntityAIInterface
{
    // These "constants" refer to the player and it's behaviour script
    private GameObject player;
    private PlayerBehaviour playerBehaviour;

    // This constant defines how much damage the player takes on contact with this entity
    [SerializeField]
    private float ContactDamage;

    // This variable defines whether the player is still in contact with this entity
    // This is needed as OnTriggerStay2D only updates on a change within the trigger and not when an object is static within the trigger
    private bool stillInContact;

    // This variable stores the most recent collision to reuse it if the player remains in this entity
    private Collider2D currentCollision;

    public bool currentlyAttacking { get; set; }

    public bool ableToAttack { get; set; }

    public int aggroLevel { get; set; }

    public int DetermineAggroLevel(GameObject entityAggroedOn)
    {
        return 0;
    }

    public void MoveUnaggroed()
    {
        //horizontalAccelerationDirection = -1;
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
        playerBehaviour = player.GetComponent<PlayerBehaviour>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (stillInContact)
        {
            OnTriggerEnter2D(currentCollision);
        }

        EntityAI.ProgressAI(currentlyAttacking, ableToAttack, aggroLevel, player, DetermineAggroLevel, MoveUnaggroed, MoveAggroed, Attack);
    }

    // Detecting contact with the player and dealing damage and knockback on collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject hitObject = collision.gameObject;

        if (hitObject.layer == 3)
        {
            stillInContact = true;
            currentCollision = collision;

            Vector3 playerKnockback = playerBehaviour.StandardContactKnockback;

            // If to the left of the player, hit them right instead of left
            if (gameObject.transform.position.x < player.transform.position.x)
            {
                playerKnockback.x *= -1;
            }

            playerBehaviour.TakeKnockback(playerKnockback);

            playerBehaviour.TakeDamage(ContactDamage);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject hitObject = collision.gameObject;

        if (hitObject.layer == 3)
        {
            stillInContact = false;
        }
    }
}

