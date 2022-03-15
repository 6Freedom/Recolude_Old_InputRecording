using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using RecordAndPlay;
using RecordAndPlay.IO;

namespace Recolude.RecordingStorage
{
    public class WebLoadRecordingOperation : ILoadRecordingOperation
    {
        Recording recording;

        UnityWebRequest webRequest;

        bool finished;

        string error;

        public WebLoadRecordingOperation(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
            this.recording = null;
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

        public Recording Recording()
        {
            return recording;
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

            if (webRequest.downloadHandler.data.Length == 0)
            {
                this.error = "File download was empty";
                this.finished = true;
                yield break;
            }

            Recording[] recs = Unpackager.Unpackage(new MemoryStream(webRequest.downloadHandler.data));
            if (recs.Length == 0)
            {
                this.error = "no recordings found when loading";
                this.finished = true;
                yield break;
            }
            this.recording = recs[0];
            this.finished = true;
        }
    }

}