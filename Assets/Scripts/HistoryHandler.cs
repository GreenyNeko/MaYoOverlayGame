using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class HistoryHandler : MonoBehaviour
{
    public GameObject[] hitVisualizer;                              // the elements that visualize hits
    public Color colorDef, colorCorrect, colorSloppy, colorMiss;    // the colors that should be represented
    private int internalCounter;                                    // which element should be affected next

    private void Start()
    {
        // initalize counter and color for each element
        internalCounter = 0;
        foreach(GameObject elem in hitVisualizer)
        {
            elem.GetComponent<RawImage>().color = colorDef;
        }
    }

    /**
     * Updates the respective element that should be affected next by setting it's color respectively given the type
     * Then call a function to update previous elements
     */
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

    // lerp the color by age where the oldest one has no color and the newer ones are more saturated(?)
    private void UpdateOldColors()
    {
        for(int i = 0; i < hitVisualizer.Length - 1; i++)
        {
            // iterate backwards
            int pos = internalCounter - 1 - i;
            // correct position from negative to positive index
            pos %= hitVisualizer.Length;
            if(pos < 0)
            {
                pos = hitVisualizer.Length + pos;
            }
            // get the previous color
            Color oldColor = hitVisualizer[pos].GetComponent<RawImage>().color;
            // update color to the lerped value between default and the color it had before
            hitVisualizer[pos].GetComponent<RawImage>().color = Color.Lerp(oldColor, colorDef, (i + 1.0f) / (float)hitVisualizer.Length);
        }
    }
}
