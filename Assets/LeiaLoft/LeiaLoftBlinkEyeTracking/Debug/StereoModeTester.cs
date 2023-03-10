using UnityEngine;
using LeiaLoft;

public class StereoModeTester : MonoBehaviour
{

    public void SetStereoMode(bool stereoMode)
    {
        if (stereoMode)
        {
            LeiaDisplay.Instance.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Stereo;
        }
        else
        {
            LeiaDisplay.Instance.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Multiview;
        }
    }
}
