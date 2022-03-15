using UnityEngine;

using UnityEngine.Networking;
using System.IO;
using Recolude.RecordingStorage;
using RecordAndPlay;
using RecordAndPlay.IO;

/// <summary>
/// Responsible for interfacing with the <a href="https://recolude.com/">Recolude Service</a> for saving, retrieving, and performing analytics on recordings made in game.
/// </summary>
namespace Recolude
{

    /// <summary>
    /// Stores credentials that can be used to access Recolude's different services. 
    /// This config can be instantiated at runtime, but an easier way of 
    /// keeping up with credentials to your application might be to instantiate
    /// it right in the Assets folder in your project. You can right-click 
    /// anywhere in your project’s Asset folder and select “Create > Recolude > Config”.
    /// ![creating config](/images/creating_recolude_config.png)
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(menuName = "Recolude/Config", fileName = "RecoludeConfig")]
    public class RecoludeConfig : ScriptableObject, ISerializationCallbackReceiver, IRecordingStorage, Config
    {

        private readonly static string fileserverProtocol = "https";

        private readonly static string url = "api.recolude.com";

        private readonly static int fileserverPort = 443;

        [SerializeField]
        private string apiKey;

        [SerializeField]
        private string projectID;

        [SerializeField]
        private RecordingVisibility defaultUploadVisibility;

        /// <summary>
        /// The URL we will be targeting for making our calls to Recolude
        /// </summary>
        /// <value></value>
        public string BasePath => string.Format("{0}://{1}", fileserverProtocol, url);

        /// <summary>
        /// API Key generated on recolude.com that has had specific permissions attached to it.
        /// </summary>
        public string ApiKeyAuth => apiKey;

        /// <summary>
        /// Cognito auth token if the user chooses to sign into their account inside a unity application
        /// </summary>
        public string CognitoAuth => "";

        /// <summary>
        /// Personal Development key for managing cloud Recolude configurations inside a unity application.
        /// </summary>
        public string DevKeyAuth => "";

        /// <summary>
        /// Constructs an instance of the Recolude config scriptable object.
        /// </summary>
        /// <param name="apiKey">API key that will be used when making calls to Recolude</param>
        /// <param name="projectID">ID of the project we want to interface with inside of Recolude.</param>
        /// <returns></returns>
        public static RecoludeConfig CreateInstance(string apiKey, string projectID)
        {
            var data = CreateInstance<RecoludeConfig>();
            data.apiKey = apiKey;
            data.projectID = projectID;
            return data;
        }

        /// <summary>
        /// <b>DO NOT CALL</b>: For Unity Scriptable Object Serialization Purposes.
        /// </summary>
        public void OnAfterDeserialize() { }

        /// <summary>
        /// <b>DO NOT CALL</b>: For Unity Scriptable Object Serialization Purposes.
        /// </summary>
        public void OnBeforeSerialize() { }

        /// <summary>
        /// URL to Recolude
        /// </summary>
        /// <returns>URL to Recolude</returns>
        public string GetURL()
        {
            return url;
        }

        /// <summary>
        /// API Key used to identify the application with Recolude.
        /// </summary>
        /// <returns>API Key used to identify the application with Recolude.</returns>
        public string GetAPIKey()
        {
            return this.apiKey;
        }

        /// <summary>
        /// Configures the client to point to the ID of the project inside of Recolude.
        /// </summary>
        /// <param name="newKey">The new api key to use.</param>
        /// <returns>Whether or not an update actually occurred (no update occurs if the new ID matches the present one).</returns>
        public bool SetAPIKey(string newKey)
        {
            if (string.Equals(newKey, this.apiKey))
            {
                return false;
            }
            this.apiKey = newKey;
            return true;
        }

        /// <summary>
        /// ID of the project we want to interface with.
        /// </summary>
        /// <returns>ID of the project we want to interface with.</returns>
        public string GetProjectID()
        {
            return this.projectID;
        }

        /// <summary>
        /// Configures the client to interface with a specific project inside of Recolude.
        /// </summary>
        /// <param name="projectID">The ID of the project to interface with.</param>
        /// <returns>Whether or not an update actually occurred (no update occurs if the new ID matches the present one).</returns>
        public bool SetProjectID(string projectID)
        {
            if (string.Equals(projectID, this.projectID))
            {
                return false;
            }
            this.projectID = projectID;
            return true;
        }

        /// <summary>
        /// The default visibility for the recordings that get uploaded when no
        /// visibility is specified in a direct upload call.
        /// </summary>
        /// <returns>The default visibility for recordings</returns>
        public RecordingVisibility DefaultUploadVisibility()
        {
            return defaultUploadVisibility;
        }

        /// <summary>
        /// Assign the visibility to use for uploading recordings, when no 
        /// other is specified.
        /// </summary>
        /// <param name="visibility">The recording visibility to use</param>
        public bool SetDefaultUploadVisibility(RecordingVisibility visibility)
        {
            if (visibility == defaultUploadVisibility)
            {
                return false;
            }
            defaultUploadVisibility = visibility;
            return true;
        }

        /// <summary>
        /// Builds a HTTP network request to upload a recording, but does not 
        /// execute it. The developer can choose whenever to call it. You most
        /// likely want to be using [SaveRecording](xref:Recolude.RecoludeConfig.SaveRecording(RecordAndPlay.Recording))
        /// </summary>
        /// <param name="thingToUpload">Recording to upload</param>
        /// <returns>The network request that still needs to be ran</returns>
        public UnityWebRequest BuildUploadRecordingRequest(Recording thingToUpload, RecordingVisibility visibility)
        {
            if (thingToUpload == null)
            {
                throw new System.ArgumentNullException("recoding is required to upload a recording", "thingToUpload");
            }

            byte[] recordingData = null;
            using (MemoryStream fs = new MemoryStream())
            {
                Packager.Package(fs, thingToUpload);
                recordingData = fs.GetBuffer();
            }

            if (recordingData == null)
            {
                Debug.LogWarning("Provide a valid recording");
                return null;
            }

            WWWForm form = new WWWForm();
            form.AddBinaryData("recording", recordingData, "myFile.rap", "multipart/form-data");

            switch (visibility)
            {
                case RecordingVisibility.Public:
                    form.AddField("visibility", "PUBLIC");
                    break;

                case RecordingVisibility.Private:
                    form.AddField("visibility", "PRIVATE");
                    break;

                default:
                    form.AddField("visibility", "PRIVATE");
                    break;
            }

            UnityWebRequest www = UnityWebRequest.Post(
               $"{fileserverProtocol}://{url}:{fileserverPort}/file-api/v1/projects/{GetProjectID()}/recordings",
                form
            );
            www.SetRequestHeader("X-API-KEY", GetAPIKey());
            return www;
        }

        /// <summary>
        /// Builds a HTTP network request to download a recording, but does not
        /// execute it. The developer can choose whenever to call it. You most
        /// likely want to be using [SaveRecording](xref:Recolude.RecoludeConfig.LoadRecording(System.String))
        /// </summary>
        /// <param name="recordingID">ID of the recording to download.</param>
        /// <returns>The network request that still needs to be ran</returns>
        public UnityWebRequest BuildDownloadRecordingRequest(string recordingID)
        {
            if (string.IsNullOrEmpty(recordingID))
            {
                throw new System.ArgumentNullException("provide a non-empty recording id", nameof(recordingID));
            }
            UnityWebRequest www = UnityWebRequest.Get(
                $"{fileserverProtocol}://{url}:{fileserverPort}/file-api/v1/recording/{recordingID}"
            );
            www.SetRequestHeader("X-API-KEY", GetAPIKey());
            return www;
        }

        /// <summary>
        /// Creates an operation that when ran, will upload the recording under 
        /// these specific.
        /// </summary>
        /// <param name="thingToUpload">Recording to upload</param>
        /// <returns>Request that when ran, will save the recording to Recolude.</returns>
        public ISaveRecordingOperation SaveRecording(Recording thingToUpload)
        {
            var req = this.BuildUploadRecordingRequest(thingToUpload, defaultUploadVisibility);
            return new WebSaveRecordingOperation(req);
        }

        /// <summary>
        /// Creates an operation that when ran, loads a recording from Recolude.
        /// </summary>
        /// <param name="recordingID">ID of the recording to download.</param>
        /// <returns>Request that when ran, will download the recording.</returns>
        public ILoadRecordingOperation LoadRecording(string recordingID)
        {
            var req = this.BuildDownloadRecordingRequest(recordingID);
            return new WebLoadRecordingOperation(req);
        }

        /// <summary>
        /// Creates an operation that when ran, will delete a recording from Recolude.
        /// </summary>
        /// <param name="recordingID">ID of the recording to delete.</param>
        /// <returns>Request that when ran, will delete the recording.</returns>
        public IDeleteRecordingOperation DeleteRecording(string id)
        {
            var rs = new RecordingService(this);
            return new WebDeleteRecordingOperation(rs.DeleteRecording(this.projectID, id).UnderlyingRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thingToUpload"></param>
        public void UploadRecording(Recording thingToUpload)
        {
            RecordingUploader.GetInstance().StartCoroutine(StartUploadOfRecording(thingToUpload));
        }

        private System.Collections.IEnumerator StartUploadOfRecording(Recording thingToUpload)
        {
            var req = this.BuildUploadRecordingRequest(thingToUpload, defaultUploadVisibility);
            yield return new WebSaveRecordingOperation(req).Run();
        }

    }

}