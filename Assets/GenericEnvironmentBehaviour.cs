using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericEnvironmentBehaviour : MonoBehaviour
{
    // This determines whether this object is a boundary for the camera's horizontal movement
    public bool HorizontalCameraBounding;

    // If this object bounds the camera horizontally, how far offset is the bound from the centre of this object
    public float RelativeHorizontalCameraBounding;

    // This determines whether this object is a boundary for the camera's vertical movement
    public bool VerticalCameraBounding;

    // If this object bounds the camera horizontally, how far offset is the bound from the centre of this object
    public float RelativeVerticalCameraBounding;
}
