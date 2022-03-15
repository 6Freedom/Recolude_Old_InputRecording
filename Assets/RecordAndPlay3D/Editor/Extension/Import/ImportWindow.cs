using UnityEngine;
using UnityEditor;
using System.IO;

namespace RecordAndPlay.Editor.Extension.Import
{

    /// <summary>
    /// Window for assisting in the importing of recordings from binary format.
    /// </summary>
    public class ImportWindow : EditorWindow
    {

        ILoadSelection loadSelection = null;

        string folderName = "";

        public static ImportWindow Init(string filePath)
        {
            ImportWindow window = (ImportWindow)GetWindow(typeof(ImportWindow));
            window.loadSelection = new RapFileLoadSelection(filePath);
            window.folderName = window.loadSelection.SuggestedFolderName();
            window.Show();
            window.Repaint();
            return window;
        }

        // public static ImportWindow Init(RecoludeConfig rec, List<global::Recolude.Resources.V1.Recording> recs)
        // {
        //     ImportWindow window = (ImportWindow)GetWindow(typeof(ImportWindow));
        //     window.loadSelection = new RecoludeLoadSelection(rec, recs);
        //     window.folderName = window.loadSelection.SuggestedFolderName();
        //     window.Show();
        //     window.Repaint();
        //     return window;
        // }


        [MenuItem("Window/Record And Play/Import Recordings")]
        public static ImportWindow Init()
        {
            return Init("");
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Import Recordings");
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            if (loadSelection == null)
            {
                this.loadSelection = new RapFileLoadSelection("");
                this.folderName = this.loadSelection.SuggestedFolderName();
            }

            folderName = EditorGUILayout.TextField("Folder Name", folderName);

            loadSelection.Render();

            EditorGUILayout.Space();

            string error = Error();
            if (string.IsNullOrEmpty(error) == false)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }

            if (GUILayout.Button("Import"))
            {
                var dir = Path.Combine("Assets", folderName);
                Directory.CreateDirectory(dir);
                loadSelection.Import(dir);
                AssetDatabase.Refresh();
            }
        }

        private string Error()
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return "Please provide a folder name to store the recordings";
            }

            if (Directory.Exists(Path.Combine("Assets", folderName)))
            {
                return "Folder name is already taken";
            }

            return loadSelection.Error();
        }

    }

}