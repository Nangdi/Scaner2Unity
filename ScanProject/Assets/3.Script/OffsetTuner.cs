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
public class OffsetTuner : MonoBehaviour
{
    [SerializeField]
    private ArUcoMarkerDetector arucoMarkerDetector;

    [Header("Input")]
    public RawImage scannedImageDisplay;
    public Texture2D scannedTex;
    public Texture2D UVmap;

    [Header("Offset Controls")]
    [Range(-840, 840)] public int offsetX = 0;
    [Range(-1180, 1180)] public int offsetY = 0;
    [Range(-0, 2048)] public int cropSize = 1509;
    private int startOffsetY = 300;
    private int startOffsetX = 0;
    public GameObject ob;
    private ObjectScanData objectData;
    private Texture2D cropTex;

    [Header("Live Crop")]
    public RawImage CropDIsplay;
    private Mat scannedMat;
    private Point markerTopLeft; // ��Ŀ �»�� (�ӽð�)

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

        // ��Ŀ ���� crop ���� ���
        int x = (int)markerTopLeft.x + startOffsetX + offsetX;
        int y = (int)markerTopLeft.y + startOffsetY + offsetY;
        if (x < 0 || y < 0 || x + cropSize > scannedMat.cols() || y + cropSize > scannedMat.rows())
        {
            offsetX = previousX - startOffsetX-(int)markerTopLeft.x;
            offsetY = previousY - startOffsetY-(int)markerTopLeft.y;
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

        Debug.Log($"�������� ������ǥ({x},{y})");


        // crop ���� ����
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(x, y, cropSize, cropSize);
        Mat cropped = new Mat(scannedMat, cropRect);
        
        cropTex = new Texture2D(cropped.cols(), cropped.rows(), TextureFormat.RGBA32, false);
        // ����׿� �̸�����

        Utils.matToTexture2D(cropped, cropTex);
        if (!isTrigger)
        {
            isTrigger = true;
        }
        else
        {
            Utils.matToTexture2D(cropped, cropTex);

        }
        CropDIsplay.texture = cropTex;
        ApplyMatarial();


    }
    public void MarkerOffSetInit()
    {
        //arucoMarkerDetector.DetectMarker(scannedTex);
        scannedTex = arucoMarkerDetector.scannedTex;
       
        RoadData();
       

        Debug.Log($"markerID{arucoMarkerDetector.markerId}");
        Debug.Log($"objectID{objectData.objectID}");
        double[] ptArr = arucoMarkerDetector.corners[0].get(0, 0); // index 0 = �»��
        markerTopLeft = new Point(ptArr[0], ptArr[1]);
        Debug.Log(markerTopLeft);
        scannedMat = new Mat(scannedTex.height, scannedTex.width, CvType.CV_8UC3);
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTex, scannedMat);
        scannedImageDisplay.texture = scannedTex;

        isTuningStart = true;
    }
    public void ApplyMatarial()
    {
        if (ob != null)
        {
            Renderer renderer = ob.GetComponent<Renderer>();
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
        ob = JsonManager.jsonManager.objectList[objectData.objectID];
        Debug.Log(objectData.offsetY);

        startOffsetX = objectData.offsetX;
        startOffsetY = objectData.offsetY;
        cropSize = objectData.cropSize;
        Debug.Log("�����ͷε�Ϸ�");
    }
    public void CaculateCropSize()
    {
        int width = UVmap.width;
        int height = UVmap.height;

        // �Ǽ������� ��Ȯ�ϰ� ���
        float dpi = 72f;
        float mmPerInch = 25.4f;

        float widthMM = (width / dpi) * mmPerInch;
        float heightMM = (height / dpi) * mmPerInch;
        Debug.Log($"width : {widthMM} , height : {heightMM}");
    }
}
