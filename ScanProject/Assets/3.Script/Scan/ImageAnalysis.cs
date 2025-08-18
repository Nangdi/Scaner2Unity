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
using System.Threading.Tasks;

public class ImageAnalysis : MonoBehaviour
{
    //OffsetTurner에게 상속

    [Header("Dependencies")]
    [SerializeField]
    protected ArUcoMarkerDetector arucoMarkerDetector;
    public ObjectSpawner objectSpawner;
    [Header("Debug UI")]
    public RawImage debugImage;

    [Header("Outline Removal Settings")]
    public int lowerValue =0; 
    public int UpperValue =50; 
    public int kernelValue =4; 
    public int LerpValue =3;
    [Header("Options")]
    public bool isOutlineEnabled = false;
    public bool isCropName;
    public GameObject go;
    
    private ObjectScanData obScanData;
    Texture2D resultTex;
    protected Mat inputImage;
    protected Mat nameImage;
    [SerializeField]
    public DetectInfo detectInfo;
    protected Point startPoint;

    public async void ProcessAnalysis(Mat scannedMat)
    {
        Mat imageMat = scannedMat;

        await Task.Run(() => OpenCVMatoud(imageMat));
        MainThreadDispatcher.Enqueue(() => objectSpawner.EffectSpawn());
 
    }
    public Mat CroppedImage(MatOfPoint2f markerCorners , ObjectScanData data,bool useNameImage)
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
        if (useNameImage)
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


        return expandedMat;
    }
    public void OpenCVMatoud(Mat imageMat)
    {
        nameImage = imageMat.clone();
        //nameImage = FlipY(imageMat.clone());
        Core.flip(nameImage, nameImage, 0); // X축 기준  반전
        //scannedTexture Marker 정보 가져오기

        detectInfo = arucoMarkerDetector.GetDetectInfo(imageMat);
        imageMat = ExpandCropedMat(detectInfo.scanMat, 1.5f);
        Core.flip(imageMat, imageMat, 0); // X축 기준 반전

        inputImage = imageMat;

      

        obScanData = CustomJsonManager.jsonManager.dataList[detectInfo.markerId];
        ObjectScanData nameInfoData = CustomJsonManager.jsonManager.dataList[3];
        

        MatOfPoint2f markerCorners = new MatOfPoint2f(detectInfo.markerCorner);



        Mat cropped = CroppedImage(markerCorners, obScanData, false);
        Mat nameTagMat = CroppedImage(markerCorners, nameInfoData, true);
        if (!isOutlineEnabled)
        {
            cropped = RemoveOutline(cropped);
        }
        objectSpawner.drawingAreaMat = cropped;
        objectSpawner.nameAreaMat = nameTagMat;
        objectSpawner.currentScanData = obScanData;

      
    }
     private static Mat FlipY(Mat mat)
    {
        Core.flip(mat, mat, 0);
        return mat;
    }
}
