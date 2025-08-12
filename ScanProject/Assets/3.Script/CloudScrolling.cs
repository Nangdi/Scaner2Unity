using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CloudScrolling : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(1, 2);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        if(transform.position.x > 120)
        {
            Vector3 pos = transform.position;
            pos.x = -120;
            transform.position = pos;
        }
    }
}
