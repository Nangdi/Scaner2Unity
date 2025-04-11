using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCvSharp.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static OpenCVForUnityExample.ArUcoCreateMarkerExample;

public class ArUcoMarkerDetector : MonoBehaviour
{
    public RawImage debugMarkerDisplay; // 카메라 또는 이미지 출력용
    public List<Mat> corners;
    public Texture2D scannedTex;
    public int markerId;
    void Start()
    {
    }

    public Mat DetectMarker(Texture2D marker)
    {
        debugMarkerDisplay.texture = marker;
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
        Mat perspectiveMat = PerspectiveTransform(imgMat);
        // 5. 결과 표시
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(imgMat, corners, ids);
            for (int i = 0; i < ids.total(); i++)
            {
                markerId = (int)ids.get(i, 0)[0];
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


        return perspectiveMat;
    }
    private Mat PerspectiveTransform(Mat imgMat)
    {
        if (corners.Count == 0) return imgMat;

        MatOfPoint2f markerCorners = new MatOfPoint2f(corners[0]);
        int outputSize = 2048;

        Point[] dstPoints = new Point[]
        {
        new Point(0, 0),
        new Point(outputSize - 1, 0),
        new Point(outputSize - 1, outputSize - 1),
        new Point(0, outputSize - 1)
        };

        Mat transform = Imgproc.getPerspectiveTransform(markerCorners, new MatOfPoint2f(dstPoints));

        Mat result = new Mat();
        Imgproc.warpPerspective(imgMat, result, transform, imgMat.size());

        return result;
    }

    private Mat CorrectPerspective(Mat inputMat, MatOfPoint2f markerCorners, int outputSize = 300)
    {
        Point[] srcPoints = markerCorners.toArray();

        Point[] dstPoints = new Point[]
        {
        new Point(0, 0),
        new Point(outputSize - 1, 0),
        new Point(outputSize - 1, outputSize - 1),
        new Point(0, outputSize - 1)
        };

        MatOfPoint2f srcMat = new MatOfPoint2f(srcPoints);
        MatOfPoint2f dstMat = new MatOfPoint2f(dstPoints);

        Mat transform = Imgproc.getPerspectiveTransform(srcMat, dstMat);
        Mat result = new Mat();
        Imgproc.warpPerspective(inputMat, result, transform, new Size(outputSize, outputSize));

        return result;
    }
}
