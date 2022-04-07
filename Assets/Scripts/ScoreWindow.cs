using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreWindow : MonoBehaviour
{
    private Text scoreText;

    private void Awake() {
        scoreText = transform.Find("ScoreText").GetComponent<Text>();
    }

    // divded by two to because bird passing two pipes every gap
    private void Update() {
        scoreText.text = (Level.GetInstance().GetPipesPassedCount()/2).ToString();
    }
}
