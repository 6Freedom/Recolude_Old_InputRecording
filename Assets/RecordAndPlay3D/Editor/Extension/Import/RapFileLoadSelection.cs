using UnityEngine;
using UnityEditor;
using System.IO;
using RecordAndPlay.IO;

namespace RecordAndPlay.Editor.Extension.Import
{
    class RapFileLoadSelection : ILoadSelection
    {
        string path;

        int numberRecordings;

        string error;

        public RapFileLoadSelection(string path)
        {
            this.path = path;
            error = ExaminePath(path);
        }

        private string ExaminePath(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return "Please provide a filepath";
            }
            try
            {
                using (FileStream fs = File.OpenRead(p))
                {
                    numberRecordings = Unpackager.Peak(fs);
                }
            }
            catch (System.Exception e)
            {
                numberRecordings = -1;
                return e.ToString();
            }
            return null;
        }

        public string SuggestedFolderName()
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public void Render()
        {
            EditorGUILayout.LabelField("File", path);
            if (GUILayout.Button((string.IsNullOrEmpty(path) ? "Select" : "Change") + " Recordings To Load"))
            {
                this.path = EditorUtility.OpenFilePanelWithFilters("Load Recordings", "", new string[] { "FileType", "rap" });
                error = ExaminePath(path);
            }
            if (HasError())
            {
                return;
            }
            EditorGUILayout.LabelField("Recordings", numberRecordings.ToString());
        }

        public string Error()
        {
            return error;
        }

        public bool HasError()
        {
            return error != "";
        }

        public void Import(string dir)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                var recordings = Unpackager.Unpackage(fs);
                foreach (var record in recordings)
                {
                    RecordAndPlay.Editor.RecordingUtil.SaveToAssets(record, record.RecordingName, dir);
                }
            }
        }
    }

}