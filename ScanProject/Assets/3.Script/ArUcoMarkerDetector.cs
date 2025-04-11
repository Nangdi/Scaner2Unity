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
    public RawImage debugMarkerDisplay; // ī�޶� �Ǵ� �̹��� ��¿�
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
        // 1. �ؽ�ó �� Mat

        Mat imgMat = new Mat(marker.height, marker.width, CvType.CV_8UC3);
        Utils.texture2DToMat(marker, imgMat);

        // 2. Grayscale�� ��ȯ
        Mat grayMat = new Mat();
        Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        // 3. ��ųʸ� ����
        Dictionary dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        // 4. ��Ŀ ����
        corners = new List<Mat>();
        Mat ids = new Mat();
        Aruco.detectMarkers(grayMat, dictionary, corners, ids);
        Mat perspectiveMat = PerspectiveTransform(imgMat);
        // 5. ��� ǥ��
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(imgMat, corners, ids);
            for (int i = 0; i < ids.total(); i++)
            {
                markerId = (int)ids.get(i, 0)[0];
                Debug.Log(" ������ ��Ŀ ID: " + markerId);

            }
        }
        else
        {

            Debug.Log("��Ŀ�νľȵ�");

        }

        // 6. ����� �ؽ�ó�� ǥ��
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
