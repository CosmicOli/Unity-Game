using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingBehaviour : MonoBehaviour
{
    // These "constants" defining damage, knockback, and swing time are always passed through by SwordBehavior
    [HideInInspector]
    public float damage; 
    [HideInInspector]
    public Vector3 enemyKnockbackStrength; 
    [HideInInspector]
    public Vector3 playerKnockbackStrength;
    [HideInInspector]
    public PlayerBehaviour playerBehaviour;
    [HideInInspector]
    public float swingTime;

    // This constant tracks how much time has passed since the Swing game object's creation
    private float timer = 0;

    // This variable tracks the enemies hit by the Swing object to prevent double hitting the same object
    private List<GameObject> hitEnemies = new List<GameObject>(); 

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

            GenericEntityBehaviour hitObjectBehaviour = hitObject.GetComponent<GenericEntityBehaviour>();

            switch (hitObject.layer) // What type of object was hit
            {
                case 3: // Enemies layer, deal damage
                    hitObjectBehaviour.TakeDamage(damage);
                    goto case 8;

                case 8: // Environmental "enemies" layer, deal knockback
                    switch (hitObject.tag) // What type of enemy was hit
                    {
                        case "Boss": // Bosses currently don't take knockback
                            goto default;

                        case "Environmental Enemy": // Environmental enemies, e.g. a spiky floor, don't take knockback
                            goto default;

                        case "Enemy": // Enemies take knockback based on the knockback applied
                            hitObjectBehaviour.TakeKnockback(enemyKnockbackStrength);
                            goto default;

                        default:
                            playerBehaviour.TakeKnockback(playerKnockbackStrength);
                            break;
                    }

                    break;

                case 7:
                    //Debug.Log("hit wall");
                    break;

                default:
                    //Debug.Log("hit other");
                    break;
            }
        }
    }
}
