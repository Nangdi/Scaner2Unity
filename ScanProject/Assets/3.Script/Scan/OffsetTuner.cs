using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ArucoModule;
using static UnityEngine.UI.GridLayoutGroup;
using System.Collections.Generic;
using UnityEngine.Timeline;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using System;
public class OffsetTuner : ImageAnalysis 
{
    
    [Header("Input")]
    public Texture2D scannedTex;

    [Header("Offset Controls")]
    [Range(-840, 1500)] public int offsetX = 0;
    [Range(-1180, 1180)] public int offsetY = 0;
    [Range(-0, 3000)] public int cropSize = 1509;
     public float hRatio = 1.5f;
     public float expandRatio = 1.5f;
    private int startOffsetY = 300;
    private int startOffsetX = 0;
    private ObjectScanData objectData;
    private Texture2D cropTex;

    [Header("Live Crop")]
    public RawImage CropDisplay;
    private Mat scannedMat= new Mat();
    private Point markerTopLeft = new Point(0,0); // 마커 좌상단 (임시값)

    private bool isTuningStart;
    
    private int previousX;
    private int previousY;
    private int previousCropSize;
    private int previouslowerValue = 0;
    private int previousUpperValue = 50;
    private int previouskernelValue = 4;
    private int previousLerpValue = 3;
    private float previousHRatio = 1.5f;
    private float previousExpandRatio = 1;
    private bool isTrigger;

    [SerializeField]
    private List<UIButtonIncrementer> uIButtonIncrementers;
    void Start()
    {
        StartCoroutine(dataIntit());
    }
    

    void Update()
    {

        if (!isTuningStart) return;

        UpdateCropView();
    }
    private void UpdateCropView()
    {
        // 마커 기준 crop 영역 계산
        int x = (int)Math.Round(startPoint.x) + objectData.offsetX + offsetX;
        int y = (int)Math.Round(startPoint.y) + objectData.offsetY + offsetY;
        //if (x < 0 || y < 0 || x + cropSize > scannedMat.cols() || y + cropSize > scannedMat.rows())
        //{
        //    offsetX = scannedMat.cols();
        //    offsetY = scannedMat.rows();
        //    cropSize = previousCropSize;
        //    return;
        //}
        if (!ChangedValue(x, y))
        {
            return;
        }
        else
        {
            CashingData(x ,y );
        }
        Debug.Log($"추출적용 시작좌표({(int)Math.Round(startPoint.x)},{(int)Math.Round(startPoint.y)})");
        Debug.Log($"추출적용 시작좌표({x},{y})");
        //scannedTex = new Texture2D(scannedMat.cols(), scannedMat.rows(), TextureFormat.RGBA32, false);
        //Utils.matToTexture2D(scannedMat, scannedTex);
        //arucoMarkerDetector.correctionImage.texture = scannedTex;

        // crop 영역 추출
        //600 50  300 150
        float cropY = cropSize / hRatio;
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(x, y, cropSize, (int)cropY);
        Mat cropped;
        if (isCropName)
        {
            cropped = new Mat(nameImage, cropRect).clone();
        }
        else
        {
            cropped = new Mat(scannedMat, cropRect).clone();
        }



        Core.flip(cropped, cropped, 0); // X축 기준 좌우 반전
                                        //cropTex = ExpandCropedMat(cropped, expandRatio);
                                        //if(pr)
        if (cropTex == null || cropTex.width != cropped.cols() || cropTex.height != cropped.rows())
        {
            cropTex = new Texture2D(cropped.cols(), cropped.rows(), TextureFormat.RGBA32, false);
        }
        // 디버그용 미리보기

        Utils.matToTexture2D(cropped, cropTex);
        //Utils.matToTexture2D(cropped, cropTex);
        CropDisplay.texture = cropTex;
        ApplyMaterial();
    }


    public void InitSetting()
    {
        //arucoMarkerDetector.GetDetectInfo();
        RoadData();

        scannedMat = inputImage;
        //Core.flip(scannedMat, scannedMat, 0); // X축 기준 좌우 반전
        //ApplyTexture(scannedMat, CropDisplay);
        foreach (var item in uIButtonIncrementers)
        {
            item.InitValue();
        }

        isTuningStart = true;
    }
    public void ApplyMaterial()
    {
        if (go != null)
        {
            Renderer renderer = go.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Debug.Log(renderer);
                //Debug.Log(renderer.material);
                //Debug.Log(renderer.material.mainTexture);
                renderer.materials[0].mainTexture = cropTex;
            }
        }
    }
    public void SaveData()
    {
        //objectData.objectID = arucoMarkerDetector.markerId;
        objectData.offsetX += offsetX;
        objectData.offsetY += offsetY;
        objectData.cropSize = cropSize;
        objectData.hRatio = hRatio;
        offsetX = 0;
        offsetY = 0;
        CustomJsonManager.jsonManager.dataList[objectData.objectID] = objectData;
        CustomJsonManager.jsonManager.SaveScanData();
    }
    private void RoadData()
    {
        objectData = CustomJsonManager.jsonManager.dataList[detectInfo.markerId];
        if (isCropName)
        {
        objectData = CustomJsonManager.jsonManager.dataList[3];

        }
        //go = CustomJsonManager.jsonManager.objectList[objectData.objectID];
        Debug.Log(objectData.offsetY);

        startOffsetX = objectData.offsetX;
        startOffsetY = objectData.offsetY;
        cropSize = objectData.cropSize;
        hRatio = objectData.hRatio;
        Debug.Log("데이터로드완료");
    }
    private bool ChangedValue(int x , int y)
    {
        //Debug.Log("밸류변했는지 체크");
        if (previousX == x && previousY == y && cropSize == previousCropSize && previouslowerValue == lowerValue && previousUpperValue == UpperValue &&
           previouskernelValue == kernelValue && previousLerpValue == LerpValue && Mathf.Approximately(previousHRatio , hRatio) && Mathf.Approximately(previousExpandRatio , expandRatio))
        {
            return false;
        }
        return true;
    }
    private void CashingData(int x , int y )
    {
        previousX = x;
        previousY = y;
        previousCropSize = cropSize;
        previouslowerValue = lowerValue;
        previousUpperValue = UpperValue;
        previouskernelValue = kernelValue;
        previousLerpValue = LerpValue;
        previousHRatio = hRatio;
        previousExpandRatio = expandRatio;
    }
    public float ModifyValue(string variableName, float amount)
    {
        switch (variableName)
        {
            case "X":
                offsetX += Mathf.RoundToInt(amount);
                return offsetX;
            case "Y":
                offsetY += Mathf.RoundToInt(amount);
                return offsetY;
            case "CropSize":
                cropSize += Mathf.RoundToInt(amount);
                return cropSize;
            case "HRatio":
                hRatio += amount;
                return hRatio;
                
        }
        Debug.Log($"{variableName} updated");
        return 0;
    }
    public void UpdateValue()
    {

    }
}
