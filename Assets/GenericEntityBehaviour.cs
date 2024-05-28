using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class GenericEntityBehaviour : MonoBehaviour
{
    // This variable contains the current health of the entity
    [SerializeField]
    private float health;

    // These variables represent a multiplier to knockback based on an entities resistance to it
    [SerializeField]
    private float horizontalKnockbackModifier;
    [SerializeField]
    private float verticalKnockbackModifier;

    // This "constant" refers to the entity's rigid body
    // This is passed in automatically on start as every entity needs a rigid body
    protected Rigidbody2D entityRigidBody;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // A generic entity has a RigidBody 2D component
        entityRigidBody = gameObject.GetComponent<Rigidbody2D>();

        // A generic entity shouldn't rotate
        entityRigidBody.freezeRotation = true;
    }

    protected float calculateGravitylessAxisVelocity(float axisVelocity, float axisDrag, float axisAccelerationPower, float axisAccelerationDirection, float maximumAxisSpeedFromPower)
    {
        float newAxisVelocity = axisVelocity;

        // If the entity is trying to slow itself down, slow it down at either the drag or speed, depending on what is higher
        if (Mathf.Sign(axisVelocity) != Mathf.Sign(axisAccelerationDirection) && axisAccelerationDirection != 0)
        {
            // If the drag is less than the acceleration then accelerate with only acceleration
            if (axisDrag <= axisAccelerationPower)
            {
                newAxisVelocity += axisAccelerationDirection * axisAccelerationPower;
            }
            // If the drag is greater than the acceleration and the drag won't remove more than the current speed, take drag
            else if (axisDrag > axisAccelerationPower && axisDrag > newAxisVelocity)
            {
                newAxisVelocity = TakeDragInAnAxis(newAxisVelocity, axisDrag);
            }
            // If the drag is greater than the acceleration and the drag will remove more than the current speed, take a proportion of drag and acceleration
            else
            {
                newAxisVelocity = (1 - newAxisVelocity / axisDrag) * axisAccelerationDirection * axisAccelerationPower;
            }
        }
        // If the entity is trying to speed up
        else if (axisAccelerationDirection != 0)
        {
            // If there is speed to gain in a direction, add it and cap it to the maximum speed.
            if (Mathf.Abs(axisVelocity) < maximumAxisSpeedFromPower)
            {
                newAxisVelocity += axisAccelerationDirection * axisAccelerationPower;

                // Cap the speed at the limit
                if (Mathf.Abs(newAxisVelocity) > maximumAxisSpeedFromPower)
                {
                    newAxisVelocity = Mathf.Sign(axisVelocity) * maximumAxisSpeedFromPower;
                }
            }
            // If there is no speed to gain, and specifically there is too much, it slows down with drag.
            else if (Mathf.Abs(axisVelocity) > maximumAxisSpeedFromPower)
            {
                newAxisVelocity = TakeDragInAnAxis(newAxisVelocity, axisDrag);
            }
        }
        // If the entity isn't trying to change speed
        else
        {
            // When there is no attempt at acceleration a drag should be experienced.
            if (Mathf.Abs(axisAccelerationDirection) == 0 && Mathf.Abs(axisVelocity) > 0)
            {
                newAxisVelocity = TakeDragInAnAxis(newAxisVelocity, axisDrag);
            }
        }

        return newAxisVelocity;
    }

    public float TakeDragInAnAxis(float currentAxisVelocity, float axisDrag)
    {
        float currentAxisDirection = Mathf.Sign(currentAxisVelocity);

        // Drag is taken opposite to the direction of travel
        currentAxisVelocity -= currentAxisDirection * axisDrag;

        // If after taking drag the velocity is now in the opposite direction, set the velocity to 0
        if (currentAxisDirection != Mathf.Sign(currentAxisVelocity))
        {
            currentAxisVelocity = 0;
        }

        return currentAxisVelocity;
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void TakeKnockback(Vector3 knockback)
    {
        Vector2 newVelocity = entityRigidBody.velocity;

        // If the knockback in an axis isn't 0, set the velocity to the knockback

        if (Mathf.Abs(knockback.x) > 0)
        {
            newVelocity.x = horizontalKnockbackModifier * knockback.x;
        }

        if (Mathf.Abs(knockback.y) > 0)
        {
            newVelocity.y = verticalKnockbackModifier * knockback.y;
        }

        entityRigidBody.velocity = newVelocity;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

}
