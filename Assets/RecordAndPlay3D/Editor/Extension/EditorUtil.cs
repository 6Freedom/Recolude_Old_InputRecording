using UnityEngine;
using UnityEditor;


/// <summary>
/// All classes for extending the unity editor. You shouldn't care about this unless you want to change how Record and Play works.
/// </summary>
namespace RecordAndPlay.Editor.Extension
{

    /// <summary>
    /// A Utility class for common functions for extending the editor.
    /// </summary>
    public static class EditorUtil
    {

        public static Rect CenterWindowPosition(int windowWidth, int windowHeight)
        {
            return new Rect(
                (Screen.currentResolution.width / 2) - (windowWidth / 2),
                (Screen.currentResolution.height / 2) - (windowHeight / 2),
                windowWidth,
                windowHeight
            );
        }

        public static void RenderMultipleRecordingsInfo(Recording[] recordings)
        {
            if (recordings == null)
            {
                throw new System.Exception("Can't render null recordings!");
            }

            EditorGUILayout.LabelField("Number Of Recordings", recordings.Length.ToString());
            EditorGUILayout.LabelField("Total Duration", string.Format("{0:0.00} seconds", Duration(recordings)));
            EditorGUILayout.LabelField("Total Subjects", NumberSubjects(recordings).ToString());
            EditorGUILayout.LabelField("Total Custom Events", NumberCustomEvents(recordings).ToString());

        }

        public static void RenderSingleRecordingInfo(Recording recording)
        {
            if (recording == null)
            {
                throw new System.Exception("Can't render a null recording!");
            }

            EditorGUILayout.LabelField("Name", recording.RecordingName.ToString());
            EditorGUILayout.LabelField("Duration", string.Format("{0:0.00} seconds", Duration(recording)));
            EditorGUILayout.LabelField("Subjects", NumberSubjects(recording).ToString());
            EditorGUILayout.LabelField("Custom Events", NumberCustomEvents(recording).ToString());
        }

        public static int NumberCustomEvents(params Recording[] recordings)
        {
            if (recordings == null)
            {
                return 0;
            }

            int sum = 0;
            foreach (var recording in recordings)
            {
                sum += recording.CapturedCustomEvents == null ? 0 : recording.CapturedCustomEvents.Length;
                foreach (var subject in recording.SubjectRecordings)
                {
                    sum += subject.CapturedCustomEvents == null ? 0 : subject.CapturedCustomEvents.Length;
                }
            }
            return sum;
        }

        /// <summary>
        /// A hack to run and block until a coroutine finishes. Use at your own discretion.
        /// </summary>
        /// <param name="enumerator">coroutine to run on.</param>
        public static void RunCoroutine(System.Collections.IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == null)
                {
                    continue;
                }

                var asyncOpp = enumerator.Current as AsyncOperation;
                if (asyncOpp != null)
                {
                    while (asyncOpp.isDone == false)
                    {

                    }
                }
            }
        }


        private static int NumberSubjects(params Recording[] recordings)
        {
            if (recordings == null)
            {
                return 0;
            }

            int sum = 0;
            foreach (var recording in recordings)
            {
                sum += recording.SubjectRecordings == null ? 0 : recording.SubjectRecordings.Length;
            }
            return sum;
        }

        private static float Duration(params Recording[] recordings)
        {
            if (recordings == null)
            {
                return 0;
            }

            float sum = 0;
            foreach (var recording in recordings)
            {
                sum += recording.GetDuration();
            }
            return sum;
        }

    }

}