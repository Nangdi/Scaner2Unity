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
    public RawImage debugMarkerDisplay; // ī�޶� �Ǵ� �̹��� ��¿�
    public RawImage debugMarkerDisplay1; // ī�޶� �Ǵ� �̹��� ��¿�
    public List<Mat> corners;
    public Texture2D scannedTex;
    public int markerId; 
    public Mat perspectiveMat; // ������� ������ Mat
    public Point standardOffset { get; private set; } //������ crop ���� offset
    void Start()
    {
    }

    public int DetectMarker(Texture2D marker)
    {
       
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
        //ScanInfo persepectiveInfo = new ScanInfo(marker, null, 0);
        //perspectiveMat = persepectiveInfo.correctMat;
        //standardOffset = persepectiveInfo.standardOffset;
        // 5. ��� ǥ��
        if (ids.total() > 0)
        {
            Aruco.drawDetectedMarkers(imgMat, corners, ids);
            for (int i = 0; i < ids.total(); i++)
            {
                markerId = (int)ids.get(i, 0)[0];
                //persepectiveInfo.markerId = markerId;
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
       
        debugMarkerDisplay1.texture = resultTex;

        return markerId;
    }
    private ScanInfo PerspectiveTransform(Mat imgMat)
    {
        if (corners.Count == 0)
        {
            Debug.Log("��Ŀ��������");
            return null;
        }

        // 1. ��Ŀ ������ ��������
        MatOfPoint2f markerCorners = new MatOfPoint2f(corners[0]);
        Point[] cornerArray = markerCorners.toArray();
        Point[] topLeftBottomLeftP = ArrayPoints(cornerArray);
        

        // 2. ���� ���� ȸ�� ���� ��� (topLeft �� topRight)
        Point topLeft = topLeftBottomLeftP[0];
        Point bottomLeft = topLeftBottomLeftP[1];

        double dx = bottomLeft.x - topLeft.x;
        double dy = bottomLeft.y - topLeft.y;
        double angle = Mathf.Atan2((float)dy, (float)dx) * Mathf.Rad2Deg;
        if (Mathf.Abs((float)angle) > 45f)
        {
            Debug.LogWarning("�̻��� ȸ���� ������. ��Ŀ ������ �߸� �νĵ��� �� ����.");
        }

        // 3. �̹��� ȸ�� (Affine)
        Point center = new Point(imgMat.cols() / 2, imgMat.rows() / 2);
        Mat rotMat = Imgproc.getRotationMatrix2D(center, -angle+90f, 1.0);

        Mat rotatedMat = new Mat();
        Imgproc.warpAffine(imgMat, rotatedMat, rotMat, imgMat.size());
        // 4. ȸ���� �̹������� ��Ŀ ��ǥ�� ���� ȸ�����Ѿ� �� (����!)
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

        // �� �������� ��� ��ġ �������� ���� (�ð� ����)
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

        // ���� ���� ���
        Point rightVec = new Point(topRight.x - topLeft.x, topRight.y - topLeft.y);
        Point downVec = new Point(bottomLeft.x - topLeft.x, bottomLeft.y - topLeft.y);

        // Ȯ��� �簢�� ������ ����
        MatOfPoint2f src = new MatOfPoint2f(
            topLeft,
            new Point(topLeft.x + rightVec.x * expand, topLeft.y + rightVec.y * expand),
            new Point(topLeft.x + rightVec.x * expand + downVec.x * expand, topLeft.y + rightVec.y * expand + downVec.y * expand),
            new Point(topLeft.x + downVec.x * expand, topLeft.y + downVec.y * expand)
        );

        // ��ǥ �簢�� ũ��
        int W = 1680;
        int H = 2360;

        MatOfPoint2f dst = new MatOfPoint2f(
            new Point(0, 0),
            new Point(W, 0),
            new Point(W, H),
            new Point(0, H)
        );

        // ���� ����
        Mat perspectiveCorrected = new Mat();
        Mat transform = Imgproc.getPerspectiveTransform(src, dst);
        Imgproc.warpPerspective(srcImg, perspectiveCorrected, transform, new Size(W, H));




        Texture2D resultTex = new Texture2D(perspectiveCorrected.cols(), perspectiveCorrected.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(perspectiveCorrected, resultTex);

        debugMarkerDisplay.texture = resultTex;
        //// 5. ��Ŀ ���� offset ��ġ���� cropRect ����
        //int cropX = Mathf.Clamp(offsetX, 0, (int)markerSize - cropSize);
        //int cropY = Mathf.Clamp(offsetY, 0, (int)markerSize - cropSize);
        //OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(offsetX, offsetY, cropSize, cropSize);

        // 6. Crop ����
        //Mat cropped = new Mat(perspectiveCorrected, cropRect);
        Mat cropped = new Mat();

        return cropped;
    }
}
