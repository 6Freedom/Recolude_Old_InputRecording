using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using RecordAndPlay;
using RecordAndPlay.IO;

namespace Recolude.RecordingStorage
{
    public class WebDeleteRecordingOperation : IDeleteRecordingOperation
    {

        UnityWebRequest webRequest;

        bool finished;

        string error;

        public WebDeleteRecordingOperation(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
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


        public IEnumerator Run()
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                this.error = webRequest.error;
                this.finished = true;
                yield break;
            }
           
            this.finished = true;
        }
    }

}