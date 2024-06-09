using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericGravityEntityBehaviour : GenericHorizontalEntityBehaviour
{
    // These constants define jumping power and the terminal falling speed
    [SerializeField]
    protected float JumpAccelerationPower;
    public float TerminalSpeed;

    // This "constant" contains a record of the gravity scale experienced by the entity
    protected float gravityScale;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        gravityScale = entityRigidBody.gravityScale;
    }

    protected virtual void FixedUpdate()
    {
        float newHorizontalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.x, HorizontalDrag, HorizontalAccelerationPower, horizontalAccelerationDirection, MaximumHorizontalSpeedFromPower);
        float newVerticalVelocity = calculateGravityAxisVelocity(entityRigidBody.velocity.y, TerminalSpeed);

        entityRigidBody.velocity = new Vector2(newHorizontalVelocity, newVerticalVelocity);
    }

    private float calculateGravityAxisVelocity(float axisVelocity, float axisTerminalSpeed)
    {
        // Gravity is handled by the Rigid Body 2D assocciated with the gameObject

        float newAxisVelocity = axisVelocity;

        // If faster than the maximum speed then set to the maximum speed.
        // It is assumed that gravity acts downwards.
        if (Mathf.Abs(newAxisVelocity) >= Mathf.Abs(TerminalSpeed) && Mathf.Sign(-1 * newAxisVelocity) > 0)
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
