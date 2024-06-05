using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class middleBehaviour : MonoBehaviour
{
    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindAnyObjectByType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);
    }
}
