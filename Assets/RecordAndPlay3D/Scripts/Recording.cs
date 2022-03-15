using System.Text;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using RecordAndPlay.Playback;
using RecordAndPlay.Util;

namespace RecordAndPlay
{

    /// <summary>
    /// A collection of different events and objects that where captured with 
    /// a recorder.
    /// </summary>
    /// <remarks>
    /// This is a [Scriptable Object](https://docs.unity3d.com/ScriptReference/ScriptableObject.html), 
    /// so to create it you should call the static method 
    /// [Recording.CreateInstance()](xref:RecordAndPlay.Recording.CreateInstance*).
    /// However, creation of recordings is probably best left up to the 
    /// [Recorder](xref:RecordAndPlay.Record.Recorder) object.
    /// 
    /// If you wish to save this recording to the projects assets for ease of 
    /// playback in the editor, then look into 
    /// [SaveToAssets](xref:RecordAndPlay.Recording.SaveToAssets*). Doing this
    ///  will create a recording object in your assets folder that can be 
    /// viewed directly in the editor, or can used as a variable in the 
    /// inspector of some script.
    /// 
    /// If you wish to save the recording efficiently to disk for purposes such
    /// as saving memory or transmitting over the web, look into the 
    /// [Packager](xref:RecordAndPlay.IO.Packager) class.
    /// </remarks>
    [System.Serializable]
    public class Recording : ScriptableObject, ISerializationCallbackReceiver
    {

        [SerializeField]
        private SubjectRecording[] subjectRecordings;

        /// <summary>
        /// All events that where logged while we where recording the scene.
        /// </summary>
        [SerializeField]
        private CustomEventCapture[] capturedCustomEvents;

        private Dictionary<string, string> metadata;

        [SerializeField]
        private List<string> metadataKeys;

        [SerializeField]
        private List<string> metadataValues;

        private float? startTime = null;

        private float? duration = null;

        /// <summary>
        /// Acts as the constructor of the recording class since it inherits from <a href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">ScriptableObject</a>.
        /// </summary>
        /// <param name="subjectRecordings">The different subjects that whose position and rotation where recorded.</param>
        /// <param name="capturedCustomEvents">Different global custom events that occured during capture.</param>
        /// <param name="metadata">Global key value pairs with no associated timestamp.</param>
        /// <returns>A Recording that can be used for playback.</returns>
        public static Recording CreateInstance(SubjectRecording[] subjectRecordings, CustomEventCapture[] capturedCustomEvents, Dictionary<string, string> metadata)
        {
            var data = CreateInstance<Recording>();
            data.metadata = metadata == null ? new Dictionary<string, string>() : metadata;
            data.metadataKeys = new List<string>();
            data.metadataValues = new List<string>();

            data.subjectRecordings = subjectRecordings;
            if (subjectRecordings == null)
            {
                data.subjectRecordings = new SubjectRecording[0];
            }

            data.capturedCustomEvents = capturedCustomEvents;
            if (capturedCustomEvents == null)
            {
                data.capturedCustomEvents = new CustomEventCapture[0];
            }
            return data;
        }

        /// <summary>
        /// Gets the duration of the recording by comparing the timestamps of the first and last captured events.
        /// </summary>
        /// <returns>The duration of the recording.</returns>
        public float GetDuration()
        {
            if (duration == null)
            {
                duration = CalculateDuration();
            }
            return (float)duration;
        }

        private float CalculateDuration()
        {
            if ((subjectRecordings == null || subjectRecordings.Length == 0) && (CapturedCustomEvents == null || CapturedCustomEvents.Length == 0))
            {
                return 0;
            }

            float maxFinish = Mathf.NegativeInfinity;
            float minStart = Mathf.Infinity;

            if (subjectRecordings != null)
            {
                foreach (var subject in subjectRecordings)
                {
                    if (subject != null)
                    {
                        minStart = Mathf.Min(minStart, subject.GetStartTime());
                        maxFinish = Mathf.Max(maxFinish, subject.GetEndTime());
                    }
                }
            }

            foreach (var e in CapturedCustomEvents)
            {
                minStart = Mathf.Min(minStart, e.Time);
                maxFinish = Mathf.Max(maxFinish, e.Time);
            }

            return maxFinish - minStart;
        }

        /// <summary>
        /// Builds actors meant to act out the recorded objects and what they did. 
        /// </summary>
        /// <param name="actorBuilder">What will take the subject data and return us actors for playback.</param>
        /// <param name="parent">What all created actors will be parented to.</param>
        /// <returns>Controls for controlling the playback of each individual actor.</returns>
        /// <remarks> If the actorBuilder is null then cubes will be used to represent the subject. If an actorBuilder is supplied but they supply a null Actor, then that subject will be excluded from the playback. It' generally best to not call this method directly, but use an instance of <a page="EliCDavis.RecordAndPlay.Playback.PlaybackBehavior">PlaybackBehavior</a> to call this method for you and manage all the actors.</remarks>
        public ActorPlaybackControl[] BuildActors(IActorBuilder actorBuilder, Transform parent)
        {
            List<ActorPlaybackControl> actors = new List<ActorPlaybackControl>();

            for (int actorIndex = 0; actorIndex < SubjectRecordings.Length; actorIndex++)
            {
                SubjectRecording subject = SubjectRecordings[actorIndex];
                GameObject actorRepresentation = null;
                Actor actor = null;

                if (actorBuilder != null)
                {
                    actor = actorBuilder.Build(subject.SubjectID, subject.SubjectName, subject.Metadata);
                    actorRepresentation = actor == null ? null : actor.Representation;
                }
                else
                {
                    actorRepresentation = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }

                if (actorRepresentation != null)
                {
                    actorRepresentation.transform.SetParent(parent);
                    actorRepresentation.transform.name = subject.SubjectName;
                    actorRepresentation.transform.position = subject.GetStartingPosition();
                    actorRepresentation.transform.rotation = subject.GetStartingRotation();
                    actors.Add(new ActorPlaybackControl(actorRepresentation, actor == null ? null : actor.CustomEventHandler, this, subject));
                }

            }

            return actors.ToArray();
        }

        /// <summary>
        /// The name of the recording/scriptable object asset
        /// </summary>
        public string RecordingName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// All objects whose positions and rotations where recorded in the scene.
        /// </summary>
        public SubjectRecording[] SubjectRecordings
        {
            get
            {
                return subjectRecordings;
            }
        }

        /// <summary>
        /// All custom events global to the recording. 
        /// </summary>
        public CustomEventCapture[] CapturedCustomEvents
        {
            get
            {
                return capturedCustomEvents;
            }
        }

        /// <summary>
        /// Global key value pairs with no associated timestamp. 
        /// </summary>
        public Dictionary<string, string> Metadata
        {
            get
            {
                return metadata;
            }
        }

        /// <summary>
        /// Saves the recording as an asset to the asset folder of the project. <b>Editor Only</b>
        /// </summary>
        /// <param name="name">Name of the asset.</param>
        /// <param name="path">Where in the project for the asset to be saved.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        [System.Obsolete("SaveToAssets is an editor only method and does nothing once built.")]
        public void SaveToAssets(string name, string path)
        {
            Debug.LogWarning("SaveToAssets is an editor only method and does nothing once built.");
#if UNITY_EDITOR
            if (path == "")
            {
                path = "Assets";
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, string.Format("{0}.asset", name)));

            AssetDatabase.CreateAsset(this, assetPathAndName);
            AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// Saves the recording as an asset to the root of the asset folder of the project. <b>Editor Only</b>
        /// </summary>
        /// <param name="name">Name of the asset.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        [System.Obsolete("SaveToAssets is an editor only method and does nothing once built.")]
        public void SaveToAssets(string name)
        {
#if UNITY_EDITOR
            SaveToAssets(name, "");
#endif
        }

        private string[,] LifeEventCSVEntries()
        {
            int lifeEventEntries = 0;
            for (int i = 0; i < this.subjectRecordings.Length; i++)
            {
                lifeEventEntries += subjectRecordings[i].CapturedLifeCycleEvents.Length;
            }
            var entries = new string[lifeEventEntries + 1, 3];

            entries[0, 0] = "SubjectID";
            entries[0, 1] = "Time";
            entries[0, 2] = "Event";

            int entryIndex = 1;
            foreach (var subj in subjectRecordings)
            {
                foreach (var data in subj.CapturedLifeCycleEvents)
                {
                    entries[entryIndex, 0] = subj.SubjectID.ToString();
                    entries[entryIndex, 1] = data.Time.ToString();
                    entries[entryIndex, 2] = data.LifeCycleEvent.ToString();
                    entryIndex++;
                }
            }

            return entries;
        }

        private string[,] MetadataCSVEntries()
        {
            int metadataEntries = this.metadata.Count;
            for (int i = 0; i < this.subjectRecordings.Length; i++)
            {
                metadataEntries += subjectRecordings[i].Metadata.Count;
            }
            var entries = new string[metadataEntries + 1, 3];

            entries[0, 0] = "SubjectID";
            entries[0, 1] = "Key";
            entries[0, 2] = "Value";

            int entryIndex = 1;
            foreach (var data in this.metadata)
            {
                entries[entryIndex, 0] = "";
                entries[entryIndex, 1] = data.Key;
                entries[entryIndex, 2] = data.Value;
                entryIndex++;
            }

            foreach (var subj in subjectRecordings)
            {
                foreach (var data in subj.Metadata)
                {
                    entries[entryIndex, 0] = subj.SubjectID.ToString();
                    entries[entryIndex, 1] = data.Key;
                    entries[entryIndex, 2] = data.Value;
                    entryIndex++;
                }
            }

            return entries;
        }

        private string[,] VectorEventsCSVEntries(bool pos)
        {
            int entryCount = 0;
            for (int i = 0; i < this.subjectRecordings.Length; i++)
            {
                if (pos)
                {
                    entryCount += subjectRecordings[i].CapturedPositions.Length;
                }
                else
                {
                    entryCount += subjectRecordings[i].CapturedRotations.Length;
                }
            }
            var entries = new string[entryCount + 1, 5];

            entries[0, 0] = "SubjectID";
            entries[0, 1] = "Time";
            entries[0, 2] = "X";
            entries[0, 3] = "Y";
            entries[0, 4] = "Z";

            int entryIndex = 1;
            foreach (var subj in subjectRecordings)
            {
                VectorCapture[] captures;
                if (pos)
                {
                    captures = subj.CapturedPositions;
                }
                else
                {
                    captures = subj.CapturedRotations;
                }
                foreach (var capture in captures)
                {
                    entries[entryIndex, 0] = subj.SubjectID.ToString();
                    entries[entryIndex, 1] = capture.Time.ToString();
                    entries[entryIndex, 2] = capture.Vector.x.ToString();
                    entries[entryIndex, 3] = capture.Vector.y.ToString();
                    entries[entryIndex, 4] = capture.Vector.z.ToString();
                    entryIndex++;
                }
            }

            return entries;
        }

        private string[,] CustomEventsCSVEntries()
        {
            int customEventEntries = 0;
            foreach (var e in capturedCustomEvents)
            {
                customEventEntries += e.Contents.Count;
            }

            for (int i = 0; i < this.subjectRecordings.Length; i++)
            {
                foreach (var e in subjectRecordings[i].CapturedCustomEvents)
                {
                    customEventEntries += e.Contents.Count;
                }
            }
            var entries = new string[customEventEntries + 1, 6];

            entries[0, 0] = "SubjectID";
            entries[0, 1] = "Index";
            entries[0, 2] = "Time";
            entries[0, 3] = "Name";
            entries[0, 4] = "Key";
            entries[0, 5] = "Value";

            int entryIndex = 1;
            foreach (var data in this.capturedCustomEvents)
            {
                foreach (var keyVal in data.Contents)
                {
                    entries[entryIndex, 0] = "";
                    entries[entryIndex, 1] = entryIndex.ToString();
                    entries[entryIndex, 2] = data.Time.ToString();
                    entries[entryIndex, 3] = data.Name;
                    entries[entryIndex, 4] = keyVal.Key;
                    entries[entryIndex, 5] = keyVal.Value;
                }
                entryIndex++;
            }

            foreach (var subj in subjectRecordings)
            {
                foreach (var data in subj.CapturedCustomEvents)
                {
                    foreach (var keyVal in data.Contents)
                    {
                        entries[entryIndex, 0] = subj.SubjectID.ToString();
                        entries[entryIndex, 1] = entryIndex.ToString();
                        entries[entryIndex, 2] = data.Time.ToString();
                        entries[entryIndex, 3] = data.Name;
                        entries[entryIndex, 4] = keyVal.Key;
                        entries[entryIndex, 5] = keyVal.Value;
                    }
                    entryIndex++;
                }
            }

            return entries;
        }

        /// <summary>
        /// Builds a CSV representation of the data contained within the recording.
        /// </summary>
        /// <returns>CSV representation of the data contained within the recording</returns>
        public IO.CSVPage[] ToCSV()
        {
            string[,] subjects = new string[this.subjectRecordings.Length + 1, 2];
            subjects[0, 0] = "ID";
            subjects[0, 1] = "Name";
            for (int i = 0; i < this.subjectRecordings.Length; i++)
            {
                subjects[i + 1, 0] = this.subjectRecordings[i].SubjectID.ToString();
                subjects[i + 1, 1] = this.subjectRecordings[i].SubjectName;
            }

            return new IO.CSVPage[6]{
                new IO.CSVPage("Subjects", subjects),
                new IO.CSVPage("MetaData", MetadataCSVEntries()),
                new IO.CSVPage("CustomEvents", CustomEventsCSVEntries()),
                new IO.CSVPage("PositionData", VectorEventsCSVEntries(true)),
                new IO.CSVPage("RotationData", VectorEventsCSVEntries(false)),
                new IO.CSVPage("LifeCycleEvents", LifeEventCSVEntries())
            };
        }

        /// <summary>
        /// Converts the Recording to a json formatted string.
        /// </summary>
        /// <returns>Json formatted String.</returns>
        public string ToJSON()
        {

            StringBuilder sb = new StringBuilder("{");
            sb.AppendFormat("\"Name\": \"{0}\", ", FormattingUtil.JavaScriptStringEncode(RecordingName));
            sb.AppendFormat("\"Duration\": {0}, ", GetDuration());
            sb.AppendFormat("\"Metadata\": {0}, ", FormattingUtil.ToJSON(metadata));

            sb.Append("\"CustomEvents\": [");
            for (int i = 0; i < capturedCustomEvents.Length; i++)
            {
                sb.Append(capturedCustomEvents[i].ToJSON());
                if (i < capturedCustomEvents.Length - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append("], \"Subjects\": [");
            for (int i = 0; i < subjectRecordings.Length; i++)
            {
                sb.Append(subjectRecordings[i].ToJSON());
                if (i < subjectRecordings.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("] }");

            return sb.ToString();
        }


        /// <summary>
        /// The first event of a recording does not have to occur at timestamp 0. This method calculates and caches the time of the first event captured in the recording.
        /// </summary>
        /// <returns>The time of the first event captured.</returns>
        public float GetStartTime()
        {
            if (startTime == null)
            {
                startTime = CalculateStartTime();
            }
            return (float)startTime;
        }

        private float CalculateStartTime()
        {
            var currentStart = Mathf.Infinity;
            for (int i = 0; i < subjectRecordings.Length; i++)
            {
                currentStart = Mathf.Min(currentStart, subjectRecordings[i].GetStartTime());
            }

            foreach (var e in CapturedCustomEvents)
            {
                currentStart = Mathf.Min(currentStart, e.Time);
            }
            return currentStart;
        }

        /// <summary>
        /// Used for custom Unity serialization. <b>Do not call this method</b>.
        /// </summary>
        public void OnBeforeSerialize()
        {
            metadataKeys.Clear();
            metadataValues.Clear();
            if (metadata != null)
            {
                foreach (KeyValuePair<string, string> pair in metadata)
                {
                    metadataKeys.Add(pair.Key);
                    metadataValues.Add(pair.Value);
                }
            }

        }

        /// <summary>
        /// Used for custom Unity serialization. <b>Do not call this method</b>.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }
            metadata.Clear();

            if (metadataKeys.Count != metadataValues.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < metadataKeys.Count; i++)
                metadata.Add(metadataKeys[i], metadataValues[i]);
        }

    }

}