/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace LeiaLoft
{
	/// <summary>
    /// Defines file IO and external library calls for Leia firmware on Linux.
    /// </summary>
	public class LinuxLeiaDevice : AbstractLeiaDevice
	{
		private const int backlightStatusFail = 0;

		private static DisplayParseTarget parseTarget;

		/// <summary>
		/// Initializes the <see cref="LeiaLoft.LinuxLeiaDevice"/> class.
		/// 
		/// When the parseTarget static variable is retrieved, a call is automatically spliced in to first run this static code
		/// which actually populates the parseTarget static variable.
		/// </summary>
		static LinuxLeiaDevice()
		{
			if (parseTarget == null)
			{
				// this line causes DisplayConfig-like-data to be written to program's console standard output
				getDisplayConfig();

				if (Application.platform != RuntimePlatform.LinuxPlayer)
				{
					// Generate error, but do not stop progress in case user intends to use this functionality
					LogUtil.Log(LogLevel.Error, "Trying to construct a LinuxLeiaDevice obj on platform {0}", Application.platform);
				}

				// this line is an absolute path to program's log, which contains standard output
				// Linux-specific so unix-style paths will always be fine
				string linuxBuildLogPath = string.Format("{0}/.config/unity3d/{1}/{2}/Player.log",
					Environment.GetFolderPath(Environment.SpecialFolder.Personal), Application.companyName, Application.productName);

				if (!File.Exists(linuxBuildLogPath))
				{
					LogUtil.Log(LogLevel.Error, "No log found at {0}", linuxBuildLogPath);
					return;
				}

				try
				{
					string text = File.ReadAllText(linuxBuildLogPath);

					Regex regConfig = new Regex("{\\s*\"CMD\"\\s*:\\s*\"getDisplayConfig\".*(\\n\"\\s*.*)*}");
					Match m = regConfig.Match(text);

					if (!m.Success)
					{
						// if no parseable data, return
						LogUtil.Log(LogLevel.Error, "No DisplayConfig-like json found in {0}", linuxBuildLogPath);
						return;
					}

					// do some text processing before json utility can parse data
					// data comes in with mild formatting issues
					string propertyFixed = m.Value;
					// remove quotes around "[collections]"
					propertyFixed = Regex.Replace(propertyFixed, "\"\\[(?<prop>.*)\\]\"", "[${prop}]");
					// remove quotes around "floats.floats"
					propertyFixed = Regex.Replace(propertyFixed, "\"(?<prop>\\d*.\\d*)\"", "${prop}");
					parseTarget = JsonUtility.FromJson<DisplayParseTarget>(propertyFixed);

					if (parseTarget == null)
					{
						LogUtil.Log(LogLevel.Error, "Json conversion of text failed. ParseTarget is null, formatted text was {0}, original text was {1}", propertyFixed, m.Value);
					}
				}
				catch (Exception e)
				{
					LogUtil.Log(LogLevel.Error, e.ToString());
				}

			}
		}

		/// <summary>
		/// Recruits firmware data to get display size on vertical
		/// </summary>
		/// <returns>Integer vertical size in mm</returns>
		public override int GetDisplayHeight()
		{
			return parseTarget.getDisplaySizeInMm[1];
		}

		/// <summary>
		/// Recruits firmware data to get display size on horizontal
		/// </summary>
		/// <returns>Integer horizontal size in mm</returns>
		public override int GetDisplayWidth()
		{
			return parseTarget.getDisplaySizeInMm[0];
		}

		/// <summary>
		/// Returns number of horizontal views on the display
		/// </summary>
		/// <returns>Count of horizontal views</returns>
		public override int GetDisplayViewcount()
		{
			return parseTarget.getNumViewsHorizontal;
		}

		/// <summary>
		/// Gets board display class
		/// </summary>
		/// <returns>Series - A0, A1, etc.</returns>
		public string GetBoardDisplayClass()
		{
			return parseTarget.getDisplayClass;
		}

		/// <summary>
		/// A string representing that particular display's ID
		/// </summary>
		/// <returns>String containing class (A0), generation (Her), device ID</returns>
		public string GetBoardDisplayID()
		{
			return parseTarget.getDisplayID;
		}

		/// <summary>
		/// Gets firmware version
		/// </summary>
		/// <returns>A series of characters representing a semantic version</returns>
		public string GetBoardPlatformVersion()
		{
			// not supported yet
			return "0.0.0.0";
		}

		public override int[] CalibrationOffset
		{
			get
			{
				return new int[2];
			}
			set
			{
				this.Warning("CalibrationOffset - value {0}. Setting calibration from Unity Plugin is not supported anymore - use relevant app instead.",value);
			}
		}

		public LinuxLeiaDevice(string stubName)
		{
			// this constructor intentionally left blank
		}

		/// <summary>
        /// Sets backlight.
        /// </summary>
        /// <param name="modeId">2 = off, 3 = on.</param>
		public override void SetBacklightMode(int modeId)
		{
			if (IsConnected())
			{
				if (modeId == 3)
				{
					requestBacklightMode(BacklightMode.MODE_3D);
				}
				else
				{
					requestBacklightMode(BacklightMode.MODE_2D);
				}
			}
			else
			{
				LogUtil.Log (LogLevel.Error, "Display not connected");
			}
		}

		/// <summary>
		/// Sets backlight.
		/// </summary>
		/// <param name="modeId">2 = off, 3 = on.</param>
		/// <param name="delay">No effect yet</param>
		public override void SetBacklightMode(int modeId, int delay)
		{
			SetBacklightMode(modeId);
		}

		/// <summary>
		/// Sets backlight.
		/// </summary>
		/// <param name="modeId">2 = off, 3 = on.</param>
		public override void RequestBacklightMode(int modeId)
		{
			SetBacklightMode(modeId);
		}

		/// <summary>
		/// Sets backlight.
		/// </summary>
		/// <param name="modeId">2 = off, 3 = on.</param>
		/// <param name="delay">No effect yet</param>
		public override void RequestBacklightMode(int modeId, int delay)
		{
			SetBacklightMode(modeId);
		}

		/// <summary>
        /// Returns backlight state.
        /// </summary>
        /// <returns>2 if backlight off, 3 if backlight on. 0 if backlight fail</returns>
		public override int GetBacklightMode()
		{
			if (IsConnected())
			{
				try
				{
					return getBacklightMode();
				}
				catch (Exception e)
				{
					LogUtil.Log(LogLevel.Error, "LinuxLeiaDevice :: GetBacklightMode exception {0}", e);
					return (backlightStatusFail);
				}
			}
			else
			{
				return (backlightStatusFail);
			}
		}

		/// <summary>
		/// Gets displayConfig from firmware.
		/// Applies modification policies to the DisplayConfig.
		/// </summary>
		/// <returns>A DisplayConfig which has been retrieved from board then had sparse user-specified updates applied to it.</returns>
		public override DisplayConfig GetDisplayConfig()
		{
			// recruit DisplayParseTarget's explicit class conversion operator
			// assign into AbstractLeiaDevice's DisplayConfig _displayConfig
			_displayConfig = (DisplayConfig)parseTarget;

			// populate _displayConfig from FW with developer-tuned values
			base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);
			return _displayConfig;
		}

		/// <summary>
		/// Re-requests info from display about connection.
		/// </summary>
		/// <returns>True if platform is correct, shared object is available, and display is connected; false otherwise</returns>
		public override bool IsConnected()
		{
			if (Application.platform != RuntimePlatform.LinuxPlayer)
			{
				LogUtil.Log(LogLevel.Error, "LinuxLeiaDevice attempted on a platform other than Linux build. Platform is {0}", Application.platform);
				return false;
			}

			try
			{
				return (isDisplayConnected());
			}
			catch (Exception e)
			{
				LogUtil.Log(LogLevel.Error, "Tried to connect to display, got exception {0}", e);
				return false;
			}
		}

		public override RuntimePlatform GetRuntimePlatform()
		{
			return RuntimePlatform.LinuxPlayer;
		}

#region shared_object_calls 

		[DllImport("libleialfapi")]
		// really returns a DisplayConfigStruct but this is irrelevant since the struct returned is misformatted.
		private static extern void getDisplayConfig();

		[DllImport("libleialfapi")]
		private static extern bool isDisplayConnected();

		[DllImport("libleialfapi")]
		private static extern int requestBacklightMode(BacklightMode mode);

		[DllImport("libleialfapi")]
		private static extern int getBacklightMode();

#endregion

#region firmware_structs

		enum BacklightMode
		{
			MODE_2D = 2,
			MODE_3D
		}

		/// <summary>
		/// Class supports member population using private serializable properties.
		/// 
		/// Class supports member retrieval using get<propertyname>
		/// 
		/// Could have used public properties for both serialization and get/set, but this would cause Codacy flags.
		/// </summary>
		[Serializable]
		private class DisplayParseTarget
		{
#pragma warning disable 0649
			[SerializeField] private string DisplayClass;
			public string getDisplayClass { get { return DisplayClass; } }
			[SerializeField] private string DisplayID;
			public string getDisplayID { get { return DisplayID; } }
			[SerializeField] private float[] DisplaySizeInMm;
			public int[] getDisplaySizeInMm { get { return new[] { (int)DisplaySizeInMm[0], (int)DisplaySizeInMm[1] }; } } // data is parsed as float but AbstractLeiaDevice API is for an int
			[SerializeField] private float[] DotPitchInMm;
			public float[] getDotPitchInMm { get { return DotPitchInMm; } }

			[SerializeField] private int[] PanelResolution;
			[SerializeField] private int[] NumViews;
			public int getNumViewsHorizontal { get { return NumViews[0]; } }
			[SerializeField] private float[] InterlacingMatrix1;
			[SerializeField] private float[] InterlacingMatrix2;
			[SerializeField] private float[] InterlacingVector;
			[SerializeField] private int Slant;
			[SerializeField] private float[] AlignmentOffset;
			[SerializeField] private float[] ActCoefficientsX;
			[SerializeField] private float[] ActCoefficientsY;
			[SerializeField] private float Gamma;
			[SerializeField] private float Beta = 1.4f; // not in parsed data
			[SerializeField] private float SystemDisparityPixels;
			[SerializeField] private float ConvergenceDistance;
			[SerializeField] private float CenterViewNumber;
			[SerializeField] private float[] ViewBoxSize;
			[SerializeField] private string result; // serializable just in case, but not part of any access API
#pragma warning restore 0649

			public static explicit operator DisplayConfig(DisplayParseTarget orig)
			{
				// calls constructor...! DisplayConfig constructor should just construct a valid DisplayConfig object
				DisplayConfig cfg = new DisplayConfig();

				// FW gives InterlacingMatrix as an 8x1 vec in InterlacingMatrix1, and an 8x1 vec in InterlacingMatrix2
				float[] imatrix = new float[orig.InterlacingMatrix1.Length + orig.InterlacingMatrix2.Length];
				Array.Copy(orig.InterlacingMatrix1, 0, imatrix, 0, orig.InterlacingMatrix1.Length);
				Array.Copy(orig.InterlacingMatrix2, 0, imatrix, orig.InterlacingMatrix1.Length, orig.InterlacingMatrix2.Length);

				cfg.InterlacingMatrix = imatrix;
				cfg.InterlacingVector = orig.InterlacingVector;
				cfg.Gamma = orig.Gamma;
				cfg.Slant = true; // DC slant is true/false, while DisplayParseTarget Slant is int 1
				cfg.Beta = orig.Beta;
				cfg.isSquare = false; // hardcoded; not in struct yet
				cfg.isSlanted = true; // hardcoded; not in struct yet
				cfg.PixelPitchInMM = new XyPair<float>(orig.DotPitchInMm[0], orig.DotPitchInMm[1]);
				cfg.PanelResolution = new XyPair<int>(orig.PanelResolution[0], orig.PanelResolution[1]);
				cfg.NumViews = new XyPair<int>(orig.NumViews[0], orig.NumViews[1]);
				cfg.AlignmentOffset = new XyPair<float>(orig.AlignmentOffset[0], orig.AlignmentOffset[1]);
				cfg.ActCoefficients = new XyPair<List<float>>(new List<float>(orig.ActCoefficientsX), new List<float>(orig.ActCoefficientsY));
				cfg.SystemDisparityPercent = orig.SystemDisparityPixels / 100.0f;
				cfg.SystemDisparityPixels = orig.SystemDisparityPixels;
				cfg.DisplaySizeInMm = new XyPair<int>(orig.getDisplaySizeInMm[0], orig.getDisplaySizeInMm[1]); // wants int x int, so recruit property getter. generates garbage
				cfg.ViewResolution = new XyPair<int>(1280, 720); // hardcoded; not in parsable data yet

				return cfg;
			}
		}
#endregion

	}
}
