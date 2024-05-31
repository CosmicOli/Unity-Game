using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericHorizontalEntityBehaviour : GenericEntityBehaviour
{
    // These constants define an entities drag, acceleration and maximum speed from it's own acceleration in the horizontal axis
    [SerializeField]
    protected float HorizontalDrag;
    [SerializeField]
    protected float HorizontalAccelerationPower;
    [SerializeField]
    protected float MaximumHorizontalSpeedFromPower;

    // This variable contains the direction of the acceleration from movement horizontally
    protected float horizontalAccelerationDirection;
}
