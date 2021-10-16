using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // UI menues to hide/show
    public GameObject overlayRunning;
    public GameObject overlayDone;

    // UI elemnts to update in running menu
    public TMPro.TMP_Text textScore;
    public TMPro.TMP_Text textAccuracy;
    public TMPro.TMP_Text textRank;
    public TMPro.TMP_Text textCombo;

    // UI elements to update in end screen menu
    public TMPro.TMP_Text textEndRank;
    public TMPro.TMP_Text textEndScore;
    public TMPro.TMP_Text textEndCorrect;
    public TMPro.TMP_Text textEndSloppy;
    public TMPro.TMP_Text textEndMiss;
    public TMPro.TMP_Text textEndCombo;
    public TMPro.TMP_Text textEndAccuracy;

    // instance to always access the game manager from wherever we need
    public static GameManager Instance
    {
        get;
        private set;
    }
    // history handler to update the history visualizer
    public HistoryHandler historyHandler;

    private int score;          // keep track of players score
    private int correctKanji;   // keep track of how many kanji are read correctly
    private int sloppyKanji;    // keep track of misread kanji but they have the reading
    private int missedKanji;    // keep track of kanji that were misread, not known or skipped
    private int combo;          // the combo the player got
    private float accuracy;     // % given the correct, sloppy and missed kanji
    private float hitRate;      // sloppy + correct kanji
    private int maxCombo;       // the highest the combo has been
    private bool running;       // whether or not the game is running

    // Start is called before the first frame update
    void Start()
    {
        // init
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
        // don't show end screen, show game screen
        overlayDone.SetActive(false);
        overlayRunning.SetActive(true);
        // pre setup the UI
        UpdateUI();
    }

    /**
     * Tells the game manager a kanji has been read correct
     */
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

    /**
     * Tells the game manager a kanji has been read sloppy
     */
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

    /**
     * Tells the game manager a kanji has been missed/misread/skipped
     */
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

    /**
     * Called when escape has been pressed and shows the end screen, if it already shows end screen it switches to game screen and resets the game
     */
    public void OnEnd()
    {
        if(running)
        {
            // update UI
            UpdateEndScreenUI();
            // change menu
            overlayDone.SetActive(true);
            overlayRunning.SetActive(false);
            // game has ended
            running = false;
        }
        else
        {
            // reset the game
            score = 0;
            correctKanji = 0;
            sloppyKanji = 0;
            missedKanji = 0;
            combo = 0;
            accuracy = 0;
            hitRate = 0;
            maxCombo = 0;
            // update UI
            UpdateUI();

            // change menu
            overlayDone.SetActive(false);
            overlayRunning.SetActive(true);
            // set the game as running
            running = true;
        }
    }

    // returns the rank given your accuracy, hitrate and missed kanji's
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

    // updates the accuracy and hitrate
    private void UpdateAccAndRate()
    {
        accuracy = (correctKanji + sloppyKanji * 0.5f) / (correctKanji + sloppyKanji + missedKanji);
        hitRate = (float)(correctKanji + sloppyKanji) / (correctKanji + sloppyKanji + missedKanji);
    }

    // updates the UI elements of the game
    private void UpdateUI()
    {
        string strScore = score.ToString();
        // add trailing 0's at the beginning
        while (strScore.Length < 9)
        {
            strScore = "0" + strScore;
        }
        textScore.SetText(strScore);
        textRank.SetText(GetRank());
        textAccuracy.SetText((accuracy * 100).ToString("F2") + "%");
        textCombo.SetText(combo.ToString() + "x");
    }

    // update the UI elements of the end screen
    private void UpdateEndScreenUI()
    {
        string strScore = score.ToString();
        // add trailing 0's at the beginning
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
