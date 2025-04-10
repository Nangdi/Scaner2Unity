using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.UnityUtils;
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
    private ScanDataManager ScanDataManager;
    private ArUcoMarkerDetector markerDetector;
    public GameObject lightningPrefab;
    public GameObject boxPrefab;

    public RawImage debugFrameDisplay;
    public int detectRange = 400;

    Renderer renderer;
    Texture2D resultTex;

    private void Start()
    {
        TryGetComponent(out ScanDataManager);
        TryGetComponent(out markerDetector);
    }

    public void ProcessAnalysis(Texture2D scannedTexture)
    {
      
        Mat imageMat = new Mat(scannedTexture.height, scannedTexture.width, CvType.CV_8UC3);
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTexture, imageMat);

        ////마커영역만 추출후 ID 받기
        //Texture2D markerImage = ExtractMarker(imageMat);

        int markerID = markerDetector.DetectMarker(scannedTexture);
        double[] ptArr = markerDetector.corners[0].get(0, 0); // index 0 = 좌상단
        Point markerPoint = new Point(ptArr[0], ptArr[1]);

        UnityEngine.Debug.Log("Detected markerID: " + markerID);

        int boxSize = 1509; // 18cm * 100px
        int centerX = imageMat.cols() / 2;
        int centerY = imageMat.rows() / 2;


        int startX = (int)markerPoint.x + 0;
        int startY = (int)markerPoint.y + 396;
        //int startX = imageMat.cols() / 2 - boxSize / 2 + 13;
        //int startY = imageMat.rows() / 2 - boxSize / 2 + -47;
        // 좌표가 음수가 되지 않도록 제한
        //startX = Mathf.Max(startX, 0);
        //startY = Mathf.Max(startY, 0);

        // 잘라낼 범위 설정
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(startX, startY, boxSize, boxSize);
        Mat drawingMat = new Mat(imageMat, cropRect);

        //외각선(검정색) 제거하는 코드 추가할예정
        //Scalar lower = new Scalar(0, 0, 0);       // 완전 검정
        //Scalar upper = new Scalar(30, 30, 30);    // 어두운 회색까지 포함
        //Mat mask = new Mat();
        //Core.inRange(drawingMat, lower, upper, mask); // → 외곽선 위치만 흰색(255)

        //// ✅ 2. 여기에서 마스크를 팽창시킵니다 (얇은 선도 굵게 감지)
        //Mat kernel = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(5, 5));
        //Imgproc.dilate(mask, mask, kernel); // 요기!


        //// 2. 주변 픽셀로 외곽선을 지우고 채우기
        //Mat inpainted = new Mat();
        //Photo.inpaint(drawingMat, mask, inpainted, 5, Photo.INPAINT_TELEA); // 반경 3, 주변 색 보간

        //// 3. 마무리 블러로 부드럽게 연결
        //Imgproc.GaussianBlur(inpainted, inpainted, new Size(3, 3), 0);

        // 텍스처화
        resultTex = new Texture2D(drawingMat.cols(), drawingMat.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(drawingMat, resultTex);
        Imgproc.resize(drawingMat, drawingMat, new Size(2048, 2048)); //  해상도만 맞춤
        debugFrameDisplay.texture = resultTex;

        // 4. 오브젝트 선택 후 머티리얼 적용
        GameObject go = null;
        if (markerID == 1) go = Instantiate(lightningPrefab);
        else if (markerID == 2) go = Instantiate(boxPrefab);
        else UnityEngine.Debug.LogWarning("Unknown shape, no object spawned.");

        if (go != null)
        {
            renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = resultTex;
            }
        }
    }

    private Texture2D ExtractMarker(Mat imageMat)
    {

        // 2. 상단 마커 인식 (0,0) 기준 100x100
        OpenCVForUnity.CoreModule.Rect shapeRect = new OpenCVForUnity.CoreModule.Rect(0, 0, detectRange, detectRange);
        Mat shapeROI = new Mat(imageMat, shapeRect);

        Texture2D marker = new Texture2D(shapeROI.cols(), shapeROI.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(shapeROI, marker);
        return marker;
    }
    private void SetPosition()
    {
        renderer.material.mainTexture = resultTex;
    }
}
