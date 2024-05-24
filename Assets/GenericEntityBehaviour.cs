using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    protected virtual void Start()
    {
        // A generic entity has a RigidBody 2D component
        entityRigidBody = gameObject.GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        Vector2 newVelocity = entityRigidBody.velocity;

        // If the entity is trying to slow itself down, slow it down at either the drag or speed, depending on what is higher
        if (Mathf.Sign(entityRigidBody.velocity.x) != Mathf.Sign(horizontalAccelerationDirection) && horizontalAccelerationDirection != 0)
        {
            if (horizontalDrag >= horizontalAccelerationPower)
            {
                TakeDragInAnAxis(ref newVelocity.x, horizontalDrag);
            }
            else
            {
                newVelocity += new Vector2(horizontalAccelerationDirection * horizontalAccelerationPower, 0);
            }
        }
        // If the entity is trying to speed up
        else if (horizontalAccelerationDirection != 0) 
        { 
            // If there is speed to gain in a direction, add it and cap it to the maximum speed.
            if (Mathf.Abs(entityRigidBody.velocity.x) < maximumHorizontalSpeedFromPower)
            {
                newVelocity += new Vector2(horizontalAccelerationDirection * horizontalAccelerationPower, 0);

                // Cap the speed at the limit
                if (Mathf.Abs(newVelocity.x) > maximumHorizontalSpeedFromPower)
                {
                    newVelocity.x = Mathf.Sign(entityRigidBody.velocity.x) * maximumHorizontalSpeedFromPower;
                }
            } 
            // If there is no speed to gain, and specifically there is too much, it slows down with drag.
            else if (Mathf.Abs(entityRigidBody.velocity.x) > maximumHorizontalSpeedFromPower)
            {
                TakeDragInAnAxis(ref newVelocity.x, horizontalDrag);
            }
        }
        // If the entity isn't trying to change speed
        else
        {
            // When there is no attempt at acceleration a drag should be experienced.
            if (Mathf.Abs(horizontalAccelerationDirection) == 0 && Mathf.Abs(entityRigidBody.velocity.x) > 0)
            {
                TakeDragInAnAxis(ref newVelocity.x, horizontalDrag);
            }
        }

        entityRigidBody.velocity = newVelocity;
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
        Vector2 knockback2D = new Vector2(knockback.x, knockback.y); // A direct assignment does this implicitly, however I find an explicit conversion to be more readable.

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
