using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingBehaviour : MonoBehaviour
{
    public float damage; // This is always passed through by sword behavior
    public Vector3 knockback; // This is always passed through by sword behavior
    public float swingTime;
    private float timer = 0;
    private List<GameObject> hitEnemies = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.5)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject hitObject = collision.gameObject;

        if (!hitEnemies.Contains(hitObject))
        {
            hitEnemies.Add(hitObject);

            GenericEntityBehaviour hitObjectScript = hitObject.GetComponent<GenericEntityBehaviour>();

            switch (hitObject.layer) // What type of object was hit
            {
                case 3: // Enemies layer, deal damage
                    hitObjectScript.TakeDamage(damage);
                    goto case 8;

                case 8: // Environmental "enemies" layer, deal knockback
                    switch (hitObject.tag) // What type of enemy was hit
                    {
                        case "Boss": // Bosses currently don't take knockback
                            goto default;

                        case "Environmental Enemy": // Environmental enemies, e.g. a spiky floor, don't take knockback
                            goto default;

                        case "Enemy": // Enemies take knockback based on the knockback applied
                            hitObjectScript.TakeKnockback(knockback);
                            goto default;

                        default:
                            
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
