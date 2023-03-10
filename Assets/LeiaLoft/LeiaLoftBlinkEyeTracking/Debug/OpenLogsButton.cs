using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OpenLogsButton : MonoBehaviour
{

    public void OpenLog()
    {
        OpenOutputLog();
    }

    void OpenOutputLog()
    {
        string filename = "C:/Users/"+Environment.UserName+"/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/Player.log";
            
            Debug.Log("Attempting to open " + filename);

            if (File.Exists(filename))
            {
                Application.OpenURL(filename);
            }
            else
            {
                Debug.Log("Failed to open file");
            }
    }
}
