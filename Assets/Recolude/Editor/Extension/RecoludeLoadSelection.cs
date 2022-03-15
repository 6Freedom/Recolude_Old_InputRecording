using UnityEngine;
using UnityEditor;
using System.IO;
using RecordAndPlay;
using RecordAndPlay.IO;
using RecordAndPlay.Editor.Extension.Import;
using System.Globalization;
using System.Collections.Generic;

namespace Recolude.Editor.Extension
{
    class RecoludeLoadSelection : ILoadSelection
    {

        enum ImportMethod
        {
            RAP,
            ScriptableObject,
            Both
        }

        List<V1Recording> recordings;

        RecoludeConfig config;

        ImportMethod methodForImporting;

        public RecoludeLoadSelection(RecoludeConfig rec, List<V1Recording> recs)
        {
            config = rec;
            recordings = recs;
            methodForImporting = ImportMethod.ScriptableObject;
        }

        public string Error()
        {
            return "";
        }

        public void Import(string dir)
        {
            var recordingsSaved = 0;
            var importHalted = false;
            try
            {
                foreach (var rec in recordings)
                {
                    var downloadRequest = config.BuildDownloadRecordingRequest(rec.id);
                    downloadRequest.SendWebRequest();

                    string progressbarMessage = string.Format("Currently downloading {0}. {1} out of {2} recordings saved", rec.name, recordingsSaved, recordings.Count);

                    while (downloadRequest.isDone == false)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Downloading Recordings", progressbarMessage, downloadRequest.downloadProgress))
                        {
                            EditorUtility.ClearProgressBar();
                            importHalted = true;
                            break;
                        };
                    }
                    if (importHalted)
                    {
                        break;
                    }

                    var dateString = rec.CreatedAt.ToUniversalTime().ToString("dd-MM-yy HH.mm.ss", DateTimeFormatInfo.InvariantInfo);
                    var fileName = string.Format("{0} {1}", string.IsNullOrEmpty(rec.name) ? "[no name]" : rec.name, dateString);
                    var rapBytes = downloadRequest.downloadHandler.data;

                    if (methodForImporting == ImportMethod.Both || methodForImporting == ImportMethod.RAP)
                    {
                        var rapfileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, fileName + ".rap"));
                        File.WriteAllBytes(rapfileName, rapBytes);
                    }

                    if (methodForImporting == ImportMethod.Both || methodForImporting == ImportMethod.ScriptableObject)
                    {
                        using (MemoryStream ms = new MemoryStream(rapBytes))
                        {
                            var recordings = Unpackager.Unpackage(ms);
                            foreach (var record in recordings)
                            {
                                var assetfileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, fileName + ".asset"));
                                AssetDatabase.CreateAsset(record, assetfileName);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }

                    AssetDatabase.Refresh();
                    recordingsSaved++;
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("Error: " + e.Message);
                throw e;
            }

            EditorUtility.ClearProgressBar();
        }

        public void Render()
        {
            EditorGUILayout.LabelField(string.Format("Import {0} recordings from recolude", recordings.Count));
            methodForImporting = (ImportMethod)EditorGUILayout.EnumPopup("Import Method:", methodForImporting);
        }

        public string SuggestedFolderName()
        {
            return "Recolude Downloads";
        }
    }

}