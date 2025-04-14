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
public class ScanInfo
{
    public Mat correctMat;
    public Point standardOffset;
    public int markerId;

    public ScanInfo(Mat mat, Point offset, int id)
    {
        correctMat = mat;
        standardOffset = offset;
        markerId = id;
    }
};
public class ArUcoMarkerDetector : MonoBehaviour
{
    public RawImage debugMarkerDisplay; // 카메라 또는 이미지 출력용
    public RawImage debugMarkerDisplay1; // 카메라 또는 이미지 출력용
    public List<Mat> corners;
    public Texture2D scannedTex;
    public int markerId; 
    public Mat perspectiveMat; // 수평수직 보정후 Mat
    public Point standardOffset { get; private set; } //보정후 crop 기준 offset
    void Start()
    {
    }

    public int DetectMarker(Texture2D marker)
    {
       
        scannedTex = marker;
        // 1. 텍스처 → Mat

        Mat imgMat = new Mat(marker.height, marker.width, CvType.CV_8UC3);
        Utils.texture2DToMat(marker, imgMat);

        // 2. Grayscale로 변환
        Mat grayMat = new Mat();
        Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        // 3. 딕셔너리 정의
        Dictionary dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        // 4. 마커 검출
        corners = new List<Mat>();
        Mat ids = new Mat();
        Aruco.detectMarkers(grayMat, dictionary, corners, ids);
        //ScanInfo persepectiveInfo = new ScanInfo(marker, null, 0);
        //perspectiveMat = persepectiveInfo.correctMat;
        //standardOffset = persepectiveInfo.standardOffset;
        // 5. 결과 표시
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(imgMat, corners, ids);
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
        Texture2D resultTex = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(imgMat, resultTex);
       
        debugMarkerDisplay1.texture = resultTex;

        return markerId;
    }
    private ScanInfo PerspectiveTransform(Mat imgMat)
    {
        if (corners.Count == 0)
        {
            Debug.Log("마커감지에러");
            return null;
        }

        // 1. 마커 꼭짓점 가져오기
        MatOfPoint2f markerCorners = new MatOfPoint2f(corners[0]);
        Point[] cornerArray = markerCorners.toArray();
        Point[] topLeftBottomLeftP = ArrayPoints(cornerArray);
        

        // 2. 수평 기준 회전 각도 계산 (topLeft → topRight)
        Point topLeft = topLeftBottomLeftP[0];
        Point bottomLeft = topLeftBottomLeftP[1];

        double dx = bottomLeft.x - topLeft.x;
        double dy = bottomLeft.y - topLeft.y;
        double angle = Mathf.Atan2((float)dy, (float)dx) * Mathf.Rad2Deg;
        if (Mathf.Abs((float)angle) > 45f)
        {
            Debug.LogWarning("이상한 회전각 감지됨. 마커 방향이 잘못 인식됐을 수 있음.");
        }

        // 3. 이미지 회전 (Affine)
        Point center = new Point(imgMat.cols() / 2, imgMat.rows() / 2);
        Mat rotMat = Imgproc.getRotationMatrix2D(center, -angle+90f, 1.0);

        Mat rotatedMat = new Mat();
        Imgproc.warpAffine(imgMat, rotatedMat, rotMat, imgMat.size());
        // 4. 회전된 이미지에서 마커 좌표도 같이 회전시켜야 함 (주의!)
        MatOfPoint2f rotatedMarkerCorners = new MatOfPoint2f();

        Core.transform(markerCorners, rotatedMarkerCorners, rotMat);
        Point[] points = rotatedMarkerCorners.toArray();
        ScanInfo scanInfo = new ScanInfo(rotatedMat, points[0] , 0);


        return scanInfo;
    }

    private Point[] ArrayPoints(Point[] corner)
    {
        Point center = new Point(0, 0);
        foreach (Point p in corner)
        {
            center.x += p.x;
            center.y += p.y;
        }
        center.x /= corner.Length;
        center.y /= corner.Length;

        // 각 꼭짓점의 상대 위치 기준으로 정렬 (시계 방향)
        Point topLeft = null, topRight = null, bottomRight = null, bottomLeft = null;

        foreach (Point p in corner)
        {
            if (p.x < center.x && p.y < center.y) topLeft = p;
            else if (p.x > center.x && p.y < center.y) topRight = p;
            else if (p.x > center.x && p.y > center.y) bottomRight = p;
            else if (p.x < center.x && p.y > center.y) bottomLeft = p;
        }
        Point[] result = new Point[2];
        result[0] = topLeft;
        result[1] = bottomLeft;
        return result;
    }

    public Mat CropFromPerspective(Mat srcImg, MatOfPoint2f markerCorners, int offsetX, int offsetY, int cropSize)
    {
        float expand = 10.0f;
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
        int W = 1680;
        int H = 2360;

        MatOfPoint2f dst = new MatOfPoint2f(
            new Point(0, 0),
            new Point(W, 0),
            new Point(W, H),
            new Point(0, H)
        );

        // 보정 수행
        Mat perspectiveCorrected = new Mat();
        Mat transform = Imgproc.getPerspectiveTransform(src, dst);
        Imgproc.warpPerspective(srcImg, perspectiveCorrected, transform, new Size(W, H));




        Texture2D resultTex = new Texture2D(perspectiveCorrected.cols(), perspectiveCorrected.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(perspectiveCorrected, resultTex);

        debugMarkerDisplay.texture = resultTex;
        //// 5. 마커 기준 offset 위치에서 cropRect 정의
        //int cropX = Mathf.Clamp(offsetX, 0, (int)markerSize - cropSize);
        //int cropY = Mathf.Clamp(offsetY, 0, (int)markerSize - cropSize);
        //OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(offsetX, offsetY, cropSize, cropSize);

        // 6. Crop 수행
        //Mat cropped = new Mat(perspectiveCorrected, cropRect);
        Mat cropped = new Mat();

        return cropped;
    }
}
