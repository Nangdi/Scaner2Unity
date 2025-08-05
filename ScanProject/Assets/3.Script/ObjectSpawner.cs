using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.UnityUtils;
//using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour
{
    public OpenCVForUnity.CoreModule.Mat nameAreaMat;        // �̸��� ���� ����
    public OpenCVForUnity.CoreModule.Mat drawingAreaMat;     // ��ĥ ������ �ִ� ����
    public ObjectScanData currentScanData;
    [SerializeField]
    private FollowTarget followTarget;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject nameObject;
    private GameObject myObject;
    private Renderer renderer;
    public BoxCollider col;
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
    public GameObject CreatObject()
    {
        myObject = CustomJsonManager.jsonManager.objectList[currentScanData.objectID];
        GetSpawnPos(myObject);
       
        GameObject ob = Instantiate(myObject, GetSpawnPos(myObject) , Quaternion.identity); //내오브젝트 생성
        GameObject nameTagOb = Instantiate(nameObject, canvas.transform); // 내오브젝트에 달릴 이름생성
        nameTagOb.GetComponent<NameTagController>().targetHeadPos = ob.GetComponent<ObjectController>().namePos; //이름표가 따라다닐 타겟
        followTarget.target = ob.transform;
        Texture2D modelTexture = mat2Text(drawingAreaMat);
        Texture2D nameTexture = mat2Text(nameAreaMat);

        Text2Model(modelTexture, nameTexture, ob, nameTagOb);
        return ob;


    }
    public Texture2D mat2Text(OpenCVForUnity.CoreModule.Mat mat)
    {
        Texture2D resultTex = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false); // 오브젝트에 입힐 texture
        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(mat, resultTex); //Mat -> texture변환

        return resultTex;
    }
    public void Text2Model(Texture2D drawing , Texture2D name , GameObject modelOb , GameObject nameOb )
    {
        nameOb.GetComponent<RawImage>().texture = name;
        {
            renderer = modelOb.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = drawing;
            }
        }
    }
    private Vector3 GetSpawnPos(GameObject ob)
    {
       ObjectController obCon =  ob.GetComponent<ObjectController>();
        Bounds bounds = col.bounds;
        float randX = Random.Range(bounds.min.x, bounds.max.x);
        float randZ = Random.Range(bounds.min.z, bounds.max.z);
        float randY = transform.position.y;
        if (obCon.type == ObjectType.Fariy)
        {
            randY = Random.Range(bounds.min.y, bounds.max.y);
        }
       Vector3 targetPosition = new Vector3(randX, randY, randZ);

        return targetPosition;
    }
}
