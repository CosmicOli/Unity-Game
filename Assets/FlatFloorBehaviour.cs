using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatFloorBehaviour : GenericFloorBehaviour
{
    public override float JumpableSurfaceEquation(float x)
    {
        // If the floor is horizontally flat, then to be above the floor an entity needs to be above the top of the collider
        return gameObject.transform.position.y + ((BoxCollider2D)Collider2D).offset.y + (((BoxCollider2D)Collider2D).size.y * gameObject.transform.localScale.y / 2);
    }
}
