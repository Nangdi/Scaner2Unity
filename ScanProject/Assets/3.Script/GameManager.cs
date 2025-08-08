using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingData
{
   
    public int motionRatio = 50;
    public Vector2 WaitTimeMinMax = new Vector2(2, 4);
    public int LifeTime = 600;
    public int correctionTime= 60;
    public int basedFieldCount = 5;
}

public class GameManager : MonoBehaviour
{
    /*
    GameSettingData
    ���ִ� ��� �ð� �ּ� �ִ�
    �ȱ� : ��� Ȯ��
    LifeTime �����ð� 10��
    ������Ʈ ���������� LifeTime ������
    ������ ������Ʈ����

    ����������
    �ʵ� ������Ʈ����
    
    */
    public static GameManager instance;

    [SerializeField]
    private RawImage uvFrame;
    [SerializeField]
    private Texture2D[] uvImages;
    public GameSettingData gameSettingData;

    [Header("�ΰ���������")]
    public float HueShift =14;
    public float Saturation =32;
    public float Brightness =1;
    public float Contrast =0;
       

    public int ObjectCount = 0;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        gameSettingData = CustomJsonManager.jsonManager.LoadGameSettingData();
    }
  
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            uvFrame.texture = uvImages[0];
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            uvFrame.texture = uvImages[1];
        }
    }
}
