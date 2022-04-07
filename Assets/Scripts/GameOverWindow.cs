using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey;
using CodeMonkey.Utils;

public class GameOverWindow : MonoBehaviour
{
    private Text scoreText;
    public GameObject background;
    public GameObject text; 
    public GameObject scoreTextObject;
    public GameObject retryButton; 

    private void Awake() { 
        scoreText = transform.Find("ScoreText").GetComponent<Text>();

        transform.Find("RetryButton").GetComponent<Button_UI>().ClickFunc = () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        };
        Hide();
    }

    private void Start() {
        Bird.GetInstance().OnDied += Bird_OnDied;
    }

    private void Bird_OnDied(object sender, System.EventArgs e) {
        scoreText.text = (Level.GetInstance().GetPipesPassedCount()/2).ToString();
        Show();
    }

    private void Hide() {
        background.SetActive(false);
        text.SetActive(false);
        scoreTextObject.SetActive(false);
        retryButton.SetActive(false);
    }

    private void Show() {
        background.SetActive(true);
        text.SetActive(true);
        scoreTextObject.SetActive(true);
        // retryButton.SetActive(true);
    }
}
