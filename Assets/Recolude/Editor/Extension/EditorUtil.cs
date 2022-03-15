using UnityEngine;
using UnityEditor;


/// <summary>
/// All classes for extending the unity editor. You shouldn't care about this unless you want to change how Record and Play works.
/// </summary>
namespace Recolude.Editor.Extension
{

    /// <summary>
    /// A Utility class for common functions for extending the editor.
    /// </summary>
    public static class EditorUtil
    {

        public static RecoludeConfig RenderAPIConfigControls(RecoludeConfig config)
        {
            var newConfig = EditorGUILayout.ObjectField("Recolude Config", config, typeof(RecoludeConfig), true) as RecoludeConfig;
            if (newConfig == null && GUILayout.Button("Create New Recolude Config"))
            {
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/Recolude Config.asset");
                newConfig = RecoludeConfig.CreateInstance("", "");
                AssetDatabase.CreateAsset(newConfig, assetPathAndName);
                Selection.activeObject = newConfig;
                EditorGUIUtility.PingObject(newConfig);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return newConfig;
        }

    }

}