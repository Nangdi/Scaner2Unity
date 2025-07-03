using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ScanController : MonoBehaviour
{
    public void StartScan()
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, "ScanScript.exe");
        exePath = Path.GetFullPath(exePath);

        if (File.Exists(exePath))
        {
            Process.Start(exePath);
            UnityEngine.Debug.Log(exePath);
            UnityEngine.Debug.Log("스캔 실행파일 호출 성공");
        }
        else
        {
            UnityEngine.Debug.LogError("실행파일이 존재하지 않습니다: " + exePath);
        }
    }
}
