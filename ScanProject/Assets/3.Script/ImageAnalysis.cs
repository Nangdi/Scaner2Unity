using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.UnityUtils;
using System;
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
    [SerializeField]
    protected ArUcoMarkerDetector arucoMarkerDetector;

    public GameObject go;
    private ObjectScanData obScanData;
    public RawImage debugImage;
    Renderer renderer;
    Texture2D resultTex;
    protected Mat inputImage;
    public DetectInfo detectInfo;
    protected Mat perspectiveMat;
    protected Point startPoint;
    private void Start()
    {
    }

    //public void ProcessAnalysis(Texture2D scannedTexture)
    //{

    //    Mat imageMat = new Mat(scannedTexture.height, scannedTexture.width, CvType.CV_8UC3);
    //    OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTexture, imageMat);

    //    ////마커영역만 추출후 ID 받기
    //    //Texture2D markerImage = ExtractMarker(imageMat);

    //    int markerID = markerDetector.DetectMarker(scannedTexture);
    //    //Point markerPoint = scanInfo.standardOffset;

    //    obScanData = JsonManager.jsonManager.LoadData()[markerDetector.markerId];
    //    UnityEngine.Debug.Log("Detected markerID: " + markerDetector.markerId);


    //    MatOfPoint2f markerCorners = new MatOfPoint2f(markerDetector.corners[0]);
    //    int boxSize = obScanData.cropSize;


    //    int startX = /*(int)markerPoint.x */ obScanData.offsetX;
    //    int startY = /*(int)markerPoint.y */ obScanData.offsetY;
    //    Mat test =  markerDetector.CropFromPerspective(imageMat, markerCorners, startX, startY ,boxSize);
    //    //int startX = imageMat.cols() / 2 - boxSize / 2 + 13;
    //    //int startY = imageMat.rows() / 2 - boxSize / 2 + -47;
    //    // 좌표가 음수가 되지 않도록 제한
    //    //startX = Mathf.Max(startX, 0);
    //    //startY = Mathf.Max(startY, 0);

    //    // 잘라낼 범위 설정
    //    OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(startX, startY, boxSize, boxSize);
    //    Mat drawingMat = new Mat(imageMat, cropRect);

    //    //외각선(검정색) 제거하는 코드 추가할예정
    //    //Scalar lower = new Scalar(0, 0, 0);       // 완전 검정
    //    //Scalar upper = new Scalar(30, 30, 30);    // 어두운 회색까지 포함
    //    //Mat mask = new Mat();
    //    //Core.inRange(drawingMat, lower, upper, mask); // → 외곽선 위치만 흰색(255)

    //    //// ✅ 2. 여기에서 마스크를 팽창시킵니다 (얇은 선도 굵게 감지)
    //    //Mat kernel = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(5, 5));
    //    //Imgproc.dilate(mask, mask, kernel); // 요기!


    //    //// 2. 주변 픽셀로 외곽선을 지우고 채우기
    //    //Mat inpainted = new Mat();
    //    //Photo.inpaint(drawingMat, mask, inpainted, 5, Photo.INPAINT_TELEA); // 반경 3, 주변 색 보간

    //    //// 3. 마무리 블러로 부드럽게 연결
    //    //Imgproc.GaussianBlur(inpainted, inpainted, new Size(3, 3), 0);

    //    // 텍스처화
    //    resultTex = new Texture2D(test.cols(), test.rows(), TextureFormat.RGBA32, false);
    //    OpenCVForUnity.UnityUtils.Utils.matToTexture2D(test, resultTex);
    //    Imgproc.resize(test, test, new Size(2048, 2048)); //  해상도만 맞춤
    //    debugFrameDisplay.texture = resultTex;

    //    // 4. 오브젝트 선택 후 머티리얼 적용
    //    //GameObject go = JsonManager.jsonManager.objectList[obScanData.objectID];

    //    if (go != null)
    //    {
    //        renderer = go.GetComponent<Renderer>();
    //        if (renderer != null)
    //        {
    //            renderer.material.mainTexture = resultTex;
    //        }
    //    }
    //}
    public virtual void ProcessAnalysis(Texture2D scannedTexture)
    {
        //Tex ->Mat 변환
        Mat imageMat = new Mat(scannedTexture.height, scannedTexture.width, CvType.CV_8UC3);
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTexture, imageMat);
        inputImage = imageMat;


        UnityEngine.Debug.Log(arucoMarkerDetector);
        //scannedTexture Marker 정보 가져오기
        detectInfo = arucoMarkerDetector.GetDetectInfo(imageMat);
        //Point markerPoint = scanInfo.standardOffset;

        obScanData = CustomJsonManager.jsonManager.dataList[detectInfo.markerId];
        UnityEngine.Debug.Log("Detected markerID: " + detectInfo.markerId);

        MatOfPoint2f markerCorners = new MatOfPoint2f(detectInfo.markerCorners[0]);


        //perspectiveMat = AlignImageByMarker(detectInfo.scanMat, markerCorners);

        //detectInfo = arucoMarkerDetector.GetDetectInfo(perspectiveMat);
        Point[] points = markerCorners.toArray();
        startPoint = points[0]; // ← 바로 이게 좌측 상단
        int boxSize = obScanData.cropSize;
        int startX = (int)Math.Round(startPoint.x) + obScanData.offsetX;
        int startY = (int)Math.Round(startPoint.y) + obScanData.offsetY;

        //crop하는기능
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(startX, startY, boxSize, boxSize);
        Mat cropped = new Mat(inputImage, cropRect);
        ApplyTexture(cropped, debugImage);

        Texture2D cropTex = new Texture2D(cropped.cols(), cropped.rows(), TextureFormat.RGBA32, false);

        // 텍스처화
    }

    public void ApplyTexture(Mat mat ,RawImage debugDisplay = null)
    {
        resultTex = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(mat, resultTex);
        //OpenCVForUnity.UnityUtils.Utils.matToTexture2D(mat, resultTex);
        Imgproc.resize(mat, mat, new Size(2048, 2048)); //  해상도만 맞춤
        if(debugDisplay != null)
        {
            debugDisplay.texture = resultTex;

        }

        // 4. 오브젝트 선택 후 머티리얼 적용
        //GameObject go = JsonManager.jsonManager.objectList[obScanData.objectID];

        if (go != null)
        {
            renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = resultTex;
            }
        }
    }
    public Mat AlignImageByMarker(Mat srcImg, MatOfPoint2f markerCorners)
    {
        Point[] pts = markerCorners.toArray();
        Point topLeft = pts[0];
        Point topRight = pts[1];

        Point dir = new Point(topRight.x - topLeft.x, topRight.y - topLeft.y);

        double angleRad = Mathf.Atan2((float)dir.y, (float)dir.x);
        double angleDeg = angleRad * (180.0 / Math.PI);

        Point center = new Point(srcImg.cols() / 2, srcImg.rows() / 2);
        Mat rotationMatrix = Imgproc.getRotationMatrix2D(center, -angleDeg, 1.0);

        Mat aligned = new Mat();
        Imgproc.warpAffine(srcImg, aligned, rotationMatrix, srcImg.size(), Imgproc.INTER_LINEAR, Core.BORDER_REPLICATE);

        return aligned;
    }
}
