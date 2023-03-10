﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LeiaLoft.Editor
{
    [CustomEditor(typeof(LeiaLoft.LeiaMediaViewer))]
    public class LeiaMediaViewerEditor : UnityEditor.Editor
    {
        LeiaLoft.LeiaMediaViewer lmv;

        SerializedProperty leiaMediaVideoURL;
        SerializedProperty leiaMediaVideoClip;
        SerializedProperty leiaMediaTexture;
        SerializedProperty onscreenPercent;
 
        private enum propertyStringIDs
        {
            LeiaMediaCol = 0,
            LeiaMediaRow = 1
        }
        // layout: ID = row. Row[0] = field name on LeiaMediaViewer. Row[1] = public-facing label for that property
        private readonly string[][] propertyStringResources = new string[][]
        {
            new [] {"property_col_count", "LeiaMedia {0} Column count"},
            new [] {"property_row_count", "LeiaMedia {0} Row count" }
        };

        void OnEnable()
        {
            lmv = (LeiaMediaViewer)target;

            leiaMediaVideoURL = serializedObject.FindProperty("leiaMediaVideoURL");
            leiaMediaVideoClip = serializedObject.FindProperty("leiaMediaVideoClip");
            leiaMediaTexture = serializedObject.FindProperty("leiaMediaTexture");
            onscreenPercent = serializedObject.FindProperty("onscreenPercent");
        }

        public override void OnInspectorGUI()
        {
    
            UndoableInputFieldUtils.EnumFieldWithTooltip(
                () => { return lmv.activeMediaType; },
                (System.Enum value) => { lmv.activeMediaType = (LeiaMediaViewer.LeiaMediaType)value; },
                "Select active LeiaMedia", "Users can select which LeiaMedia is being displayed", lmv);

            EditorGUILayout.LabelField("Set media here. Do NOT use VideoPlayer.");
            EditorGUILayout.HelpBox("Media filename should have format [name...]_[cols]x[rows].[fmt]", MessageType.Info);
            if (lmv.activeMediaType == LeiaMediaViewer.LeiaMediaType.Video)
            {
                EditorGUILayout.HelpBox("Make sure that video files are transcoded or they may not display in builds."
                    +" To do this, click on the video file in the Unity \"Project\" window and in the inspector check the \"Transcode\" checkbox.", MessageType.Info);
            }

            // display several properties using same style
            SerializedProperty[] leiaMediaProperties = new [] { leiaMediaTexture, leiaMediaVideoClip, leiaMediaVideoURL };
            bool[] leiaMediaPropertyUpdated = new bool[leiaMediaProperties.Length];
            for (int i = 0; i < leiaMediaProperties.Length; i++)
            {
                // show LeiaMedia properties, and record changes in their values
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(leiaMediaProperties[i]);
                leiaMediaPropertyUpdated[i] = EditorGUI.EndChangeCheck();

                if (leiaMediaPropertyUpdated[i])
                {
                    // if user updated the ith LeiaMedia property to a new value
                    lmv.activeMediaTypeInt = i;
                    serializedObject.FindProperty("_activeMediaType").enumValueIndex = i;
                }
            }
 

            if (GUILayout.Button("Move to Convergence Plane") && lmv != null)
            {
                lmv.ProjectOntoZDP();
            }

            // newer style for newer properties - UndoableInputFieldUtils
            // in addition to changing LeiaMedia URL/video/texture, allow users to change cols / rows
            UndoableInputFieldUtils.ImmediateIntField(
                // get
                () => lmv.GetLeiaMediaColsFor(lmv.activeMediaType),
                // set
                (int val) => { lmv.SetLeiaMediaColsFor(lmv.activeMediaType, val); EditorUtility.SetDirty(target); },
                // label
                string.Format(propertyStringResources[(int)propertyStringIDs.LeiaMediaCol][1], lmv.activeMediaType),
                serializedObject.targetObject);

            UndoableInputFieldUtils.ImmediateIntField(
                // get
                () => lmv.GetLeiaMediaRowsFor(lmv.activeMediaType),
                // set
                (int val) => { lmv.SetLeiaMediaRowsFor(lmv.activeMediaType, val); EditorUtility.SetDirty(target); },
                // label
                string.Format(propertyStringResources[(int)propertyStringIDs.LeiaMediaRow][1], lmv.activeMediaType),
                serializedObject.targetObject);


            UndoableInputFieldUtils.EnumFieldWithTooltip(
              () => { return lmv.mediaScaleMode; },
              (System.Enum value) => { lmv.mediaScaleMode = (LeiaMediaViewer.MediaScaleMode)value; },
              "Media Scale Mode", "World XYZ - behave as any other object in the scene: respects transform and perspective distortion . OnscreenPercent - use screen coordinates with given scale and offset percentage.", lmv);

            if (lmv.mediaScaleMode == LeiaMediaViewer.MediaScaleMode.OnscreenPercent)
            {
                EditorGUI.BeginChangeCheck();
                // display UI for onscreenPercent rect; and allow user to interact with onscreenPercent rect
                EditorGUILayout.PropertyField(onscreenPercent);
                bool changed = EditorGUI.EndChangeCheck();

                if (changed)
                {
                    // have to trigger the property setter so that LeiaMediaViewer can trigger Rebuild. So this property gets its UI displayed, and user can write into UI at that time
                    lmv.OnscreenPercent = onscreenPercent.rectValue;

                    // later, we Apply Modified Properties from serializedObject back onto LMV; but since lmv.OnscreenPercent is already up-to-date with serializedObject property, this is not an issue. Just a redundant reassignment
                }
            }

            serializedObject.ApplyModifiedProperties();

            // if we detected a change in LeiaMedia property, then after applying property update we should also update cols / rows
            for (int i = 0; i < leiaMediaProperties.Length; i++)
            {
                // if user updated a URL/texture/video property
                if (leiaMediaPropertyUpdated[i])
                {
                    string filename = lmv.ActiveLeiaMediaName;
                    int cols = 0, rows = 0;
                    bool parsed = !string.IsNullOrEmpty(filename) && StringExtensions.TryParseColsRowsFromFilename(filename, out cols, out rows);

                    // if we could parse cols and rows from the LeiaMedia name
                    if (parsed)
                    {
                        // then set cols and rows on LeiaMediaViewer
                        lmv.SetLeiaMediaColsFor(lmv.activeMediaType, cols);
                        lmv.SetLeiaMediaRowsFor(lmv.activeMediaType, rows);
                    }
                }
            }
        }
    }
}
