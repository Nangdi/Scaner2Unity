using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class Example
{

}

public class JsonManager : MonoBehaviour

{
    string portPath;
    //저장할 json 객체 , 경로설정
    public static void SaveData<T>(T jsonObject, string path)
    {
        
        string json = JsonUtility.ToJson(jsonObject, true);
        File.WriteAllText(path, json);
        Debug.Log($"저장됨: {path}");
    }
    //반환값 설정
    public static T LoadData<T>( string path, Action saveMathod )
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("JSON 파일이 존재하지 않습니다.");
            saveMathod();
        }
        Debug.Log("JSON로드");
        string json = File.ReadAllText(path);
        T jsonData = JsonUtility.FromJson<T>(json);
        return jsonData;
    }
    public static T LoadData1<T>(string path , T data ) 
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("JSON 파일이 존재하지 않습니다.");
            SaveData(data ,path);
        }
        Debug.Log("JSON로드");
        string json = File.ReadAllText(path);
        T jsonData = JsonUtility.FromJson<T>(json);
        return jsonData;
    }
    //예시 실행코드
    //wrapper = JsonClass
    //filePath = Json 경로
    //
    //public void SaveScanData()
    //{
    //    DataWrapper wrapper = new DataWrapper { objectDatas = dataList };
    //    SaveData(wrapper, filePath);
    //}
    //public List<ObjectScanData> LoadData()
    //{
    //    DataWrapper wrapper = LoadData<DataWrapper>(filePath, SaveScanData);
    //    return wrapper.objectDatas;
    //}

}
