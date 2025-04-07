using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialUVController : MonoBehaviour
{
    public float moveStep = 0.01f;
    public float scaleStep = 0.01f;

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) mat = rend.sharedMaterial;
    }

    void Update()
    {
        if (mat == null) return;

        Vector2 offset = mat.mainTextureOffset;
        Vector2 scale = mat.mainTextureScale;

        float step = Input.GetKey(KeyCode.LeftShift) ? moveStep / 10f : moveStep;

        // 방향키로 Offset 조정
        if (Input.GetKey(KeyCode.RightArrow)) offset.x += step;
        if (Input.GetKey(KeyCode.LeftArrow)) offset.x -= step;
        if (Input.GetKey(KeyCode.UpArrow)) offset.y += step;
        if (Input.GetKey(KeyCode.DownArrow)) offset.y -= step;

        // PageUp/PageDown으로 Scale 조정 (선택사항)
        if (Input.GetKey(KeyCode.PageUp)) scale += Vector2.one * scaleStep;
        if (Input.GetKey(KeyCode.PageDown)) scale -= Vector2.one * scaleStep;

        mat.mainTextureOffset = offset;
        mat.mainTextureScale = scale;
    }
}
