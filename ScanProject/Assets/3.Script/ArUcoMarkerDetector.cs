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
    public RawImage debugMarkerDisplay; // 카메라 또는 이미지 출력용
    int markerId;
    void Start()
    {
    }

    public int DetectMarker(Texture2D marker)
    {
        debugMarkerDisplay.texture = marker;
        // 1. 텍스처 → Mat
        Mat imgMat = new Mat(marker.height, marker.width, CvType.CV_8UC3);
        Utils.texture2DToMat(marker, imgMat);

        // 2. Grayscale로 변환
        Mat grayMat = new Mat();
        Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);

        // 3. 딕셔너리 정의
        Dictionary dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        // 4. 마커 검출
        List<Mat> corners = new List<Mat>();
        Mat ids = new Mat();
        Aruco.detectMarkers(grayMat, dictionary, corners, ids);
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
       
       
        return markerId;
    }

}
