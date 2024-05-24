using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericGravityEntityBehaviour : GenericEntityBehaviour
{
    protected virtual void FixedUpdate()
    {
        float newHorizontalVelocity = calculateGravitylessAxisVelocity(entityRigidBody.velocity.x, horizontalDrag, horizontalAccelerationPower, horizontalAccelerationDirection, maximumHorizontalSpeedFromPower);
        float newVerticalVelocity = entityRigidBody.velocity.y;

        entityRigidBody.velocity = new Vector2(calculateGravitylessAxisVelocity(entityRigidBody.velocity.x, horizontalDrag, horizontalAccelerationPower, horizontalAccelerationDirection, maximumHorizontalSpeedFromPower), entityRigidBody.velocity.y);
    }
}
