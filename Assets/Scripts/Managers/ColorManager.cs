using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public Material Color1;

    public Material Color2;
    
    public Material Color3;

    public Material Color4;

    public static ColorManager Instance;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    public Material GetColor(int color)
    {
        switch (color)
        {
            case 1:
                return Color1;
            case 2:
                return Color2;
            case 3:
                return Color3;
            case 4:
                return Color4;
            default:
                return null;
        }
    }
}
