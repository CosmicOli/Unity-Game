using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericEntityBehaviour : MonoBehaviour
{
    public float health;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    public void TakeKnockback(Vector3 knockback)
    {
        // A generic enemy has a RigidBody 2D component
        Rigidbody2D enemyRigidBody = gameObject.GetComponent<Rigidbody2D>();

        Vector2 knockback2D = new Vector2(knockback.x, knockback.y); // A direct assignment does this implicitly, however I find explicit typing more readable.

        Vector2 newVelocity = new Vector2(); // If an enemy is travelling faster than the knockback speed in a particular direction, keep that speed. Otherwise set it the knockback speed.

        if (Mathf.Abs(enemyRigidBody.velocity.y) <= Mathf.Abs(knockback2D.y))
        {
            newVelocity.y = knockback2D.y;
        }
        
        if (Mathf.Abs(enemyRigidBody.velocity.x) <= Mathf.Abs(knockback2D.x))
        {
            newVelocity.x = knockback2D.x;
        }

        enemyRigidBody.velocity = newVelocity;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

}
