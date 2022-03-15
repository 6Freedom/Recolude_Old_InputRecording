using UnityEngine;
using RecordAndPlay.Util;
using System.Collections.Generic;
using System.Text;

namespace RecordAndPlay
{
    /// <summary>
    /// An event that acts as a key value pair with an associated timestamp.
    /// </summary>
    [System.Serializable]
    public class CustomEventCapture : Capture, ISerializationCallbackReceiver
    {

        [SerializeField]
        string name;

        /// <summary>
        /// Old V1 representation, is now kept around for compatibility sake. 
        /// </summary>
        [SerializeField]
        string contents;

        Dictionary<string, string> contentData;

        [SerializeField]
        private List<string> keys;

        [SerializeField]
        private List<string> values;

        /// <summary>
        /// Create a new custom event capture. Initializes a dictionary with a single entry that is "value" => contents
        /// </summary>
        /// <param name="time">The time the event occured in the recording.</param>
        /// <param name="name">The name of the event.</param>
        /// <param name="contents">The details of the event that occured.</param>
        public CustomEventCapture(float time, string name, string contents) : base(time)
        {
            this.name = name;
            this.contentData = new Dictionary<string, string> { { "value", contents } };
            this.keys = new List<string>();
            this.values = new List<string>();
        }

        /// <summary>
        /// Create a new custom event capture.
        /// </summary>
        /// <param name="time">The time the event occured in the recording.</param>
        /// <param name="name">The name of the event.</param>
        /// <param name="contents">The details of the event that occured.</param>
        public CustomEventCapture(float time, string name, Dictionary<string, string> contentData) : base(time)
        {
            this.name = name;
            this.contentData = contentData;
            this.keys = new List<string>();
            this.values = new List<string>();
        }

        /// <summary>
        /// The details of the event
        /// </summary>
        public Dictionary<string, string> Contents
        {
            get
            {
                // We're actually an old event type that we're converting to new data representaiton.
                if (string.IsNullOrEmpty(contents) == false)
                {
                    return new Dictionary<string, string>(){
                        { "value", contents },
                    };
                }
                return contentData;
            }
        }

        /// <summary>
        /// The name of the event
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Creates a new CustomEventCapture with the same name and details but with a modified time, leaving the original event unchanged.
        /// </summary>
        /// <param name="newTime">The new time the event occured in the recording.</param>
        /// <returns>A entirely new capture that occured with the time passed in.</returns>
        public override Capture SetTime(float newTime)
        {
            return new CustomEventCapture(newTime, name, Contents);
        }

        /// <summary>
        /// Builds a JSON string that represents the CustomEventCapture object.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public override string ToJSON()
        {
            StringBuilder builder = new StringBuilder("{");
            builder.AppendFormat(" \"Time\": {0}, \"Name\": \"{1}\", \"Contents\": [", Time, FormattingUtil.JavaScriptStringEncode(name));

            int entriesWritten = 0;
            foreach (var keyVal in contentData)
            {
                builder.AppendFormat(
                    "{{\"Key\": \"{0}\", \"Value\": \"{1}\"}}",
                    FormattingUtil.JavaScriptStringEncode(keyVal.Key),
                    FormattingUtil.JavaScriptStringEncode(keyVal.Value)
                );

                if (entriesWritten < contentData.Count - 1)
                {
                    builder.AppendFormat(", ");
                }

                entriesWritten++;
            }

            builder.Append("]}");
            return builder.ToString();
        }

        /// <summary>
        /// Builds a string that represents a single row in a csv file that contains this object's data.
        /// </summary>
        /// <returns>A row of csv data as a string.</returns>
        public override string ToCSV()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var keyVal in contentData)
            {
                sb.AppendFormat(
                    "{0}, {1}, {2}\n",
                    Time,
                    FormattingUtil.StringToCSVCell(keyVal.Key),
                    FormattingUtil.StringToCSVCell(keyVal.Value)
                );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Used for custom Unity serialization. <b>Do not call this method</b>.
        /// </summary>
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            if (contentData != null)
            {
                foreach (KeyValuePair<string, string> pair in contentData)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }

        }

        /// <summary>
        /// Used for custom Unity serialization. <b>Do not call this method</b>.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (contentData == null)
            {
                contentData = new Dictionary<string, string>();
            }
            contentData.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                contentData.Add(keys[i], values[i]);
        }

    }
}