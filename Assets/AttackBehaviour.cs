using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackBehaviour : MonoBehaviour
{
    public Vector2 currentDirection;
    private Vector2 lastHorizontalDirection;

    // Start is called before the first frame update
    void Start()
    {
        lastHorizontalDirection = new Vector2 (1, 0); // This should be able to be changed in the inspector
    }

    public Vector2 GetDirection(Vector2 inputDirection)
    {
        if (Mathf.Abs(inputDirection.x) > 0 && inputDirection.y == 0)
        {
            lastHorizontalDirection = inputDirection;
        }

        if (inputDirection != Vector2.zero)
        {
            currentDirection = inputDirection;
        }
        else
        {
            currentDirection = lastHorizontalDirection;
        }

        return currentDirection;
    }
}
