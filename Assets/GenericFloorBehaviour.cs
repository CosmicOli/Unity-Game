using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public abstract class GenericFloorBehaviour : GenericEnvironmentBehaviour
{
    public abstract float JumpableSurfaceEquation(float x);

    protected Collider2D Collider2D;

    void Start()
    {
        Collider2D = gameObject.GetComponent<Collider2D>();
    }
}
