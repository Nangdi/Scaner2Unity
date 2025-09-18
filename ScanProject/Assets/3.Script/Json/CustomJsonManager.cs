using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class ObjectScanData
{
    public int objectID;
    public int cropSize;
    public int startOffset;
    public int offsetX;
    public int offsetY;
    public float hRatio;
}
[Serializable]
public class PortJson
{
    public string com = "COM4";
    public int baudLate = 19200;
}
[Serializable]
public class DataWrapper
{
    public List<ObjectScanData> objectDatas;
}

public class CustomJsonManager : JsonManager
{
    // Start is called before the first frame update
    public static CustomJsonManager jsonManager { get; private set; }

   
    private string filePath;
    private string portPath;
    private string gameDataPath;

    [Header("공유데이터")]
    public List<ObjectScanData> dataList;
    public List<GameObject> objectList;
    public GameSettingData settingData;
    // Update is called once per frame
    private void Awake()
    {
        if (jsonManager == null)
        {
            jsonManager = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
        DataInit();
        filePath = Path.Combine(Application.persistentDataPath, "objectData.json");
        Debug.Log(filePath);
        portPath = Path.Combine(Application.streamingAssetsPath, "port.json");
        gameDataPath = Path.Combine(Application.streamingAssetsPath, "GameSettingData.json");
        //SaveScanData();
        dataList = LoadData();
        settingData = LoadGameSettingData();
    }



    public void DataInit()
    {
        dataList = new List<ObjectScanData>()
        {
            new ObjectScanData { objectID = 0, cropSize = 1975, startOffset = 0, offsetX = 0, offsetY = 0, hRatio =1f},
            new ObjectScanData { objectID = 1, cropSize = 1399, startOffset = 0, offsetX = 816, offsetY = 420, hRatio =1f},
            new ObjectScanData { objectID = 2, cropSize = 1394, startOffset = 0, offsetX = 840, offsetY = 379, hRatio =1f},
            new ObjectScanData { objectID = 3, cropSize = 464, startOffset = 0, offsetX = 2038, offsetY = 550, hRatio = 2.85f }
        };

    }
    //저장할 json 객체 , 경로설정
   
    public void SaveScanData()
    {
        DataWrapper wrapper = new DataWrapper { objectDatas = dataList };
        SaveData(wrapper, filePath);
    }
    public void SavePortJson()
    {
        PortJson portJson = new PortJson();
        SaveData(portJson, portPath);
    }
    public void SaveGameSettingData()
    {
        GameSettingData dataJson = new GameSettingData();
        SaveData(dataJson, gameDataPath);
    }
    public void SaveScreenToneData()
    {

    }
    public List<ObjectScanData> LoadData()
    {
        DataWrapper wrapper = LoadData<DataWrapper>(filePath, SaveScanData);
        return wrapper.objectDatas;
    }
    public PortJson LoadPortData()
    {
        return LoadData<PortJson>(portPath, SavePortJson);
    }
    public GameSettingData LoadGameSettingData()
    {
        return LoadData<GameSettingData>(gameDataPath, SaveGameSettingData);
    }
}
