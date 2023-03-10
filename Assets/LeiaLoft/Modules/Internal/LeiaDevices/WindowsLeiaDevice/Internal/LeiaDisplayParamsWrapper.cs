using System;
using System.Runtime.InteropServices;

namespace LeiaLoft
{
    /// <summary>
    /// Class which wraps the LeiaDisplayParams dll. This DLL retrieves members of the DisplayConfig data structure.
    /// Calling GetValue/SetValue will
    /// route the call to one of the extern functions, which will
    /// route the call to the LeiaDisplayParams dll.
    /// </summary>
    public class LeiaDisplayParamsWrapper : AbstractArtifactWrapper
    {
        private const string dllName = "LeiaDisplayParams";

        [DllImport(dllName)] private static extern IntPtr getDisplayConfigString();
        [DllImport(dllName)] private static extern float getConvergenceDistance();
        [DllImport(dllName)] private static extern float getBeta();
        [DllImport(dllName)] private static extern float getGamma();
        [DllImport(dllName)] private static extern int getSystemDisparity();
        // float**
        [DllImport(dllName)] private static extern IntPtr getInterlacingMatrix();
        [DllImport(dllName)] private static extern IntPtr getInterlacingVector();
        [DllImport(dllName)] private static extern bool isSlanted();
        [DllImport(dllName)] private static extern bool isDisplayConnected();
        [DllImport(dllName)] private static extern int getBacklightMode();
        [DllImport(dllName)] private static extern IntPtr getPanelResolution();
        [DllImport(dllName)] private static extern IntPtr getNumViews();
        [DllImport(dllName)] private static extern IntPtr getAlignmentOffset();
        [DllImport(dllName)] private static extern IntPtr getDotPitchInMm();
        [DllImport(dllName)] private static extern IntPtr getDisplaySizeInMm();
        [DllImport(dllName)] private static extern IntPtr getViewResolution();
        [DllImport(dllName)] private static extern IntPtr getPlatformVersion();
        [DllImport(dllName)] private static extern float getCenterViewNumber();
        [DllImport(dllName)] private static extern IntPtr getViewboxSize();
        [DllImport(dllName)] private static extern IntPtr getDisplayId();
        // float**
        [DllImport(dllName)] private static extern IntPtr getViewSharpeningCoefficients();

    }
}
