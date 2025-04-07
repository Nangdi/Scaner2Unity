using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class ImageAnalysis : MonoBehaviour
{
    private ScanDataManager ScanDataManager;
    private ArUcoMarkerDetector markerDetector;
    public GameObject lightningPrefab;
    public GameObject boxPrefab;

    public RawImage debugFrameDisplay;
    public int detectRange = 400;

    Renderer renderer;
    Texture2D resultTex;

    private void Start()
    {
        TryGetComponent(out ScanDataManager);
        TryGetComponent(out markerDetector);
    }

    public void ProcessAnalysis(Texture2D scannedTexture)
    {
        Mat imageMat = new Mat(scannedTexture.height, scannedTexture.width, CvType.CV_8UC3);
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTexture, imageMat);

        Texture2D markerImage = ExtractMarker(imageMat);

        int markerID = markerDetector.DetectMarker(markerImage);
       
        UnityEngine.Debug.Log("Detected markerID: " + markerID);

        int boxSize = 1600; // 18cm * 100px
        int centerX = imageMat.cols() / 2;
        int centerY = imageMat.rows() / 2;

        int startX = centerX - boxSize / 2;
        int startY = centerY - boxSize / 2+50;

        // ��ǥ�� ������ ���� �ʵ��� ����
        startX = Mathf.Max(startX, 0);
        startY = Mathf.Max(startY, 0);

        // �߶� ���� ����
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(startX, startY, boxSize, boxSize);
        Mat drawingMat = new Mat(imageMat, cropRect);



        //// 3. �߾� �׸�Ʋ ����
        //OpenCVForUnity.CoreModule.Rect drawRect = new OpenCVForUnity.CoreModule.Rect(0, detectRange, imageMat.cols(), imageMat.rows() - detectRange);
        //Mat drawingMat = new Mat(imageMat, drawRect);

        // �ܰ��� ����
        Mat gray = new Mat();
        Imgproc.cvtColor(drawingMat, gray, Imgproc.COLOR_BGR2GRAY);
        Imgproc.GaussianBlur(gray, gray, new Size(5, 5), 0);
        Mat edges = new Mat();
        Imgproc.Canny(gray, edges, 100, 200);

        // �ܰ��� �̿� ����ŷ ó��
        Mat mask = new Mat();
        Core.bitwise_not(edges, mask);

        Mat coloredRegion = new Mat();
        drawingMat.copyTo(coloredRegion, mask);

        // �ؽ�óȭ
        resultTex = new Texture2D(coloredRegion.cols(), coloredRegion.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(coloredRegion, resultTex);
        Imgproc.resize(drawingMat, drawingMat, new Size(2048, 2048)); //  �ػ󵵸� ����
        debugFrameDisplay.texture = resultTex;

        // 4. ������Ʈ ���� �� ��Ƽ���� ����
        GameObject go = null;
        if (markerID == 1) go = Instantiate(lightningPrefab);
        else if (markerID == 2) go = Instantiate(boxPrefab);
        else UnityEngine.Debug.LogWarning("Unknown shape, no object spawned.");

        if (go != null)
        {
            renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = resultTex;
            }
        }
    }

    private Texture2D ExtractMarker(Mat imageMat)
    {

        // 2. ��� ��Ŀ �ν� (0,0) ���� 100x100
        OpenCVForUnity.CoreModule.Rect shapeRect = new OpenCVForUnity.CoreModule.Rect(0, 0, detectRange, detectRange);
        Mat shapeROI = new Mat(imageMat, shapeRect);

        Texture2D marker = new Texture2D(shapeROI.cols(), shapeROI.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(shapeROI, marker);
        return marker;
    }
    private void SetPosition()
    {
        renderer.material.mainTexture = resultTex;
    }
}
