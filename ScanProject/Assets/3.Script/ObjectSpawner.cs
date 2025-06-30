using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.UnityUtils;
using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour
{
    public OpenCVForUnity.CoreModule.Mat nameAreaMat;        // �̸��� ���� ����
    public OpenCVForUnity.CoreModule.Mat drawingAreaMat;     // ��ĥ ������ �ִ� ����
    public ObjectScanData currentScanData;

    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject nameObject;
    private GameObject myObject;
    private Renderer renderer;
    ////�ָ� nameTagManager�� ����
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
    public void CreatObject()
    {
        myObject = CustomJsonManager.jsonManager.objectList[currentScanData.objectID];

        GameObject ob = Instantiate(myObject); //내오브젝트 생성
        GameObject nameTagOb = Instantiate(nameObject, canvas.transform); // 내오브젝트에 달릴 이름생성
        nameTagOb.GetComponent<NameTagController>().targetHeadPos = ob.transform.GetChild(0); //이름표가 따라다닐 타겟
        Texture2D resultTex = new Texture2D(drawingAreaMat.cols(), drawingAreaMat.rows(), TextureFormat.RGBA32 , false); // 오브젝트에 입힐 texture
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(drawingAreaMat, resultTex); //Mat -> texture변환
        if (ob != null)
        {
            renderer = ob.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = resultTex;
            }
        }
    }
    public void InitUpdate()
    {
        
    }
}
