using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static OpenCVForUnityExample.ArUcoCreateMarkerExample;

public class ArUcoMarkerDetector : MonoBehaviour
{
    public RawImage debugMarkerDisplay; // ī�޶� �Ǵ� �̹��� ��¿�
    int markerId;
    void Start()
    {
    }

    public int DetectMarker(Texture2D marker)
    {
        debugMarkerDisplay.texture = marker;
        // 1. �ؽ�ó �� Mat
        Mat imgMat = new Mat(marker.height, marker.width, CvType.CV_8UC3);
        Utils.texture2DToMat(marker, imgMat);

        // 2. Grayscale�� ��ȯ
        Mat grayMat = new Mat();
        Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        // 3. ��ųʸ� ����
        Dictionary dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        // 4. ��Ŀ ����
        List<Mat> corners = new List<Mat>();
        Mat ids = new Mat();
        Aruco.detectMarkers(grayMat, dictionary, corners, ids);
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
       
       
        return markerId;
    }

}
