using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.UnityUtils;
using System;
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
    //OffsetTurner에게 상속


    [SerializeField]
    protected ArUcoMarkerDetector arucoMarkerDetector;

    public ObjectSpawner objectSpawner;

    public GameObject go;
    private ObjectScanData obScanData;
    public RawImage debugImage;
    Renderer renderer;
    Texture2D resultTex;
    protected Mat inputImage;
    protected Mat nameImage;
    public DetectInfo detectInfo;
    protected Point startPoint;
    public bool isOutlineEnabled = false;
    public bool isCropName;
    [Header("SettingVelue")]
    public int lowerValue =0; 
    public int UpperValue =50; 
    public int kernelValue =4; 
    public int LerpValue =3;
    private void Start()
    {
    }

   
    public virtual void ProcessAnalysis(Texture2D scannedTexture)
    {


        //Tex ->Mat 변환
        Mat imageMat = new Mat(scannedTexture.height, scannedTexture.width, CvType.CV_8UC3).clone();
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTexture, imageMat);
        nameImage = imageMat.clone();
        Core.flip(nameImage, nameImage, 0); // X축 기준 좌우 반전
        //scannedTexture Marker 정보 가져오기
        detectInfo = arucoMarkerDetector.GetDetectInfo(imageMat);
        imageMat = ExpandCropedMat(detectInfo.scanMat, 1.5f);
        Core.flip(imageMat, imageMat, 0); // X축 기준 좌우 반전
        Texture2D debugTex = new Texture2D(imageMat.cols(), imageMat.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(imageMat, debugTex);

        arucoMarkerDetector.ScanImage.texture = debugTex;
        inputImage = imageMat;

        //Point markerPoint = scanInfo.standardOffset;

        obScanData = CustomJsonManager.jsonManager.dataList[detectInfo.markerId];
        ObjectScanData nameInfoData = CustomJsonManager.jsonManager.dataList[3];
        UnityEngine.Debug.Log("Detected markerID: " + detectInfo.markerId);

        MatOfPoint2f markerCorners = new MatOfPoint2f(detectInfo.markerCorner);



        Mat cropped = CroppedImage(markerCorners, obScanData, false);
        Mat nameTagMat = CroppedImage(markerCorners, nameInfoData, true);

        objectSpawner.drawingAreaMat = cropped;
        objectSpawner.nameAreaMat = nameTagMat;
        objectSpawner.currentScanData = obScanData;

        go= objectSpawner.CreatObject();

        ////애를 nameTagManager로 전달
        //if (isCropName)
        //{
        //    ApplyTexture(nameTagMat, debugImage);
        //}
        //else
        //{
        //    if (!isOutlineEnabled)
        //    {
        //        cropped = RemoveOutline(cropped);
        //    }
        //ApplyTexture(cropped, debugImage);
        //}


        //Texture2D cropTex = new Texture2D(cropped.cols(), cropped.rows(), TextureFormat.RGBA32, false);
        //Texture2D nameTagTex = new Texture2D(nameTagMat.cols(), nameTagMat.rows(), TextureFormat.RGBA32, false);
        //OpenCVForUnity.UnityUtils.Utils.matToTexture2D(nameTagMat, nameTagTex);
        //nameTagManager.test.texture = nameTagTex;

        // 텍스처화
    }
    public IEnumerator ProcessAnalysisCoroutine(Texture2D scannedTexture)
    {
        // Step 1: Tex -> Mat
        Mat imageMat = new Mat(scannedTexture.height, scannedTexture.width, CvType.CV_8UC3).clone();
        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(scannedTexture, imageMat);
        yield return null; // 한 프레임 쉬기

        nameImage = imageMat.clone();
        Core.flip(nameImage, nameImage, 0);
        detectInfo = arucoMarkerDetector.GetDetectInfo(imageMat);
        yield return null;

        imageMat = ExpandCropedMat(detectInfo.scanMat, 1.5f);
        Core.flip(imageMat, imageMat, 0);
        Texture2D debugTex = new Texture2D(imageMat.cols(), imageMat.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(imageMat, debugTex);
        yield return null;

        arucoMarkerDetector.ScanImage.texture = debugTex;
        inputImage = imageMat;

        obScanData = CustomJsonManager.jsonManager.dataList[detectInfo.markerId];
        ObjectScanData nameInfoData = CustomJsonManager.jsonManager.dataList[3];
        UnityEngine.Debug.Log("Detected markerID: " + detectInfo.markerId);

        MatOfPoint2f markerCorners = new MatOfPoint2f(detectInfo.markerCorner);
        yield return null;

        Mat cropped = CroppedImage(markerCorners, obScanData, false);
        Mat nameTagMat = CroppedImage(markerCorners, nameInfoData, true);

        yield return null;

        objectSpawner.drawingAreaMat = cropped;
        objectSpawner.nameAreaMat = nameTagMat;
        objectSpawner.currentScanData = obScanData;

        go = objectSpawner.CreatObject();
    }
    public Mat CroppedImage(MatOfPoint2f markerCorners , ObjectScanData data,bool isName)
    {
        Point[] points = markerCorners.toArray();
        startPoint = points[0]; // ← 바로 이게 좌측 상단

        int boxSize = data.cropSize;
        int startX = (int)Math.Round(startPoint.x) + data.offsetX;
        int startY = (int)Math.Round(startPoint.y) + data.offsetY;

        //crop하는기능
        float cropY = boxSize / data.hRatio;
        OpenCVForUnity.CoreModule.Rect cropRect = new OpenCVForUnity.CoreModule.Rect(startX, startY, boxSize, (int)cropY);
        Mat cropped;
        if (isName)
        {
            cropped = new Mat(nameImage, cropRect).clone();
        }
        else
        {
            cropped = new Mat(inputImage, cropRect).clone();
        }
        Core.flip(cropped, cropped, 0); // X축 기준 좌우 반전
        return cropped;

    }
    public void ApplyTexture(Mat mat ,RawImage debugDisplay = null)
    {
        resultTex = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(mat, resultTex);
        //OpenCVForUnity.UnityUtils.Utils.matToTexture2D(mat, resultTex);
        //Imgproc.resize(mat, mat, new Size(1024 , 1024)); //  해상도만 맞춤
        if(debugDisplay != null)
        {
            debugDisplay.texture = resultTex;

        }

        // 4. 오브젝트 선택 후 머티리얼 적용
        GameObject go = CustomJsonManager.jsonManager.objectList[obScanData.objectID];

        GameObject ob = Instantiate(go);
        if (ob != null)
        {
            renderer = ob.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = resultTex;
            }
        }
    }
    public Mat RemoveOutline(Mat cropped)
    {
        UnityEngine.Debug.Log("Remove호출");
        //외각선(검정색) 제거하는 코드 추가할예정
        Scalar lower = new Scalar(lowerValue, lowerValue, lowerValue);       // 완전 검정
        Scalar upper = new Scalar(UpperValue, UpperValue, UpperValue);    // 어두운 회색까지 포함
        Mat mask = new Mat();
        Core.inRange(cropped, lower, upper, mask); // → 외곽선 위치만 흰색(255)

        // ✅ 2. 여기에서 마스크를 팽창시킵니다 (얇은 선도 굵게 감지)
        Mat kernel = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(kernelValue, kernelValue));
        Imgproc.dilate(mask, mask, kernel); // 요기!


        // 2. 주변 픽셀로 외곽선을 지우고 채우기
        Mat inpainted = new Mat();
        Photo.inpaint(cropped, mask, inpainted, LerpValue, Photo.INPAINT_TELEA); // 반경 3, 주변 색 보간

        // 3. 마무리 블러로 부드럽게 연결
        Imgproc.GaussianBlur(inpainted, inpainted, new Size(3, 3), 0);

        return inpainted;
    }
    public static Mat ExpandCropedMat(Mat croppedMat, float expandRatio)
    {
        // 1. 기존 crop된 이미지 크기
        int originalW = croppedMat.cols();
        int originalH = croppedMat.rows();

        // 2. 확장할 canvas 크기
        int expandedW = (int)(originalW * expandRatio);
        int expandedH = (int)(originalH * expandRatio);

        // 3. 흰색 배경으로 새 Mat 생성 (3채널)
        Mat expandedMat = new Mat(expandedH, expandedW, croppedMat.type(), new Scalar(255, 255, 255));

        // 4. 가운데 위치 계산
        int offsetX = (expandedW - originalW) / 2;
        int offsetY = (expandedH - originalH) / 2;

        // 5. ROI 영역에 원본 Mat 붙이기
        OpenCVForUnity.CoreModule.Rect roi = new OpenCVForUnity.CoreModule.Rect(offsetX, offsetY, originalW, originalH);
        croppedMat.copyTo(new Mat(expandedMat, roi));



        //// 6. Texture2D로 변환
        //Texture2D resultTex = new Texture2D(expandedW, expandedH, TextureFormat.RGBA32, false);
        //OpenCVForUnity.UnityUtils.Utils.matToTexture2D(expandedMat, resultTex);

        return expandedMat;
    }
}
