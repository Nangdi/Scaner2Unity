using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public enum VariableName
{
    X,
    Y,
    CropSize,
    HRatio
}


public class UIButtonIncrementer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    public OffsetTuner tuner;
    public VariableName variableName;   // ��: "int1", "float2"
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
    }
    public void OnPointerDown()
    {
        if (tuner == null) return;
        ChangeAmount();
        isHolding = true;
        float value = tuner.ModifyValue(variableName.ToString(), direction* amount);
        ReplaceValue(value);
        holdCoroutine = StartCoroutine(RepeatModify());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDown();
        Debug.Log("��ư����");
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
        yield return new WaitForSeconds(0.2f); // �� ������ ���� ���
        Debug.Log("�ڷ�ƾ������");
        while (isHolding)
        {
        Debug.Log("�����ȭ");
           float value =  tuner.ModifyValue(variableName.ToString(), direction* amount);
            ReplaceValue(value);
            if (stopRequested)
            {
                break;
            }
            yield return new WaitForSeconds(0.05f); // ���� ���� ����
        }
        stopRequested = false;
    }
    private void ReplaceValue(float value)
    {
        Debug.Log("���ڿ���ü");
        text.text = cashText + value.ToString();
        //�����ּҰ����;���
    }
  
    //������ư����, ���ҹ�ư���� , � �Ķ���͸� �����Ұ�����.
    public void ChangeAmount()
    {

        amount = float.Parse(inputField.text);
    }
}
