using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericGravityEntityBehaviour : GenericHorizontalHorizontalEntityBehaviour
{
    public float jumpAccelerationPower;
    public float terminalSpeed;
    protected float gravityScale;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        gravityScale = entityRigidBody.gravityScale;
    }

    protected virtual void FixedUpdate()
    {
        float newHorizontalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.x, horizontalDrag, horizontalAccelerationPower, horizontalAccelerationDirection, maximumHorizontalSpeedFromPower);
        float newVerticalVelocity = calculateGravityAxisVelocity(entityRigidBody.velocity.y, terminalSpeed);

        entityRigidBody.velocity = new Vector2(newHorizontalVelocity, newVerticalVelocity);
    }

    private float calculateGravityAxisVelocity(float axisVelocity, float axisTerminalSpeed)
    {
        // Gravity is handled by the Rigid Body 2D assocciated with the gameObject

        float newAxisVelocity = axisVelocity;

        // If faster than the maximum speed then set to the maximum speed.
        // It is assumed that gravity acts downwards.
        if (Mathf.Abs(newAxisVelocity) >= Mathf.Abs(terminalSpeed) && Mathf.Sign(-1 * newAxisVelocity) > 0)
        {
            newAxisVelocity = -1 * axisTerminalSpeed;
            entityRigidBody.gravityScale = 0;
        }
        else
        {
            entityRigidBody.gravityScale = gravityScale;
        }

        return newAxisVelocity;
    }
}
