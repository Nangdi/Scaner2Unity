using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public int lifeTime;
    public int correctionTime ;
    public int basedFieldCount ;

    public int calculatedLifeTime;
    public float currentLifeTime;
    private ObjectController objectController;
    private void Start()
    {
        lifeTime = GameManager.instance.gameSettingData.LifeTime;
        correctionTime = GameManager.instance.gameSettingData.correctionTime;
        basedFieldCount = GameManager.instance.gameSettingData.basedFieldCount;
        calculatedLifeTime = lifeTime - ((GameManager.instance.ObjectCount / basedFieldCount) * correctionTime);
        StartCoroutine(LifeCheckLoop());
    }

    IEnumerator LifeCheckLoop()
    {
        while (true)
        {
            currentLifeTime += Time.deltaTime;
            if(currentLifeTime > calculatedLifeTime)
            {
                TryGetComponent(out objectController);
                //이름표도 삭제
                GameManager.instance.ObjectCount--;
                Destroy(objectController.myNameTagOb);
                Destroy(gameObject);
                yield break; 
            }
            yield return null;
        }
    }
}
