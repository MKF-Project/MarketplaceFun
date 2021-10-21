using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour
{
    public GameObject NicknameCanvas;
    public Text NicknameText;
    
    public SkinnedMeshRenderer MeshRenderer;


    // Update is called once per frame
    void Update()
    {
        NicknameCanvas.transform.LookAt(Camera.main.transform.position);
    }

    public void DisplayNickname(String nickname)
    {
        NicknameText.text = nickname;
    }

    public void SetColor(int color)
    {
        Material material = ColorManager.Instance.GetColor(color);
        Material[] materials = MeshRenderer.materials;

        materials[0] = material;

        MeshRenderer.materials = materials;
    }
}
