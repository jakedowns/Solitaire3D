using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeiaLoft;
public class DebugPanel : MonoBehaviour
{
    public void SetProfilingEnabled(bool enabled)
    {
        LeiaDisplay.Instance.tracker.SetProfilingEnabled(enabled);
    }
}
