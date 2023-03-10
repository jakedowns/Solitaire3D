using System;
using System.Runtime.InteropServices;

namespace LeiaLoft
{
    /// <summary>
    /// Class which wraps the LeiaDisplaySdkCpp dll. This DLL handles several versioning and backlight state calls for Windows devices.
    ///
    /// Calling GetValue/SetValue will
    /// route the call to one of the extern functions, which will
    /// route the call to the LeiaDisplaySdkCpp dll.
    /// </summary>
    public class LeiaDisplaySdkCppWrapper : AbstractArtifactWrapper
    {
        private const string dllName = "LeiaDisplaySdkCpp";

        // shared with LeiaDisplayParamsWrapper
        [DllImport(dllName)] private static extern IntPtr getPlatformVersion();
        [DllImport(dllName)] private static extern bool isDisplayConnected();

        [DllImport(dllName)] private static extern int getBacklightMode();
        [DllImport(dllName)] private static extern void requestBacklightMode(int mode);
        [DllImport(dllName)] private static extern void setBrightnessLevel(char brightness);
        [DllImport(dllName)] private static extern void restoreDefaults();

        [DllImport(dllName)] private static extern void setBacklightTransition(float ratio2d, float ratio3d);
    }
}
