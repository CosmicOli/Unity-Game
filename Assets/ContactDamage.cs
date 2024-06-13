using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ContactDamageInterface
{
    // These "constants" refer to the player and it's behaviour script
    public GameObject player { get; set; }
    public PlayerBehaviour playerBehaviour { get; set; }

    // This constant defines how much damage the player takes on contact with this entity
    public float ContactDamage { get; set; }

    // This variable defines whether the player is still in contact with this entity
    // This is needed as OnTriggerStay2D only updates on a change within the trigger and not when an object is static within the trigger
    public bool stillInContact { get; set; }

    // This variable stores the most recent collision to reuse it if the player remains in this entity
    public Collider2D currentCollision { get; set; }
}

public static class ContactDamageBehaviour
{
    // This function determines the behaviour needed to be ran every fixed update for
    public static void FixedUpdate(Collider2D currentCollision, bool stillInContact, PlayerBehaviour playerBehaviour, GameObject gameObject, GameObject player, float ContactDamage)
    {
        if (stillInContact)
        {
            OnTriggerEnter2D(currentCollision, currentCollision, stillInContact, playerBehaviour, gameObject, player, ContactDamage);
        }
    }

    // Detecting contact with the player and dealing damage and knockback on collision
    public static Tuple<Collider2D, bool> OnTriggerEnter2D(Collider2D collision, Collider2D currentCollision, bool stillInContact, PlayerBehaviour playerBehaviour, GameObject gameObject, GameObject player, float ContactDamage)
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

            playerBehaviour.TakeKnockback(playerKnockback, true);

            playerBehaviour.TakeDamage(ContactDamage);
        }

        return new Tuple<Collider2D, bool>(currentCollision, stillInContact);
    }

    public static bool OnTriggerExit2D(Collider2D collision, bool stillInContact)
    {
        GameObject hitObject = collision.gameObject;

        if (hitObject.layer == 3)
        {
            stillInContact = false;
        }

        return stillInContact;
    }
}
