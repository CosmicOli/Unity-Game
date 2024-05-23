using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class GenericEntityBehaviour : MonoBehaviour
{
    public float health;

    public float horizontalDrag;

    public float horizontalAccelerationPower;
    public float verticalAccelerationPower;

    public float maximumHorizontalSpeedFromPower;
    public float maximumVerticalVelocityFromPower;

    protected float horizontalAccelerationDirection;
    protected Rigidbody2D entityRigidBody;

    // Start is called before the first frame update
    void Start()
    {
        // A generic entity has a RigidBody 2D component
        entityRigidBody = gameObject.GetComponent<Rigidbody2D>();
    }

    public void TakeDragInAnAxis(ref float currentAxisVelocity, float axisDrag)
    {
        currentAxisVelocity -= Mathf.Sign(currentAxisVelocity) * axisDrag;
    }

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
        Vector2 knockback2D = new Vector2(knockback.x, knockback.y); // A direct assignment does this implicitly, however I find explicit typing more readable.

        Vector2 newVelocity = entityRigidBody.velocity;

        if (Mathf.Abs(knockback2D.x) > 0)
        {
            newVelocity.x = knockback2D.x;
        }

        if (Mathf.Abs(knockback2D.y) > 0)
        {
            newVelocity.y = knockback2D.y;
        }

        entityRigidBody.velocity = newVelocity;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

}
