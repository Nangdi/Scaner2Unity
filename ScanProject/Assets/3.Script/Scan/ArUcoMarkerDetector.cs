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
using System.Threading.Tasks;
using static OpenCVForUnityExample.ArUcoCreateMarkerExample;
public class DetectInfo
{
    public Mat scanMat;
    public Mat markerCorner;
    public int markerId;

    public DetectInfo(Mat mat, Mat corner, int id)
    {
        scanMat = mat;
        markerCorner = corner;
        markerId = id;
    }
};
public class ArUcoMarkerDetector : MonoBehaviour
{
    public RawImage ScanImage; // 왜곡보정이미지
    public RawImage correctionImage; // 
    public List<Mat> corners;
    public Texture2D scannedTex;
    public int markerId; 
    public Mat perspectiveMat; // 수평수직 보정후 Mat
    public Point standardOffset { get; private set; } //보정후 crop 기준 offset
    Mat correctedMat = new Mat();
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
        Mat corner = new Mat();

        int markerId = int.MaxValue;

        Aruco.detectMarkers(grayMat, dictionary, corners, ids);
        //마커 4개인식못할시 리턴
        if (ids.total() < 4)
        {
            //Debug.LogWarning("마커가 4개 미만입니다.");
            return null;
        }

        // 4. 마커가 감지되었을 경우 처리
        if (ids.total() > 0)
        {
           
            Aruco.drawDetectedMarkers(scanMat, corners, ids);
            for(int i = 0; i < ids.total(); i++)
{
                int id = (int)ids.get(i, 0)[0];
                if (id < markerId)
                {
                    markerId = id;
                    corner = corners[i];
                   //id2번에 해당하는 corner를 찾아야함


                }
            }
            //Debug.Log("감지된 마커 ID: " + markerId);
        }
        else
        {
            //Debug.Log("마커 인식 안됨");
        }
        // 4. 각 마커의 중심점 구하기
        List<Point> centerPoints = new List<Point>();
        for (int i = 0; i < 4; i++)
        {
            float[] data = new float[8];
            corners[i].get(0, 0, data);

            double sumX = 0, sumY = 0;
            for (int j = 0; j < 4; j++)
            {
                sumX += data[j * 2];
                sumY += data[j * 2 + 1];
            }

            centerPoints.Add(new Point(sumX / 4.0, sumY / 4.0));
        }

        // 5. 시계방향 정렬 (좌상 → 우상 → 우하 → 좌하)
        List<Point> ordered = OrderPointsClockwise(centerPoints);
        CalculateMarkerQuadAspectRatio(ordered[0], ordered[1], ordered[2], ordered[3]);
        // 6. 보정 대상 좌표 정의 (A4 비율)
        int width = 1980;
        int height = 1120;
        MatOfPoint2f src = new MatOfPoint2f(ordered.ToArray());
        MatOfPoint2f dst = new MatOfPoint2f(
            new Point(0, 0),
            new Point(width, 0),
            new Point(width, height),
            new Point(0, height)
        );

        // 7. Homography로 왜곡 보정
      
        Mat transform = Imgproc.getPerspectiveTransform(src, dst);
        Imgproc.warpPerspective(scanMat, correctedMat, transform, new Size(width, height));

        // 8. 디버깅용 표시
        //Texture2D debugTex = new Texture2D(correctedMat.cols(), correctedMat.rows(), TextureFormat.RGBA32, false);
        //Utils.matToTexture2D(correctedMat, debugTex);
        //correctionImage.texture = debugTex;

        // 9. DetectInfo 생성 및 반환
        return new DetectInfo(correctedMat, corner, markerId);
    }
   

   
    public  MatOfPoint2f GetA4DestCorners(int width, int height)
    {
        return new MatOfPoint2f(
            new Point(0, 0),
            new Point(width, 0),
            new Point(width, height),
            new Point(0, height)
        );
    }

    public Point[] GetMarkerPoints(Mat cornerMat)
    {
        double[] cornerData = new double[8]; // x1, y1, x2, y2, x3, y3, x4, y4
        cornerMat.get(0, 0, cornerData);

        Point[] pts = new Point[4];
        for (int i = 0; i < 4; i++)
        {
            pts[i] = new Point(cornerData[i * 2], cornerData[i * 2 + 1]);
        }

        return pts;
    }

    //시계방향정렬
    public List<Point> OrderPointsClockwise(List<Point> pts)
    {
        Point center = new Point(0, 0);
        foreach (var p in pts) { center.x += p.x; center.y += p.y; }
        center.x /= pts.Count;
        center.y /= pts.Count;

        pts.Sort((a, b) => {
            double angleA = Mathf.Atan2((float)(a.y - center.y), (float)(a.x - center.x));
            double angleB = Mathf.Atan2((float)(b.y - center.y), (float)(b.x - center.x));
            return angleA.CompareTo(angleB);
        });

        return pts;
    }
    public float CalculateMarkerQuadAspectRatio(Point topLeft, Point topRight, Point bottomRight, Point bottomLeft)
    {
        // 상단, 하단 길이 (가로)
        double widthTop = Mathf.Sqrt(Mathf.Pow((float)(topRight.x - topLeft.x), 2) + Mathf.Pow((float)(topRight.y - topLeft.y), 2));
        double widthBottom = Mathf.Sqrt(Mathf.Pow((float)(bottomRight.x - bottomLeft.x), 2) + Mathf.Pow((float)(bottomRight.y - bottomLeft.y), 2));
        double avgWidth = (widthTop + widthBottom) / 2.0;

        // 좌측, 우측 길이 (세로)
        double heightLeft = Mathf.Sqrt(Mathf.Pow((float)(bottomLeft.x - topLeft.x), 2) + Mathf.Pow((float)(bottomLeft.y - topLeft.y), 2));
        double heightRight = Mathf.Sqrt(Mathf.Pow((float)(bottomRight.x - topRight.x), 2) + Mathf.Pow((float)(bottomRight.y - topRight.y), 2));
        double avgHeight = (heightLeft + heightRight) / 2.0;

        // 비율 계산
        float aspectRatio = (float)(avgWidth / avgHeight);
        Debug.Log($"마커 사각형 비율 (가로:세로) = {avgWidth} :{avgHeight} ");
        return aspectRatio;
    }

    public async Task<DetectInfo> StartDetectAsync(Mat scanMat)
    {
        DetectInfo result = await Task.Run(() => GetDetectInfo(scanMat));

        // Unity 메인 스레드 처리
        Texture2D debugTex = new Texture2D(correctedMat.cols(), correctedMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(correctedMat, debugTex);
        correctionImage.texture = debugTex;

        // 최종 DetectInfo 만들기
        //DetectInfo info = new DetectInfo(correctedMat, result.markerCorner, result.markerId);
        return result;
    }
}
