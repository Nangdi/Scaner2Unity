using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Windows.Speech;

public enum VariableName
{
    X,
    Y,
    CropSize,
    HRatio,
    HueShift,
    Saturation,
    Brightness,
    Contrast
}


public class UIButtonIncrementer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    public OffsetTuner tuner;
    public PostProcessingSetting processingSetting;
    public VariableName variableName;   // 예: "int1", "float2"
    public float direction = 1f;
    public TMP_Text text;
    public TMP_InputField inputField;
    public float amount =1;
    private string cashText;
    private bool isHolding = false;
    private bool stopRequested;
    private Coroutine holdCoroutine;

    private void Start()
    {
        cashText = text.text;

        StartCoroutine(RateValueUpdate());
    }
    IEnumerator  RateValueUpdate()
    {
        yield return new WaitForSeconds(3f);
        ModifyValue(variableName, 0);
    }
    public void OnPointerDown()
    {
        if (tuner == null) return;
        ChangeAmount();
        isHolding = true;
        ModifyValue(variableName, direction * amount);
        holdCoroutine = StartCoroutine(RepeatModify());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDown();
        Debug.Log("버튼눌림");
    }

    public void OnPointerUp()
    {
        isHolding = false;
        stopRequested = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUp();
    }

    IEnumerator RepeatModify()
    {
        yield return new WaitForSeconds(0.2f); // 꾹 누르기 감지 대기
        Debug.Log("코루틴실행중");
        while (isHolding)
        {
        Debug.Log("밸류변화");
            
            ModifyValue(variableName, direction * amount);
            if (stopRequested)
            {
                break;
            }
            yield return new WaitForSeconds(0.05f); // 연속 증가 간격
        }
        stopRequested = false;
    }
    private void ReplaceValue(float value)
    {
        Debug.Log("문자열교체");
        text.text = cashText + value.ToString();
        //변수주소가져와야함
    }
  
    //증가버튼인지, 감소버튼인지 , 어떤 파라미터를 조절할것인지.
    public void ChangeAmount()
    {

        amount = float.Parse(inputField.text);
    }
    public void InitValue()
    {
        ModifyValue(variableName, 0);
        
    }
    public void ModifyValue(VariableName name , float amount)
    {
        float value;
        if ((int)name < 4)
        {
            value = tuner.ModifyValue(variableName.ToString(), amount);
        }
        else
        {
            value = processingSetting.ModifyValue(variableName.ToString(), amount);
        }
        ReplaceValue(value);
    }
}
