using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpawnEffectController : MonoBehaviour
{
    [SerializeField]
    private ImageAnalysis ImageAnalysis;
    public ObjectSpawner spawner;
    public Vector3 targetPosition = new Vector3();
    private void Start()
    {

        ImageAnalysis = FindObjectOfType<ImageAnalysis>();
        spawner = FindObjectOfType<ObjectSpawner>();
       
    }
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5);
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < 0.1f)
        {
            ImageAnalysis.go = spawner.CreatObject(transform);
            Destroy(gameObject);
            //transform.LookAt(CameraDir);
        }
    }
}
