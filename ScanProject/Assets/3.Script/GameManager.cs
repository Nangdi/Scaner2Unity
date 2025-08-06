using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //���ִ� ��� �ð� �ּ� �ִ�
    //�ȱ� : ��� Ȯ�� 
    //
    [SerializeField]
    private RawImage uvFrame;
    [SerializeField]
    private Texture2D[] uvImages;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            uvFrame.texture = uvImages[0];
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            uvFrame.texture = uvImages[1];
        }
    }
}
