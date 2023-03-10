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

using UnityEditor;
using UnityEngine;

namespace LeiaLoft.Editor
{
    public class LeiaReleaseNotesWindow 
    {
        private string releaseNotesText;
        private static bool isChangelogFoldOut = true;
        private static Vector2 scrollPosition;
        private static readonly float changelogWidth = 500;
        static GUIStyle changeLogStyle;
        public void DrawGUI()
        {
            const int maxNoteLength = 16000;
            if (releaseNotesText == null) {
                releaseNotesText = Resources.Load<TextAsset>("RELEASE").text;
                if (!string.IsNullOrEmpty(releaseNotesText) && releaseNotesText.Length > maxNoteLength)
                {
                    releaseNotesText = releaseNotesText.Substring(0, maxNoteLength) + "\nTruncated...\n";
                }
            }
            if (changeLogStyle == null) {
                changeLogStyle = new GUIStyle(EditorStyles.textArea) {
                    richText = true 
                };
            }
            isChangelogFoldOut = EditorWindowUtils.Foldout(isChangelogFoldOut, "Release Notes");

            if (isChangelogFoldOut)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorWindowUtils.Space(10);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        scrollPosition = EditorWindowUtils.DrawScrollableSelectableLabel(
                            scrollPosition,
                            changelogWidth,
                            releaseNotesText,
                            changeLogStyle);
                    }
                }
            }
        }
    }
}
