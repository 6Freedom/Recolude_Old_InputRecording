using UnityEngine;
using UnityEditor;

using System.IO;
using System.Text;

using RecordAndPlay.Util;

namespace RecordAndPlay.Editor.Extension
{

    /// <summary>
    /// Window for assisting in the exportation of recordings to csv format.
    /// </summary>
    public class ExportCSVWindow : EditorWindow
    {
        Recording[] recordingsToExport;

        string path = "";

        string folderName = "";

        public static ExportCSVWindow Init(params Recording[] recordingToExport)
        {
            ExportCSVWindow window = (ExportCSVWindow)GetWindow(typeof(ExportCSVWindow));
            window.recordingsToExport = recordingToExport;
            window.folderName = recordingToExport.Length == 1 ? recordingToExport[0].name : "Recordings";
            window.Show();
            window.Repaint();
            return window;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Export CSV");
        }

        void OnGUI()
        {
            if (recordingsToExport == null || recordingsToExport.Length == 0)
            {
                return;
            }

            if (path != "")
            {
                EditorGUILayout.LabelField("Location", path);
            }

            if (GUILayout.Button("Select Location To Save Recordings"))
            {
                path = EditorUtility.SaveFolderPanel("Export Recording As CSV", "", "");
            }

            if (path != "")
            {
                folderName = EditorGUILayout.TextField("Folder Name", folderName);
            }

            string error = Error();

            if (error != "")
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }

            if (GUILayout.Button("Export"))
            {
                Export(Path.Combine(path, folderName), recordingsToExport);
            }
        }

        private void Export(string dir, Recording[] recordings)
        {
            if (recordings == null || recordings.Length == 0)
            {
                throw new System.Exception("Can't export null or empty recordings.");
            }

            Directory.CreateDirectory(dir);

            if (recordings.Length == 1)
            {
                ExportSingle(dir, recordings[0]);
            }
            else
            {
                ExportMultiple(dir, recordings);
            }
        }

        private void ExportSingle(string dir, Recording recording)
        {
            var pages = recording.ToCSV();

            foreach (var p in pages)
            {
                File.WriteAllText(Path.Combine(dir, p.GetName() + ".csv"), p.ToString());
            }

            EditorUtility.RevealInFinder(dir);
        }

        private void ExportMultiple(string dir, Recording[] recordings)
        {
            var pages = recordings.ToCSV();

            foreach (var p in pages)
            {
                File.WriteAllText(Path.Combine(dir, p.GetName() + ".csv"), p.ToString());
            }

            EditorUtility.RevealInFinder(dir);
        }

        private string Error()
        {
            if (path == "")
            {
                return "A location must be set for saving csv data";
            }
            else if (folderName == "")
            {
                return "Please provide a name to represent the csv data";
            }

            if (Directory.Exists(Path.Combine(path, folderName)))
            {
                return "Folder name is already taken";
            }

            return "";
        }

    }

}