using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    PlayerBehaviour playerBehaviour;

    bool currentlyFixed;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerBehaviour = player.GetComponent<PlayerBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        // If not currently fixed, trail the player
        if (!currentlyFixed)
        {
            // Let the player move briefly to convey movement 
            // Then lock the camera relative to the player

            // Vector raycast in the horizontal and vertical axis and see whether there is a wall or ceiling to block camera travel?
        }
    }

    public void FixCamera(Vector2 position)
    {
        currentlyFixed = true;

        gameObject.transform.position = position;
    }

    public void UnfixCamera()
    {
        currentlyFixed = false;
    }
}
