using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackBehaviour : MonoBehaviour
{
    // This constant stores the horizontal direction the character is facing when spawned in
    [SerializeField]
    private float StartDirection;

    // This variable stores where the player is currently pointing
    [HideInInspector]
    public Vector2 currentDirection;

    // This variable stores where the player is currently pointing
    private Vector2 lastHorizontalDirection;

    // Start is called before the first frame update
    void Start()
    {
        lastHorizontalDirection = new Vector2(StartDirection, 0);
    }

    public Vector2 GetDirection(Vector2 inputDirection)
    {
        // If a horizontal input is detected then update the current horizontal direction
        if (Mathf.Abs(inputDirection.x) > 0)
        {
            lastHorizontalDirection = new Vector2(Mathf.Sign(inputDirection.x), 0);
        }

        // If an input is being given then update the current direction
        if (inputDirection != Vector2.zero)
        {
            currentDirection = inputDirection;
        }
        // Otherwise the current direction is defaulted to the last horizontal direction inputted
        else
        {
            currentDirection = lastHorizontalDirection;
        }

        return currentDirection;
    }
}
