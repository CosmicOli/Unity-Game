using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingBehaviour : MonoBehaviour
{
    // These "constants" defining damage, knockback, swing time, and referencing the player behaviour script, are always passed through by SwordBehavior
    private float damage; 
    private Vector3 enemyKnockbackStrength; 
    private Vector3 playerKnockbackStrength;
    private float swingTime;
    private PlayerBehaviour playerBehaviour;

    // This constant tracks how much time has passed since the Swing game object's creation
    private float timer = 0;

    // This variable tracks the enemies hit by the Swing object to prevent double hitting the same object
    private List<GameObject> hitEnemies = new List<GameObject>(); 

    public void AssignConstants(float damage, Vector3 enemyKnockbackStrength, Vector3 playerKnockbackStrength, float swingTime, PlayerBehaviour playerBehaviour)
    {
        this.damage = damage;
        this.enemyKnockbackStrength = enemyKnockbackStrength;
        this.playerKnockbackStrength = playerKnockbackStrength;
        this.swingTime = swingTime;
        this.playerBehaviour = playerBehaviour;
    }

    // Update is called once per frame
    void Update()
    {
        // This tracks the time since the swing started, and finishes the swing if the correct time has ellapsed
        timer += Time.deltaTime;
        if (timer >= swingTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject hitObject = collision.gameObject;

        // If the object hasn't already been hit
        if (!hitEnemies.Contains(hitObject))
        {
            // Mark the object as having been hit
            hitEnemies.Add(hitObject);

            // This will be null hitting a wall, however isn't used when hitting a non-entity
            GenericEntityBehaviour hitObjectBehaviour = hitObject.GetComponent<GenericEntityBehaviour>();

            switch (hitObject.layer) // What type of object was hit
            {
                case 9: // Enemies layer, deal damage
                    hitObjectBehaviour.TakeDamage(damage);
                    goto case 8;

                case 8: // Environmental "enemies" layer (and rolled over regular enemies), deal knockback to player
                    switch (hitObject.tag) // What type of enemy was hit
                    {
                        case "Boss": // Bosses currently don't take knockback
                            goto default;

                        case "Environmental Enemy": // Environmental enemies, e.g. a spiky floor, don't take knockback
                            goto default;

                        case "Enemy": // Enemies take knockback based on the knockback applied
                            hitObjectBehaviour.TakeKnockback(enemyKnockbackStrength);
                            goto default;

                        default: // Default is used instead of being seperate to the switch statement to catch any erronious untagged enemies
                            playerBehaviour.TakeKnockback(playerKnockbackStrength);
                            break;
                    }

                    break;

                case 6:
                    // If the player is on top of the hit object, don't apply knockback
                    if (!playerBehaviour.CurrentlyOnTopOfWallOrFloor(hitObject))
                    {
                        // If the player is not on top of the object but still hits it downwards, don't pogo
                        if (!(playerKnockbackStrength.y > 0 && playerKnockbackStrength.x == 0))
                        {
                            // Vertical knockback is halved to not be too strong
                            playerBehaviour.TakeKnockback(new Vector2(playerKnockbackStrength.x, playerKnockbackStrength.y / 2));
                        }
                    }

                    break;

                default:
                    //Debug.Log("hit other");
                    break;
            }
        }
    }
}
