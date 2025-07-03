using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameTagController : MonoBehaviour
{
    public Transform targetHeadPos;

    private void Update()
    {
        if(targetHeadPos == null)
        {
            Debug.Log("TargetPos가 null 입니다.");
            return;
        }
        transform.position = targetHeadPos.position ;

    }
}
