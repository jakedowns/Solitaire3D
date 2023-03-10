using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class LeiaDebugDropdown : MonoBehaviour
{
    Dropdown dropdown;
    enum Option { Default = 0, Cube = 1, Rainbow = 2, Cup = 3, GrayPattern = 4, View0White = 5, ACTTestPattern = 6, R0Test = 7 };
    
    [SerializeField] private GameObject debugUI;
    [SerializeField] private LeiaVirtualDisplay leiaVirtualDisplay;
    [SerializeField] private GameObject cube;
    [SerializeField] private GameObject rainbowPattern;
    [SerializeField] private GameObject cup;
    [SerializeField] private GameObject grayPattern;
    [SerializeField] private GameObject view0White;
    [SerializeField] private GameObject actTestPattern;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    public void OnValueChanged()
    {
        Option optionSelected = (Option)dropdown.value;
        
        //Disable camera z-axis movement for the media viewer objects
        leiaVirtualDisplay.leiaCamera.cameraZaxisMovement = (optionSelected == Option.Cube || optionSelected == Option.Default);

        if (cube != null)
        {
            cube.SetActive(optionSelected == Option.Cube);
        }
        else
        {
            Debug.LogWarning("cube not set");
        }
        LeiaDisplay.Instance.SetR0TestEnabled(optionSelected == Option.R0Test);

        if (rainbowPattern != null)
        {
            rainbowPattern.SetActive(optionSelected == Option.Rainbow);
        }
        else
        {
            Debug.LogWarning("rainbowPattern not set");
        }
        if (cup != null)
        {
            cup.SetActive(optionSelected == Option.Cup);
        }
        else
        {
            Debug.LogWarning("cup not set");
        }
        if (grayPattern != null)
        {
            grayPattern.SetActive(optionSelected == Option.GrayPattern);
        }
        else
        {
            Debug.LogWarning("grayPattern not set");
        }
        if (view0White != null)
        {
            view0White.SetActive(optionSelected == Option.View0White);
        }
        else
        {
            Debug.LogWarning("view0White not set");
        }
        
        if (actTestPattern != null)
        {
            actTestPattern.SetActive(optionSelected == Option.ACTTestPattern);
        }
        else
        {
            Debug.LogWarning("actTestPattern not set");
        }
    }
}
