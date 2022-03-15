using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using RecordAndPlay;

namespace Recolude.Editor.Extension
{

    /// <summary>
    /// Window for assisting in the search of recordings hosted on recolude's online service.
    /// </summary>
    public class SearchRecoludeWindow : EditorWindow
    {
        RecoludeConfig recoludeConfig = null;

        bool[] selectedRecordingsFromSearchResults;

        RecordingService.ListRecordingsUnityWebRequest listRecordingsUnityWebRequest;

        public static SearchRecoludeWindow Init(RecoludeConfig recoludeConfig)
        {
            SearchRecoludeWindow window = (SearchRecoludeWindow)GetWindow(typeof(SearchRecoludeWindow));
            window.recoludeConfig = recoludeConfig;
            window.Show();
            window.Repaint();
            window.position = RecordAndPlay.Editor.Extension.EditorUtil.CenterWindowPosition(600, 400);
            return window;
        }

        [MenuItem("Window/Record And Play/Search Recordings")]
        public static SearchRecoludeWindow Init()
        {
            SearchRecoludeWindow window = (SearchRecoludeWindow)GetWindow(typeof(SearchRecoludeWindow));
            window.recoludeConfig = null;
            window.Show();
            window.Repaint();
            window.position = RecordAndPlay.Editor.Extension.EditorUtil.CenterWindowPosition(600, 400);
            return window;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Search Recolude");
        }


        private int NumberOfItemsSelected()
        {
            if (selectedRecordingsFromSearchResults == null || listRecordingsUnityWebRequest == null || listRecordingsUnityWebRequest.success == null)
            {
                return 0;
            }

            int total = 0;
            foreach (var selected in selectedRecordingsFromSearchResults)
            {
                if (selected)
                {
                    total++;
                }
            }
            return total;
        }

        private List<V1Recording> ItemsSelected()
        {
            var result = new List<V1Recording>();
            if (selectedRecordingsFromSearchResults == null || listRecordingsUnityWebRequest.success == null)
            {
                return result;
            }

            for (int i = 0; i < listRecordingsUnityWebRequest.success.recordings.Length; i++)
            {
                if (selectedRecordingsFromSearchResults[i])
                {
                    result.Add(listRecordingsUnityWebRequest.success.recordings[i]);
                }
            }
            return result;
        }

        private void DisplaySearchResults(RecoludeConfig config, V1RecordingsResponse searchResultsToDisplay)
        {
            if (searchResultsToDisplay.recordings.Length == 0)
            {
                GUILayout.Label("No results");
                return;
            }

            float widthOfACell = position.width / 4.0f;
            var widthLayoutOption = GUILayout.Width(widthOfACell);
            var lastItemWidthLayoutOption = GUILayout.Width(widthOfACell - 20);
            var halfWidthLayoutOption = GUILayout.Width(widthOfACell / 2f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name", widthLayoutOption);
            GUILayout.Label("Uploaded", widthLayoutOption);
            GUILayout.Label("Duration", halfWidthLayoutOption);
            GUILayout.Label("Subjects", halfWidthLayoutOption);
            EditorGUILayout.EndHorizontal();

            int i = 0;
            foreach (var result in searchResultsToDisplay.recordings)
            {
                EditorGUILayout.BeginHorizontal();
                selectedRecordingsFromSearchResults[i] = EditorGUILayout.ToggleLeft(
                    searchResultsToDisplay.recordings[i].name,
                    selectedRecordingsFromSearchResults[i],
                    widthLayoutOption
                );
                GUILayout.Label(result.summary.CreatedAt.ToUniversalTime().ToString(), widthLayoutOption);
                GUILayout.Label(result.summary.duration.ToString("#.##"), halfWidthLayoutOption);
                GUILayout.Label(result.summary.subjects.ToString(), halfWidthLayoutOption);
                if (GUILayout.Button("View", lastItemWidthLayoutOption))
                {
                    var rec = config.LoadRecording(result.id);
                    RecordAndPlay.Editor.Extension.EditorUtil.RunCoroutine(rec.Run());
                    while (rec.Finished() == false) { }
                    if (string.IsNullOrEmpty(rec.Error()) == false)
                    {
                        Debug.LogErrorFormat("Error fetching recording: {0}", rec.Error());
                        return;
                    }
                    RecordAndPlay.Editor.Extension.PlaybackWindow.
                        Init().
                        SetRecordingForPlayback(rec.Recording());
                }
                EditorGUILayout.EndHorizontal();
                i++;
            }
        }

        bool interpretedRequest = false;

        private void ExecuteSearch()
        {
            interpretedRequest = false;
            var rs = new RecordingService(recoludeConfig);
            listRecordingsUnityWebRequest = rs.ListRecordings(new RecordingService.ListRecordingsRequestParams()
            {
                ProjectId = recoludeConfig.GetProjectID(),
            });
            listRecordingsUnityWebRequest.UnderlyingRequest.SendWebRequest();
            // EliCDavis.RecordAndPlay.Editor.Extension.EditorUtil.StartBackgroundTask(listRecordingsUnityWebRequest.Run());
            // searchResult = req.success;
            // if (searchResult != null && searchResult.recordings != null)
            // {
            //     selectedRecordingsFromSearchResults = new bool[searchResult.recordings.Length];
            // }
            // else
            // {
            //     selectedRecordingsFromSearchResults = null;
            // }
        }


        void OnGUI()
        {
            EditorGUILayout.Space();
            recoludeConfig = EditorUtil.RenderAPIConfigControls(recoludeConfig);
            EditorGUILayout.Space();

            if (listRecordingsUnityWebRequest != null)
            {
                // if (listRecordingsUnityWebRequest.UnderlyingRequest.error)
                // {
                //     EditorGUILayout.HelpBox("searching " + listRecordingsUnityWebRequest.UnderlyingRequest.url, MessageType.Info);
                // }
                if (listRecordingsUnityWebRequest.UnderlyingRequest.isDone == false)
                {
                    var dots = ".";
                    if (EditorApplication.timeSinceStartup % 2 == 0)
                    {
                        dots = "..";
                    }
                    else if (EditorApplication.timeSinceStartup % 3 == 0)
                    {
                        dots = "...";
                    }
                    EditorGUILayout.HelpBox("searching" + dots, MessageType.Info);
                }
                if (listRecordingsUnityWebRequest.UnderlyingRequest.isNetworkError)
                {
                    EditorGUILayout.HelpBox("network error!", MessageType.Error);
                }

                if (interpretedRequest == false && listRecordingsUnityWebRequest.UnderlyingRequest.isDone)
                {
                    interpretedRequest = true;
                    listRecordingsUnityWebRequest.Interpret(listRecordingsUnityWebRequest.UnderlyingRequest);
                    selectedRecordingsFromSearchResults = new bool[listRecordingsUnityWebRequest.success.recordings.Length];
                }
            }


            string error = Error();
            if (error != "")
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }

            if (GUILayout.Button("Search"))
            {
                ExecuteSearch();
            }

            EditorGUILayout.Space();
            if (listRecordingsUnityWebRequest != null && listRecordingsUnityWebRequest.success != null)
            {
                DisplaySearchResults(recoludeConfig, listRecordingsUnityWebRequest.success);
            }

            if (listRecordingsUnityWebRequest != null && listRecordingsUnityWebRequest.fallbackResponse != null)
            {
                EditorGUILayout.HelpBox(listRecordingsUnityWebRequest.fallbackResponse.message, MessageType.Error);
            }

            if (NumberOfItemsSelected() > 0)
            {
                EditorGUILayout.BeginHorizontal();

                if (
                    GUILayout.Button("Delete") &&
                    EditorUtility.DisplayDialog(
                        "Delete Recordings From Recolude",
                        string.Format("Are you sure you want to permanently delete {0} recordings from recolude?", NumberOfItemsSelected()),
                        "Delete",
                        "Cancel"))
                {
                    var recordingsToDelete = ItemsSelected();
                    var recordingsDeleted = 0;
                    try
                    {
                        foreach (var rec in recordingsToDelete)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar(
                                "Deleting Recording",
                                string.Format("Currently deleting {0}. {1} out of {2} recordings deleted", rec.name, recordingsDeleted, recordingsToDelete.Count),
                                recordingsDeleted / (float)(recordingsToDelete.Count)
                            ))
                            {
                                EditorUtility.ClearProgressBar();
                                break;
                            };

                            var req = recoludeConfig.DeleteRecording(rec.id);
                            RecordAndPlay.Editor.Extension.EditorUtil.RunCoroutine(req.Run());
                            recordingsDeleted++;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                        EditorUtility.ClearProgressBar();
                    }

                    EditorUtility.ClearProgressBar();
                    ExecuteSearch();
                }

                // if (GUILayout.Button("Import"))
                // {
                //     EliCDavis.RecordAndPlay.Editor.Extension.Import.ImportWindow.Init(recoludeConfig, ItemsSelected());
                // }

                EditorGUILayout.EndHorizontal();
            }
        }

        private string Error()
        {
            if (recoludeConfig == null)
            {
                return "Provide a recolude config";
            }

            return "";
        }

    }

}