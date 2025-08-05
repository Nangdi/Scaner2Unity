using OpenCvSharp.Demo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScanDataManager : MonoBehaviour
{
    public enum Mode
    {
        Tuning,
        Game
    }
    [SerializeField]
    private ImageAnalysis imageAnalysis;

    [Header("ScanFile����")]
    private FileSystemWatcher watcher;


    public Texture2D CurrentScanImage ;
    private string folderPath;
    private string filePath;
    public string fileName;
    public string fileName1;
    public Mode mode;
    

    void Awake()
    {
       
        folderPath = Path.Combine(Application.streamingAssetsPath, "DataFiles");

        // ������ ������ ����
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log(" ���� ������: " + folderPath);
        }

    }
    private void Start()
    {
        watcher = new FileSystemWatcher(folderPath, "*.jpg");
        watcher.Created += OnNewScan;
        watcher.EnableRaisingEvents = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            string test = "C:\\Users\\Munser01\\Documents\\GitHub\\Scaner2Unity\\ScanProject\\Assets\\StreamingAssets\\DataFiles\\";
            LoadTexture(test+ fileName+".jpg");
        }
        else if(Input.GetKeyDown(KeyCode.W))
        {
            string test = "C:\\Users\\Munser01\\Documents\\GitHub\\Scaner2Unity\\ScanProject\\Assets\\StreamingAssets\\DataFiles\\";
            LoadTexture(test + fileName1 + ".jpg");
        }
      
    }
    private void OnNewScan(object sender, FileSystemEventArgs e)
    {
        Debug.Log($"���ο� ��ĵ ����: {e.FullPath}");
        MainThreadDispatcher.Enqueue(() =>
        {
            LoadTexture(e.FullPath); // ���⼭ Texture2D ���� + LoadImage ����
        });
    }

    private void LoadTexture(string path)
    {
    
        //try
        //{
            byte[] fileData = File.ReadAllBytes(path);
            Debug.Log("�õ�: Texture2D ����");
            CurrentScanImage = new Texture2D(2, 2); // LoadImage�� ������ ���
            Debug.Log("�õ�: LoadImage ȣ��");
            CurrentScanImage.LoadImage(fileData); // �̹��� �ε� (���� �߻� ����)
            Debug.Log("�ε� ����");
        //switch (mode)
        //{
        //    case Mode.Tuning:
        //        offsetTuner.OffSetInit(CurrentScanImage);
        //        break;
        //    case Mode.Game:
        //        imageAnalysis.ProcessAnalysis(CurrentScanImage);
        //        break;
        //}
        imageAnalysis.ProcessAnalysis(CurrentScanImage);
        //StartCoroutine(imageAnalysis.ProcessAnalysisCoroutine(CurrentScanImage));
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError("�̹��� �ε� ����: " + ex.Message);
        //}

    }
}
