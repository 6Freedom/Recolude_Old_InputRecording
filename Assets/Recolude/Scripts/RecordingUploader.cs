using UnityEngine;

namespace Recolude
{

    /// <summary>
    /// For Internal use by Recolude only. A hack for allowing scriptable 
    /// objects to run coroutines.
    /// </summary>
    [System.Serializable]
    internal class RecordingUploader : MonoBehaviour
    {
        private static RecordingUploader instance;

        internal static RecordingUploader GetInstance()
        {
            if (instance == null)
            {
                instance = new GameObject("Recording Uploader").AddComponent<RecordingUploader>();
            }
            return instance;
        }

    }

}