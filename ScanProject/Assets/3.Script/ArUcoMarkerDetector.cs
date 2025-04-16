using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCvSharp.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using static OpenCVForUnityExample.ArUcoCreateMarkerExample;
public class DetectInfo
{
    public Mat scanMat;
    public List<Mat> markerCorners;
    public int markerId;

    public DetectInfo(Mat mat, List<Mat> corner, int id)
    {
        scanMat = mat;
        markerCorners = corner;
        markerId = id;
    }
};
public class ArUcoMarkerDetector : MonoBehaviour
{
    public RawImage ScanImage; // 카메라 또는 이미지 출력용
    public RawImage correctionImage; // 카메라 또는 이미지 출력용
    public List<Mat> corners;
    public Texture2D scannedTex;
    public int markerId; 
    public Mat perspectiveMat; // 수평수직 보정후 Mat
    public Point standardOffset { get; private set; } //보정후 crop 기준 offset
    void Start()
    {
    }

    public DetectInfo GetDetectInfo(Mat scanMat)
    {
       
        // 1. 텍스처 → Mat


        //디버그용 현재 imgMat 
        Texture2D resultTex = new Texture2D(scanMat.cols(), scanMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(scanMat, resultTex);
        scannedTex = resultTex;
        Utils.matToTexture2D(scanMat, resultTex);
        ScanImage.texture = resultTex;

        // 2. Grayscale로 변환
        Mat grayMat = new Mat();
        Imgproc.cvtColor(scanMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        // 3. 딕셔너리 정의
        Dictionary dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        // 4. 마커 검출
        corners = new List<Mat>();
        Mat ids = new Mat();
        // 5. 결과 표시 
        Aruco.detectMarkers(grayMat, dictionary, corners, ids);
        //Utils.matToTexture2D(scanMat, resultTex);
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(scanMat, corners, ids);
            for (int i = 0; i < ids.total(); i++)
            {
                markerId = (int)ids.get(i, 0)[0];
                //persepectiveInfo.markerId = markerId;
                Debug.Log(" 감지된 마커 ID: " + markerId);

            }
        }
        else
        {

            Debug.Log("마커인식안됨");

        }

        // 6. 결과를 텍스처로 표시
     

        DetectInfo detectInfo = new DetectInfo(scanMat, corners, markerId);
        return detectInfo;
    }
   

    public Mat PerspectiveTexture(Mat srcImg, MatOfPoint2f markerCorners)
    {
        float expand = 8.0f;
        Point[] m = markerCorners.toArray();
        Point topLeft = m[0];
        Point topRight = m[1];
        Point bottomRight = m[2];
        Point bottomLeft = m[3];

        // 방향 벡터 계산
        Point rightVec = new Point(topRight.x - topLeft.x, topRight.y - topLeft.y);
        Point downVec = new Point(bottomLeft.x - topLeft.x, bottomLeft.y - topLeft.y);

        // 확장된 사각형 꼭짓점 정의
        MatOfPoint2f src = new MatOfPoint2f(
            topLeft,
            new Point(topLeft.x + rightVec.x * expand, topLeft.y + rightVec.y * expand),
            new Point(topLeft.x + rightVec.x * expand + downVec.x * expand, topLeft.y + rightVec.y * expand + downVec.y * expand),
            new Point(topLeft.x + downVec.x * expand, topLeft.y + downVec.y * expand)
        );

        // 목표 사각형 크기
        int W = 1500;
        int H = 2120;

        MatOfPoint2f dst = new MatOfPoint2f(
            new Point(0, 0),
            new Point(W, 0),
            new Point(W, H),
            new Point(0, H)
        );

        // 보정 수행
        Mat perspectiveCorrected = new Mat();
        Mat transform = Imgproc.getPerspectiveTransform(src, dst);
        Imgproc.warpPerspective(srcImg, perspectiveCorrected, transform, new Size(W, H), Imgproc.INTER_LINEAR,
    Core.BORDER_REPLICATE);




        Texture2D resultTex = new Texture2D(perspectiveCorrected.cols(), perspectiveCorrected.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(perspectiveCorrected, resultTex);

        correctionImage.texture = resultTex;
        //// 5. 마커 기준 offset 위치에서 cropRect 정의
        //int cropX = Mathf.Clamp(offsetX, 0, (int)markerSize - cropSize);
        //int cropY = Mathf.Clamp(offsetY, 0, (int)markerSize - cropSize);
        //OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(offsetX, offsetY, cropSize, cropSize);

        // 6. Crop 수행
        //Mat cropped = new Mat(perspectiveCorrected, cropRect);

        return perspectiveCorrected;
    }
}
