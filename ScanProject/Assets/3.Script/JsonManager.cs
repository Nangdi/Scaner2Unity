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
}
[Serializable]
class DataWrapper
{
    public List<ObjectScanData> objectDatas;
}

public class JsonManager : MonoBehaviour
{
    public static JsonManager jsonManager { get; private set; }

    private string filePath;

    [Header("공유데이터")]
    public List<ObjectScanData> dataList;
    public List<GameObject> objectList;
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
        dataList = LoadData();
    }
    private void Start()
    {

       
    }
    public void DataInit()
    {
        dataList = new List<ObjectScanData>()
        {
            new ObjectScanData { objectID = 0, cropSize = 1509, startOffset = 0, offsetX = 0, offsetY = 300 },
            new ObjectScanData { objectID = 1, cropSize = 1550, startOffset = 0, offsetX = 0, offsetY = 300 },
            new ObjectScanData { objectID = 2, cropSize = 1550, startOffset = 0, offsetX = 0, offsetY = 300 }
        };
    }
    public void SaveData()
    {
        DataWrapper wrapper = new DataWrapper { objectDatas = dataList };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"저장됨: {filePath}");
    }
    public List<ObjectScanData> LoadData()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("JSON 파일이 존재하지 않습니다.");
            SaveData();
        }

        string json = File.ReadAllText(filePath);
        DataWrapper wrapper = JsonUtility.FromJson<DataWrapper>(json);
        return wrapper.objectDatas;
    }
}
