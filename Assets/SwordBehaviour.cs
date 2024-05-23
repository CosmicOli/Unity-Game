using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwordBehaviour : MonoBehaviour
{
    public float damage;
    public AttackBehaviour attackBehaviour;
    public GameObject Swing;
    private GameObject swordSwing;

    private Vector3 currentDirection3D;

    // Start is called before the first frame update
    void Start()
    {
        attackBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<AttackBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (swordSwing != null)
        {
            swordSwing.transform.position = transform.position + currentDirection3D;

        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && swordSwing == null)
        {
            Vector2 currentDirection = attackBehaviour.currentDirection;
            currentDirection3D = new Vector3((int)(currentDirection.x), Mathf.Round(currentDirection.y)); // This favours up and down attacks as up and down are not needed in movement.

            Vector3 rotationVector = new Vector3(0, 0, -180 * Mathf.Atan2(currentDirection3D.x, currentDirection3D.y) / Mathf.PI);
            Quaternion rotationQuaternion = Quaternion.Euler(rotationVector);

            // Creates a swing of the sword in a direction
            swordSwing = Instantiate(Swing, transform.position + currentDirection3D, rotationQuaternion);
            swordSwing.GetComponent<SwingBehaviour>().damage = damage;

            // If hit, deal damage to enemy and do some knockback
        }
    }
}
