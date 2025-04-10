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
public class OffsetTuner : MonoBehaviour
{
    [SerializeField]
    private ArUcoMarkerDetector arucoMarkerDetector;

    [Header("Input")]
    public RawImage scannedImageDisplay;
    public Texture2D scannedTex;

    [Header("Offset Controls")]
    [Range(-840, 840)] public int offsetX = 0;
    [Range(-1180, 1180)] public int offsetY = 242;
    [Range(-0, 2048)] public int cropSize = 1509;
    private Texture2D cropTex;
    public GameObject ob;

    [Header("Live Crop")]
    public RawImage CropDIsplay;
    private Mat scannedMat;
    private Point markerTopLeft; // 마커 좌상단 (임시값)

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
        if (Input.GetKey(KeyCode.DownArrow))
        {
            offsetY -= 1;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            offsetY += 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            offsetX -= 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            offsetX += 1;
        }
        if (!isTuningStart) return;

        // 마커 기준 crop 영역 계산
        //int x = scannedMat.cols() / 2 - cropSize / 2 + offsetX;
        //int y = scannedMat.rows() / 2 - cropSize / 2 + offsetY;
        int x = (int)markerTopLeft.x + offsetX;
        int y = (int)markerTopLeft.y + offsetY;
        if (x < 0 || y < 0 || x + cropSize > scannedMat.cols() || y + cropSize > scannedMat.rows())
        {
            x = previousX;
            y = previousY;
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

        Debug.Log("추출적용");


        // crop 영역 추출
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(x, y, cropSize, cropSize);
        Mat cropped = new Mat(scannedMat, cropRect);

        cropTex = new Texture2D(cropped.cols(), cropped.rows(), TextureFormat.RGBA32, false);
        // 디버그용 미리보기

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
    public void OffSetInit()
    {
        //arucoMarkerDetector.DetectMarker(scannedTex);
        scannedTex = arucoMarkerDetector.scannedTex;
        double[] ptArr = arucoMarkerDetector.corners[0].get(0, 0); // index 0 = 좌상단
        markerTopLeft = new Point(ptArr[0], ptArr[1]);
        Debug.Log(markerTopLeft);
        scannedMat = new Mat(scannedTex.height, scannedTex.width, CvType.CV_8UC3);
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTex, scannedMat);
        scannedImageDisplay.texture = scannedTex;

        isTuningStart = true;
    }
    void FlipTextureVertically(Texture2D tex)
    {
        var pixels = tex.GetPixels();
        int width = tex.width;
        int height = tex.height;

        Color[] flipped = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            System.Array.Copy(pixels, y * width, flipped, (height - y - 1) * width, width);
        }

        tex.SetPixels(flipped);
        tex.Apply();
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

}
