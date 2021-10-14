using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject overlayRunning;
    public GameObject overlayDone;
    public TMPro.TMP_Text textScore;
    public TMPro.TMP_Text textAccuracy;
    public TMPro.TMP_Text textRank;
    public TMPro.TMP_Text textCombo;

    public TMPro.TMP_Text textEndRank;
    public TMPro.TMP_Text textEndScore;
    public TMPro.TMP_Text textEndCorrect;
    public TMPro.TMP_Text textEndSloppy;
    public TMPro.TMP_Text textEndMiss;
    public TMPro.TMP_Text textEndCombo;
    public TMPro.TMP_Text textEndAccuracy;

    public static GameManager Instance;
    public HistoryHandler historyHandler;
    private int score;
    private int correctKanji;
    private int sloppyKanji;
    private int missedKanji;
    private int combo;
    private float accuracy;
    private float hitRate;
    private int maxCombo;
    private bool running;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        score = 0;
        correctKanji = 0;
        sloppyKanji = 0;
        missedKanji = 0;
        combo = 0;
        accuracy = 0;
        hitRate = 0;
        maxCombo = 0;
        running = true;
        overlayDone.SetActive(false);
        overlayRunning.SetActive(true);
        UpdateUI();
    }

    public void OnCorrect()
    {
        if(running)
        {
            correctKanji++;
            combo++;
            score += 300 * combo;
            UpdateAccAndRate();
            UpdateUI();
            historyHandler.RegisterHit(0);
        }
    }

    public void OnSloppy()
    {
        if(running)
        {
            sloppyKanji++;
            combo++;
            score += 100 * combo;
            UpdateAccAndRate();
            UpdateUI();
            historyHandler.RegisterHit(1);
        }
    }

    public void OnMiss()
    {
        if(running)
        {
            missedKanji++;
            if (combo > maxCombo) maxCombo = combo;
            combo = 0;
            UpdateAccAndRate();
            UpdateUI();
            historyHandler.RegisterHit(2);
        }
    }

    public void OnEnd()
    {
        if(running)
        {
            UpdateEndScreenUI();
            overlayDone.SetActive(true);
            overlayRunning.SetActive(false);
            running = false;
        }
        else
        {
            score = 0;
            correctKanji = 0;
            sloppyKanji = 0;
            missedKanji = 0;
            combo = 0;
            accuracy = 0;
            hitRate = 0;
            maxCombo = 0;
            UpdateUI();
            overlayDone.SetActive(false);
            overlayRunning.SetActive(true);
            running = true;
        }
    }

    private string GetRank()
    {
        if (accuracy == 1) return "SS";
        else if (accuracy > 0.9 && missedKanji == 0) return "S";
        else if (accuracy > 0.9 && hitRate > 0.8) return "A";
        else if (accuracy > 0.8 && hitRate > 0.6) return "B";
        else if (accuracy > 0.6 && hitRate > 0.4) return "C";
        else if (accuracy > 0.4) return "D";
        return " ";
    }

    private void UpdateAccAndRate()
    {
        accuracy = (correctKanji + sloppyKanji * 0.5f) / (correctKanji + sloppyKanji + missedKanji);
        hitRate = (float)(correctKanji + sloppyKanji) / (correctKanji + sloppyKanji + missedKanji);
    }

    private void UpdateUI()
    {
        string strScore = score.ToString();
        while(strScore.Length < 9)
        {
            strScore = "0" + strScore;
        }
        textScore.SetText(strScore);
        textRank.SetText(GetRank());
        textAccuracy.SetText((accuracy * 100).ToString("F2") + "%");
        textCombo.SetText(combo.ToString() + "x");
    }

    private void UpdateEndScreenUI()
    {
        string strScore = score.ToString();
        while (strScore.Length < 9)
        {
            strScore = "0" + strScore;
        }
        if (missedKanji == 0) maxCombo = combo;
        textEndRank.SetText(GetRank());
        textEndScore.SetText(strScore);
        textEndCorrect.SetText(correctKanji.ToString() + "x");
        textEndSloppy.SetText(sloppyKanji.ToString() + "x");
        textEndMiss.SetText(missedKanji.ToString() + "x");
        textEndCombo.SetText(maxCombo.ToString() + "x");
        textEndAccuracy.SetText((accuracy * 100).ToString("F2") + "%");
    }
}
