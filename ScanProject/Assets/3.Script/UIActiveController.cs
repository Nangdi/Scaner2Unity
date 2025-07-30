using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActiveController : MonoBehaviour
{
    [SerializeField]
    private GameObject settingCanvas;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingCanvas.SetActive(!settingCanvas.activeSelf);
        }
    }
}
