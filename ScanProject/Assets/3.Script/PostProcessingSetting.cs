using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScreenToneSettingData
{
    public float HueShift = 14;
    public float Saturation = 32;
    public float Brightness = 1;
    public float Contrast = 0;
}


public class PostProcessingSetting : MonoBehaviour
{
    [SerializeField]
    private PostProcessVolume volume;
    public ScreenToneSettingData screenToneData= new ScreenToneSettingData();
    private ColorGrading colorGrading;
    private string filePath;

    private void Start()
    {
        TryGetComponent(out volume);
        volume.profile.TryGetSettings(out colorGrading);
        filePath = Path.Combine(Application.persistentDataPath, "ToneData.json");
        screenToneData = CustomJsonManager.LoadData1(filePath, screenToneData);

        initTone();
    }
    private void initTone()
    {
        colorGrading.hueShift.value = screenToneData.HueShift;
        colorGrading.saturation.value = screenToneData.Saturation;
        colorGrading.brightness.value = screenToneData.Brightness;
        colorGrading.contrast.value = screenToneData.Contrast;
    }
    public void settingValueInit()
    {

    }
    public float ModifyValue(string variableName, float amount)
    {
        switch (variableName)
        {
            case "HueShift":
                colorGrading.hueShift.value += amount;
                screenToneData.HueShift = colorGrading.hueShift.value;
                return colorGrading.hueShift.value;
            case "Saturation":
                colorGrading.saturation.value += amount;
                screenToneData.Saturation = colorGrading.saturation.value;
                return colorGrading.saturation.value;
            case "Brightness":
                colorGrading.brightness.value += amount;
                screenToneData.Brightness = colorGrading.brightness.value;
                return colorGrading.brightness.value;
            case "Contrast":
                colorGrading.contrast.value += amount;
                screenToneData.Contrast = colorGrading.contrast.value;
                return colorGrading.contrast.value;

        }
        Debug.Log($"{variableName} updated");
        return 0;
    }
    public void SaveTone()
    {
        CustomJsonManager.SaveData(screenToneData, filePath);
    }
}
