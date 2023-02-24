using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    private Text scoreText;
    
    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText(ScoreKeeper scoreKeeper)
    {
        scoreText.text = $"Score: {scoreKeeper.score} | Moves: {scoreKeeper.moves} | Time: {scoreKeeper.GetTimeFormatted()}";
    }
}
