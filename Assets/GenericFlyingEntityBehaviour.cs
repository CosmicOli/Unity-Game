using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFlyingEntityBehaviour : GenericHorizontalEntityBehaviour
{
    // These constants define an entities drag, acceleration and maximum speed from it's own acceleration in the vertical axis
    [SerializeField]
    private float verticalDrag;
    [SerializeField]
    private float maximumVerticalSpeedFromPower;
    [SerializeField]
    private float verticalAccelerationPower;

    // This variable contains the direction of the acceleration from movement vertically
    private float verticalAccelerationDirection;

    protected virtual void FixedUpdate()
    {
        float newHorizontalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.x, HorizontalDrag, HorizontalAccelerationPower, horizontalAccelerationDirection, MaximumHorizontalSpeedFromPower);
        float newVerticalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.y, verticalDrag, verticalAccelerationPower, verticalAccelerationDirection, maximumVerticalSpeedFromPower);

        entityRigidBody.velocity = new Vector2(newHorizontalVelocity, newVerticalVelocity);
    }
}
