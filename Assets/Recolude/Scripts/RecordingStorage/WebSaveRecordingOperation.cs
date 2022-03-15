using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

namespace Recolude.RecordingStorage
{
    public class WebSaveRecordingOperation : ISaveRecordingOperation
    {
        string recordingID;

        UnityWebRequest webRequest;

        bool finished;

        string error;

        public WebSaveRecordingOperation(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
            this.recordingID = null;
            this.finished = false;
            this.error = null;
        }

        public string Error()
        {
            return error;
        }

        public bool Finished()
        {
            return finished;
        }

        public string RecordingID()
        {
            return recordingID;
        }

        public IEnumerator Run()
        {
            yield return webRequest.SendWebRequest();
            var summary = JsonUtility.FromJson<V1RecordingSummary>(webRequest.downloadHandler.text);
            recordingID = summary.id;
            finished = true;
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                this.error = webRequest.error;
            }
        }
        
    }

}