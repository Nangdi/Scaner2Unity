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
            UnityEngine.Debug.Log("��ĵ �������� ȣ�� ����");
        }
        else
        {
            UnityEngine.Debug.LogError("���������� �������� �ʽ��ϴ�: " + exePath);
        }
    }
}
