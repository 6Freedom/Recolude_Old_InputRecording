using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using RecordAndPlay;

namespace Recolude.Editor.Extension
{

    /// <summary>
    /// Window for assisting in the exportation of recordings to recolude online service.
    /// </summary>
    public class ExportToRecoludeWindow : EditorWindow
    {
        List<Recording> recordingsToExport;

        List<RecordingVisibility> recordingsToExportVisibility;

        RecoludeConfig recoludeConfig = null;

        public static ExportToRecoludeWindow Init(params Recording[] recordingToExport)
        {
            ExportToRecoludeWindow window = (ExportToRecoludeWindow)GetWindow(typeof(ExportToRecoludeWindow));
            window.recordingsToExport = new List<Recording>();
            window.recordingsToExport.AddRange(recordingToExport);
            window.recoludeConfig = null;
            window.Show();
            window.Repaint();
            return window;
        }

        public static ExportToRecoludeWindow Init(RecoludeConfig recoludeConfig)
        {
            ExportToRecoludeWindow window = (ExportToRecoludeWindow)GetWindow(typeof(ExportToRecoludeWindow));
            window.recordingsToExport = new List<Recording>();
            window.recoludeConfig = recoludeConfig;
            window.Show();
            window.Repaint();
            return window;
        }

        [MenuItem("Window/Record And Play/Upload Recordings")]
        public static ExportToRecoludeWindow Init()
        {
            ExportToRecoludeWindow window = (ExportToRecoludeWindow)GetWindow(typeof(ExportToRecoludeWindow));
            window.recordingsToExport = new List<Recording>();
            window.recoludeConfig = null;
            window.Show();
            window.Repaint();
            return window;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Export To Recolude");
        }

        void RenderRecordingsToUploadList()
        {
            EditorGUILayout.LabelField("Recordings To Upload");
            if (recordingsToExport == null)
            {
                recordingsToExport = new List<Recording>();
            }
            for (int i = recordingsToExport.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                recordingsToExport[i] = EditorGUILayout.ObjectField(recordingsToExport[i], typeof(Recording), false) as Recording;
                if (GUILayout.Button("Remove"))
                {
                    recordingsToExport.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Recording"))
            {
                recordingsToExport.Add(null);
            }
        }



        void OnGUI()
        {
            EditorGUILayout.Space();
            recoludeConfig = EditorUtil.RenderAPIConfigControls(recoludeConfig);
            EditorGUILayout.Space();

            RenderRecordingsToUploadList();

            string error = Error();
            if (error != "")
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Export"))
            {
                Export(recordingsToExport);
            }
        }

        private void Export(List<Recording> recordings)
        {
            if (recordings == null || recordings.Count == 0)
            {
                throw new System.Exception("Can't export null or empty recordings.");
            }
            if (recoludeConfig == null)
            {
                throw new System.Exception("Can not upload recordings without API configuration");
            }

            int numOfRecordingsToUpload = 0;
            foreach (var rec in recordings)
            {
                if (rec == null)
                {
                    continue;
                }
                numOfRecordingsToUpload++;
            }

            int numOfRecordingsUploaded = 0;
            try
            {
                foreach (var rec in recordings)
                {
                    if (rec == null)
                    {
                        continue;
                    }
                    EditorUtility.DisplayProgressBar(
                        "Uploading Recording",
                        string.Format("{0} out of {1} recordings uploaded", numOfRecordingsUploaded, numOfRecordingsToUpload),
                        numOfRecordingsUploaded / (float)(numOfRecordingsToUpload)
                    );

                    // var req = recoludeConfig.SaveRecording(rec);
                    // EditorUtil.RunCoroutine(req.Run());

                    var req = recoludeConfig.BuildUploadRecordingRequest(rec, RecordingVisibility.Public).SendWebRequest();
                    while(req.isDone == false ) {
                        continue;
                    }
                    Debug.Log(req.webRequest.url);
                    Debug.Log(req.webRequest.error);
                    Debug.Log(req.webRequest.responseCode);

                    numOfRecordingsUploaded++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }


        }

        private string Error()
        {
            if (recordingsToExport == null || recordingsToExport.Count == 0)
            {
                return "Please provide recordings to upload";
            }

            if (recoludeConfig == null)
            {
                return "Provide a recolude config";
            }

            return "";
        }

    }

}