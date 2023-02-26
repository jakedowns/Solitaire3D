using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyController : MonoBehaviour
{
    private Slider mySlider;
    private Text myText;
    
    // Start is called before the first frame update
    void Start()
    {
        myText = GameObject.Find("DifficultyText").GetComponent<Text>();
        mySlider = GetComponent<Slider>();

        // attach event listener to slider
        mySlider.onValueChanged.AddListener(delegate { OnSliderUpdated(); });

        PoweredOn.Managers.GameManager.Instance.difficultyAssistant.SetDifficultySliderGameObject(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnSliderUpdated(){
        PoweredOn.Managers.GameManager.Instance.SetDifficulty(mySlider.value);
        myText.text = "Difficulty: " + mySlider.value.ToString();
    }

    public void UpdateDifficulty(int difficulty) {
        mySlider.value = difficulty;
    }
}
