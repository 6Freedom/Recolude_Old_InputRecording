using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SixFreedom
{
    public class Renamer : EditorWindow
    {
        string find = "";
        string replace = "";

        [MenuItem("Window/6freedom tools/Renamer")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            Renamer window = (Renamer) GetWindow(typeof(Renamer));
            window.name = "Renamer";
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Find & Replace for selected GameObjects", EditorStyles.boldLabel);
            GUILayout.Space(5);

            find = EditorGUILayout.TextField("Find", find);
            replace = EditorGUILayout.TextField("Replace", replace);

            Dictionary<GameObject, string> currentNames = new Dictionary<GameObject, string>();
            Dictionary<GameObject, string> newNames = GetNewNames(currentNames);


            if (GUILayout.Button("Replace"))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    if (go.name.Contains(find))
                        currentNames.Add(go, go.name);
                }
                Replace(GetNewNames(currentNames));
            }

            if (GUILayout.Button("Replace with childrens"))
            {
                foreach (Transform t in Selection.GetTransforms(SelectionMode.Deep))
                {
                    if (t.name.Contains(find))
                        currentNames.Add(t.gameObject, t.name);
                }
                Replace(GetNewNames(currentNames));
            }
        }

        private Dictionary<GameObject, string> GetNewNames(Dictionary<GameObject, string> _currentNames)
        {
            Dictionary<GameObject, string> newNames = new Dictionary<GameObject, string>(_currentNames);
            foreach (var keyValuePair in _currentNames)
            {
                newNames[keyValuePair.Key] = _currentNames[keyValuePair.Key].Replace(find, replace);
            }
            return newNames;
        }

        private void Replace(Dictionary<GameObject, string> newNames)
        {
            string message = "Are you sure you want to rename \n";

            foreach (var kv in newNames)
            {
                message += $"{kv.Key.name} into {newNames[kv.Key]} \n";
            }

            if (EditorUtility.DisplayDialog($"You will rename {newNames.Count} GameObjects.",
            message,
            "Rename", "Don't rename"))
            {
                foreach (var kv in newNames)
                {
                    kv.Key.name = kv.Value;
                }
            }
        }
    }
}