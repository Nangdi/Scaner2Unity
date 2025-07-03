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
        // 1. Grayscale 변환
        Mat grayMat = new Mat();
        Imgproc.cvtColor(scanMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        // 2. ArUco 딕셔너리 설정
        Dictionary dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        // 3. 마커 검출
        List<Mat> corners = new List<Mat>();
        Mat ids = new Mat();
        int markerId = -1;

        Aruco.detectMarkers(grayMat, dictionary, corners, ids);

        // 4. 마커가 감지되었을 경우 처리
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(scanMat, corners, ids);
            markerId = (int)ids.get(0, 0)[0]; // 첫 번째 마커 ID
            Debug.Log("감지된 마커 ID: " + markerId);
        }
        else
        {
            Debug.Log("마커 인식 안됨");
        }

        // 5. 결과 텍스처로 표시 (디버그용)
        Texture2D resultTex = new Texture2D(scanMat.cols(), scanMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(scanMat, resultTex); // 마커 그려진 상태
        ScanImage.texture = resultTex;
        scannedTex = resultTex;

        // 6. 결과 객체 생성 및 반환
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
