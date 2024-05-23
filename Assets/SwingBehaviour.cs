using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingBehaviour : MonoBehaviour
{
    public float damage;
    public float swingTime;
    private float timer = 0;
    private List<GameObject> hitEnemies = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.5)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject hitObject = collision.gameObject;

        if (!hitEnemies.Contains(hitObject))
        {
            hitEnemies.Add(hitObject);

            switch (hitObject.layer)
            {
                case 3:
                    hitObject.GetComponent<GenericEnemyBehaviour>().TakeDamage(damage);
                    break;

                case 7:
                    Debug.Log("hit wall");
                    break;

                default:
                    Debug.Log("hit other");
                    break;
            }
        }
    }
}
