#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace LeiaLoft
{
    /// <summary>
    /// Post-build process
    /// Copy
	/// Win
	///     LeiaDisplayService.Wcf.dll      to      build directory
    /// </summary>
#if UNITY_2018_1_OR_NEWER
class LeiaPostBuildProcess : IPostprocessBuildWithReport
#else
    class LeiaPostBuildProcess : IPostprocessBuild
#endif
    {
        public int callbackOrder { get { return 0; } }

#if UNITY_2018_1_OR_NEWER

    /// <summary>
    /// Post-process. Automatically called by Unity after building
    /// </summary>
    /// <param name="br">BuildReport with a summary and output path</param>
    public void OnPostprocessBuild(BuildReport br)
    {
        string app_path = br.summary.outputPath;
        string dest_path = Path.GetDirectoryName(br.summary.outputPath);
#else

        /// <summary>
        /// Post-process. Automatically called by Unity after building
        /// </summary>
        /// <param name="br">BuildTarget object with an output path</param>
        public void OnPostprocessBuild(BuildTarget bt, string path)
        {
            string app_path = path;
            string dest_path = Path.GetDirectoryName(app_path);
#endif

            List<string> copy_file_names = new List<string>();

#if UNITY_STANDALONE_WIN
            // on Win, add DLL for backlight communication
            copy_file_names.Add("LeiaDisplayService*.dll");

#endif

            foreach (string copy_file_name in copy_file_names)
            {
                IEnumerable<string> matching_file_paths = new DirectoryInfo(Application.dataPath).GetFiles(copy_file_name, SearchOption.AllDirectories).Select(x => x.FullName);
                if (matching_file_paths.Count() == 0)
                {
                    Debug.LogFormat("no file matching {0} found", copy_file_name);
                }
                else
                {
                    string first_source = matching_file_paths.FirstOrDefault();

                    if (matching_file_paths.Count() > 1)
                    {
                        LogUtil.Log(LogLevel.Warning, "Multiple files matching {0} detected. Using {1}", copy_file_name, first_source);
                    }

                    if (!string.IsNullOrEmpty(first_source))
                    {
                        string source_filename = first_source;
                        string dest_filename = Path.Combine(dest_path, Path.GetFileName(first_source));

                        FileUtil.ReplaceFile(source_filename, dest_filename);
                    }
                }
            }

#if UNITY_STANDALONE_WIN
            Directory.CreateDirectory(Path.Combine(dest_path, "blink_resources"));

            FileUtil.ReplaceFile("Assets/LeiaLoft/LeiaHeadTracking/native/bin/blink_resources/blink_detector_model",
               Path.Combine(dest_path, "blink_resources/blink_detector_model"));
            FileUtil.ReplaceFile("Assets/LeiaLoft/LeiaHeadTracking/native/bin/blink_resources/face_lock_config",
               Path.Combine(dest_path, "blink_resources/face_lock_config"));
            FileUtil.ReplaceFile("Assets/LeiaLoft/LeiaHeadTracking/native/bin/blink_resources/face_model_a",
               Path.Combine(dest_path, "blink_resources/face_model_a"));
#endif
        }
    }
}

#endif