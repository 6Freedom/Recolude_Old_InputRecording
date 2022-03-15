using UnityEngine;
using UnityEditor;

namespace Recolude.Editor.Extension
{

    [CustomEditor(typeof(RecoludeConfig))]
    public class ServerConfigEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            if (targets == null)
            {
                return;
            }

            if (targets.Length == 1)
            {
                RenderSingleConfig((RecoludeConfig)target);
            }
        }

        void UploadGUI(RecoludeConfig config)
        {
            if (GUILayout.Button("Upload"))
            {
                ExportToRecoludeWindow.Init(config);
            }
        }

        public void RenderSingleConfig(RecoludeConfig config)
        {
            EditorGUILayout.Space();

            if (config.SetAPIKey(EditorGUILayout.TextField("API Key", config.GetAPIKey())))
            {
                EditorUtility.SetDirty(config);
            }

            if (config.SetProjectID(EditorGUILayout.TextField("Project ID", config.GetProjectID())))
            {
                EditorUtility.SetDirty(config);
            }

            if (config.SetDefaultUploadVisibility((RecordingVisibility)EditorGUILayout.EnumPopup("Default Recording Upload Visibility", config.DefaultUploadVisibility())))
            {
                EditorUtility.SetDirty(config);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Search"))
            {
                SearchRecoludeWindow.Init(config);
            }
            UploadGUI(config);
            EditorGUILayout.EndHorizontal();
        }

    }

}