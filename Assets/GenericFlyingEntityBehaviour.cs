using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFlyingEntityBehaviour : GenericEntityBehaviour
{
    protected virtual void FixedUpdate()
    {
        float newHorizontalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.x, horizontalDrag, horizontalAccelerationPower, horizontalAccelerationDirection, maximumHorizontalSpeedFromPower);
        float newVerticalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.y, verticalDrag, verticalAccelerationPower, verticalAccelerationDirection, maximumVerticalSpeedFromPower);

        entityRigidBody.velocity = new Vector2(newHorizontalVelocity, newVerticalVelocity);
    }
}
