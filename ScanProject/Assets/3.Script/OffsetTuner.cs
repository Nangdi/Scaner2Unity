using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEditor;
using OpenCVForUnity.ArucoModule;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.GridLayoutGroup;
using System.Collections.Generic;
using UnityEngine.Timeline;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using OpenCvSharp.Demo;
using System;
public class OffsetTuner : ImageAnalysis 
{

    [Header("Input")]
    public Texture2D scannedTex;

    [Header("Offset Controls")]
    [Range(-840, 840)] public int offsetX = 0;
    [Range(-1180, 1180)] public int offsetY = 0;
    [Range(-0, 2048)] public int cropSize = 1509;
    private int startOffsetY = 300;
    private int startOffsetX = 0;
    private ObjectScanData objectData;
    private Texture2D cropTex;

    [Header("Live Crop")]
    public RawImage CropDisplay;
    private Mat scannedMat;
    private Point markerTopLeft = new Point(0,0); // 마커 좌상단 (임시값)

    private bool isTuningStart;
    
    private int previousX;
    private int previousY;
    private int previousCropSize;
    private bool isTrigger;

    void Start()
    {
      
    }
    

    void Update()
    {

        if (!isTuningStart) return;

        // 마커 기준 crop 영역 계산
        int x = (int)Math.Round(startPoint.x) + objectData.offsetX + offsetX;
        int y = (int)Math.Round(startPoint.y) + objectData.offsetY + offsetY;
        if (x < 0 || y < 0 || x + cropSize > scannedMat.cols() || y + cropSize > scannedMat.rows())
        {
            offsetX = previousX - objectData.startOffset- (int)Math.Round(startPoint.x);
            offsetY = previousY - objectData.startOffset- (int)Math.Round(startPoint.y);
            cropSize = previousCropSize;
            return;
        }
        if (previousX == x && previousY == y && cropSize == previousCropSize)
        {
            return;
        }
        else
        {
            previousX = x;
            previousY = y;
            previousCropSize = cropSize;
        }
        Debug.Log($"추출적용 시작좌표({(int)Math.Round(startPoint.x)},{(int)Math.Round(startPoint.y)})");
        Debug.Log($"추출적용 시작좌표({x},{y})");


        // crop 영역 추출
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(x, y, cropSize, cropSize);
        Mat cropped = new Mat(scannedMat, cropRect);
        
        cropTex = new Texture2D(cropped.cols(), cropped.rows(), TextureFormat.RGBA32, false);
        // 디버그용 미리보기

        Utils.matToTexture2D(cropped, cropTex);
        Utils.matToTexture2D(cropped, cropTex);
        if (!isTrigger)
        {
            isTrigger = true;
        }
        else
        {
           

        }
        CropDisplay.texture = cropTex;
        ApplyMatarial();


    }
    public void MarkerOffSetInit()
    {
        //arucoMarkerDetector.GetDetectInfo();
        RoadData();

        scannedMat = inputImage;
        //ApplyTexture(scannedMat, CropDisplay);

        isTuningStart = true;
    }
    public void ApplyMatarial()
    {
        if (go != null)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = cropTex;
            }
        }
    }
    public void SaveData()
    {
        objectData.objectID = arucoMarkerDetector.markerId;
        objectData.offsetX = offsetX;
        objectData.offsetY -= offsetY;
        objectData.cropSize = cropSize;
        offsetX = 0;
        offsetY = 0;
        JsonManager.jsonManager.dataList[objectData.objectID] = objectData;
        JsonManager.jsonManager.SaveData();
    }
    private void RoadData()
    {
        objectData = JsonManager.jsonManager.dataList[arucoMarkerDetector.markerId];
        go = JsonManager.jsonManager.objectList[objectData.objectID];
        Debug.Log(objectData.offsetY);

        startOffsetX = objectData.offsetX;
        startOffsetY = objectData.offsetY;
        cropSize = objectData.cropSize;
        Debug.Log("데이터로드완료");
    }
}
