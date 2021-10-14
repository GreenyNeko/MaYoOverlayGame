using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class HistoryHandler : MonoBehaviour
{
    public GameObject[] hitVisualizer;
    public Color colorDef, colorCorrect, colorSloppy, colorMiss;
    private int internalCounter;

    private void Start()
    {
        internalCounter = 0;
        foreach(GameObject elem in hitVisualizer)
        {
            elem.GetComponent<RawImage>().color = colorDef;
        }
    }

    public void RegisterHit(int type)
    {
        switch(type)
        {
            case 2:
                // miss
                hitVisualizer[internalCounter++].GetComponent<RawImage>().color = colorMiss;
                break;
            case 1:
                // sloppy
                hitVisualizer[internalCounter++].GetComponent<RawImage>().color = colorSloppy;
                break;
            default:
                // correct
                hitVisualizer[internalCounter++].GetComponent<RawImage>().color = colorCorrect;
                break;
        }
        internalCounter %= hitVisualizer.Length;
        UpdateOldColors();
    }

    private void UpdateOldColors()
    {
        for(int i = 0; i < hitVisualizer.Length - 1; i++)
        {
            int pos = internalCounter - 1 - i;
            pos %= hitVisualizer.Length;
            if(pos < 0)
            {
                pos = hitVisualizer.Length + pos;
            }
            Color oldColor = hitVisualizer[pos].GetComponent<RawImage>().color;
            hitVisualizer[pos].GetComponent<RawImage>().color = Color.Lerp(oldColor, colorDef, (i + 1.0f) / (float)hitVisualizer.Length);
        }
    }
}
