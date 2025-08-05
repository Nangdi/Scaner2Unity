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

    [Header("ScanFile감시")]
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

        // 폴더가 없으면 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log(" 폴더 생성됨: " + folderPath);
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
        Debug.Log($"새로운 스캔 감지: {e.FullPath}");
        MainThreadDispatcher.Enqueue(() =>
        {
            LoadTexture(e.FullPath); // 여기서 Texture2D 생성 + LoadImage 가능
        });
    }

    private void LoadTexture(string path)
    {
    
        //try
        //{
            byte[] fileData = File.ReadAllBytes(path);
            Debug.Log("시도: Texture2D 생성");
            CurrentScanImage = new Texture2D(2, 2); // LoadImage가 사이즈 덮어씀
            Debug.Log("시도: LoadImage 호출");
            CurrentScanImage.LoadImage(fileData); // 이미지 로드 (예외 발생 가능)
            Debug.Log("로드 성공");
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
        //    Debug.LogError("이미지 로드 실패: " + ex.Message);
        //}

    }
}
