using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwordBehaviour : MonoBehaviour
{
    // These are constants used to determine sword damage, knockback and swing time
    [SerializeField]
    private float Damage;
    [SerializeField]
    private float EnemyKnockbackStrength;
    [SerializeField]
    private Vector2 PlayerKnockbackStrength;
    [SerializeField]
    private float SwingTime;

    // This constant refers to the Swing game object
    [SerializeField]
    private GameObject Swing;

    // This variable stores a reference to the instantiated Swing game object
    private GameObject swordSwing;

    // These "constants" refer to scripts that control generic attacking and the player
    private AttackBehaviour attackBehaviour;
    private PlayerBehaviour playerBehaviour;

    // This variable stores where the player is currently pointing
    private Vector3 CurrentDirection3D;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        attackBehaviour = player.GetComponent<AttackBehaviour>();
        playerBehaviour = player.GetComponent<PlayerBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        // If a Swing object currently exists, place it correctly relative to the player
        if (swordSwing != null)
        {
            swordSwing.transform.position = transform.position + CurrentDirection3D;
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (playerBehaviour.currentlyAbleToInput)
        {
            // If currently swinging and there is no instantiated Swing object, create one
            if (context.performed && swordSwing == null)
            {
                // Gets the current swing direction
                Vector2 CurrentDirection = attackBehaviour.currentDirection;
                CurrentDirection3D = new Vector3((int)(CurrentDirection.x), Mathf.Round(CurrentDirection.y)); // This favours up and down attacks as up and down are not needed in movement.

                // Gets the current swing rotation
                Vector3 rotationVector = new Vector3(0, 0, -180 * Mathf.Atan2(CurrentDirection3D.x, CurrentDirection3D.y) / Mathf.PI);
                Quaternion rotationQuaternion = Quaternion.Euler(rotationVector);

                // Creates a swing of the sword in a direction
                swordSwing = Instantiate(Swing, transform.position + CurrentDirection3D, rotationQuaternion);

                // Gets the script associated with the Swing game object
                SwingBehaviour swordSwingBehaviour = swordSwing.GetComponent<SwingBehaviour>();

                // Assigns all the constants the Swing game object needs
                swordSwingBehaviour.AssignConstants(Damage, EnemyKnockbackStrength * CurrentDirection3D, new Vector3(-1 * PlayerKnockbackStrength.x * CurrentDirection3D.x, -1 * PlayerKnockbackStrength.y * CurrentDirection3D.y, 0), SwingTime, playerBehaviour);
            }
        }
    }
}
