using System.Collections.Generic;
using UnityEngine;
using RecordAndPlay.Util;
using RecordAndPlay.Time;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Everything relevant to building a recording.
/// </summary>
namespace RecordAndPlay.Record
{

    /// <summary>
    /// Used for building recordings and keeping up with all the subject 
    /// recorders.
    /// </summary>
    /// <remarks>
    /// Metadata set will persist accross multiple recordings unless cleared 
    /// with [ClearMetadata()](xref:RecordAndPlay.Record.Recorder.ClearMetadata*).
    /// 
    /// Because the Recorder is a scriptable object, you can create it through 
    /// two different methods.
    /// 
    /// Method one is to create it like most other scriptable objects:
    /// 
    /// `var myRecorder = ScriptableObject.CreateInstance<Recorder>();`
    /// Or you can create it through Unity's Editor by right clicking inside 
    /// the asset folder:
    /// 
    /// ![Create Recording](/images/CreateRecorder.png)
    /// </remarks>
    [CreateAssetMenu(menuName = "RecordAndPlay/Recorder")]
    public class Recorder : ScriptableObject
    {

        /// <summary>
        /// The current state of the recording service, as to whether or not it
        /// is recording actors.
        /// </summary>
        private RecordingState currentState = RecordingState.Stopped;

        /// <summary>
        /// Represents different periods in which the recording was paused
        /// </summary>
        private List<Vector2> pauseSlices;

        /// <summary>
        /// The time in seconds since the scene started when the pause command 
        /// was called while we where recording.
        /// </summary>
        private float timePaused;

        /// <summary>
        /// The time the recording started
        /// </summary>
        private float timeStarted;

        /// <summary>
        /// All subjects that are to be included in the recording
        /// </summary>
        private List<SubjectRecorder> subjectsToRecord = new List<SubjectRecorder>();

        /// <summary>
        /// All custom global events that have occurred while recording.
        /// </summary>
        private List<CustomEventCapture> customEvents;

        private Dictionary<string, string> metadata;

        ITimeProvider timeProvider = new BasicTimeProvider();

        [SerializeField]
        private RecordingEvent onRecordingComplete;

        /// <summary>
        /// Adds a Subject for the recorder to include when building recordings.
        /// </summary>
        /// <remarks>
        /// This function is already called by [SubjectBehavior](xref:RecordAndPlay.Record.SubjectBehavior) 
        /// on Start. You shouldn't have to call this unless you called [ClearSubjects()](xref:RecordAndPlay.Record.Recorder.ClearSubjects*)
        /// after the Start of the [SubjectBehavior](xref:RecordAndPlay.Record.SubjectBehavior).
        /// </remarks>
        /// <param name="subjectRecorder">The subject to keep up with.</param>
        public void Register(params SubjectRecorder[] subjectRecorder)
        {
            subjectsToRecord.AddRange(subjectRecorder);
        }

        private RecordingEvent onRecordingCompleteInstance()
        {
            if (onRecordingComplete == null)
            {
                onRecordingComplete = new RecordingEvent();
            }
            return onRecordingComplete;
        }

        /// <summary>
        /// Registers a callback to be invoked whenever the 
        /// [Finish()](xref:RecordAndPlay.Record.Recorder.Finish*) method is 
        /// called.
        /// </summary>
        /// <param name="callback">The callback to be executed</param>
        public void AddOnRecordingCompleteCallback(UnityAction<Recording> callback)
        {
            onRecordingCompleteInstance().AddListener(callback);
        }

        /// <summary>
        /// Removes a previously registered callback from being invoked when the 
        /// [Finish()](xref:RecordAndPlay.Record.Recorder.Finish*) method is 
        /// called.
        /// </summary>
        /// <param name="callback">The callback to be removed</param>
        public void RemoveOnRecordingCompleteCallback(UnityAction<Recording> callback)
        {
            onRecordingCompleteInstance().RemoveListener(callback);
        }

        /// <summary>
        /// Removes all callbacks from being called when 
        /// [Finish()](xref:RecordAndPlay.Record.Recorder.Finish*) is invoked
        /// </summary>
        public void RemoveAllOnRecordingCompleteCallbacks()
        {
            onRecordingCompleteInstance().RemoveAllListeners();
        }

        /// <summary>
        /// Set a key value pair for meta data. 
        /// </summary>
        /// <param name="key">Dictionary Key.</param>
        /// <param name="value">Value.</param>
        public void SetMetaData(string key, string value)
        {
            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }

            if (metadata.ContainsKey(key))
            {
                metadata[key] = value;
            }
            else
            {
                metadata.Add(key, value);
            }
        }

        public ITimeProvider GetTimeProvider()
        {
            return this.timeProvider;
        }

        /// <summary>
        /// Whether or not we are accepting events occuring.
        /// </summary>
        /// <returns>True if we are accepting events.</returns>
        public bool CurrentlyRecording()
        {
            return currentState == RecordingState.Recording;
        }

        /// <summary>
        /// Whether or not the recorder is paused. When the recorder is paused, custom events logged will be ignored.
        /// </summary>
        /// <returns>True if the recerder is paused.</returns>
        public bool CurrentlyPaused()
        {
            return currentState == RecordingState.Paused;
        }

        /// <summary>
        /// Whether or not the recorder has begun recording.
        /// </summary>
        /// <returns>True if it has not started recording.</returns>
        public bool CurrentlyStopped()
        {
            return currentState == RecordingState.Stopped;
        }

        /// <summary>
        /// The current state the recorder is in.
        /// </summary>
        /// <returns>The current recorder state.</returns>
        public RecordingState CurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Stops the recorder and builds a recording for playback. Once a recorder is finished it is free to start making a whole new recording.
        /// </summary>
        /// <returns>A recording containing everything the recorder captured while not paused.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the recorder is stopped.</exception>
        public Recording Finish()
        {
            if (CurrentlyStopped())
            {
                throw new System.InvalidOperationException("Not recording anything! Nothing to build!");
            }

            if (CurrentlyPaused())
            {
                Resume();
            }

            currentState = RecordingState.Stopped;
            var recordings = new SubjectRecording[subjectsToRecord.Count];

            float endTime = timeProvider.CurrentTime();
            if (timeStarted >= endTime)
            {
                Debug.LogWarningFormat("The starting time: {0}, is greater than or equal to the ending time: {1}. This means that Time.time is probably returning 0.0, which sometimes occurs when you save a recording in the OnDestroy() event when the application is closing. Setting the end time to Mathf.Infinity.", timeStarted, endTime);
                endTime = Mathf.Infinity;
            }

            for (int i = 0; i < recordings.Length; i++)
            {
                recordings[i] = subjectsToRecord[i].Save(timeStarted, endTime, pauseSlices);
            }

            var rec = Recording.CreateInstance(recordings, CaptureUtil.FilterAndShift(customEvents, timeStarted, endTime, pauseSlices), metadata);
            onRecordingCompleteInstance().Invoke(rec);
            return rec;
        }

        /// <summary>
        /// Takes everything the recorder has seen so far and builds a recording from it, without stopping the recording process.
        /// </summary>
        /// <returns>A recording representing everything we've seen up until this point in time.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the recorder is stopped.</exception>
        public Recording BuildRecording()
        {
            if (CurrentlyStopped())
            {
                throw new System.InvalidOperationException("Not recording anything! Nothing to build!");
            }

            if (CurrentlyPaused())
            {
                pauseSlices.Add(new Vector2(timePaused, timeProvider.CurrentTime()));
            }

            var recordings = new SubjectRecording[subjectsToRecord.Count];
            for (int i = 0; i < recordings.Length; i++)
            {
                recordings[i] = subjectsToRecord[i].Save(timeStarted, timeProvider.CurrentTime(), pauseSlices);
            }
            Recording recording = Recording.CreateInstance(recordings, CaptureUtil.FilterAndShift(customEvents, timeStarted, timeProvider.CurrentTime(), pauseSlices), metadata);
            if (CurrentlyPaused())
            {
                pauseSlices.RemoveAt(pauseSlices.Count - 1);
            }
            return recording;
        }

        /// <summary>
        /// Returns a recorder to a recording state if it was paused.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the recorder is not paused.</exception>
        public void Resume()
        {
            if (CurrentlyRecording())
            {
                throw new System.InvalidOperationException("Currentely recording! Nothing to resume!");
            }

            if (CurrentlyStopped())
            {
                throw new System.InvalidOperationException("Not recording anything! Nothing to resume!");
            }

            if (timeProvider.CurrentTime() - timePaused < 0)
            {
                throw new System.InvalidOperationException("You spent negative time paused... Please try to recreate this error and send steps to eli.davis1995@gmail.com. Ignoring Resume() and staying paused");
            }

            pauseSlices.Add(new Vector2(timePaused, timeProvider.CurrentTime()));
            currentState = RecordingState.Recording;
        }

        /// <summary>
        /// Moves the recorder state to be paused. Custom events that occur during this state are ignored.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the recorder is not recording.</exception>
        public void Pause()
        {
            if (CurrentlyStopped() || CurrentlyPaused())
            {
                throw new System.InvalidOperationException("Not recording anything! Nothing to pause!");
            }

            timePaused = timeProvider.CurrentTime();
            currentState = RecordingState.Paused;
        }

        /// <summary>
        /// If you want to keep up with something special that occured at a certain time in your recording, then you can call this function with the details of the special event.
        /// </summary>
        /// <param name="name">Name of the event.</param>
        /// <param name="contents">Details of the event.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when the recorder is stopped.</exception>
        /// <remarks>Will be ignored if the recorder is currently paused.</remarks>
        public void CaptureCustomEvent(string name, string contents)
        {
            if (CurrentlyPaused())
            {
                return;
            }
            if (CurrentlyStopped())
            {
                throw new System.InvalidOperationException("Not recording anything! Can't capture event!");
            }
            if (customEvents == null)
            {
                customEvents = new List<CustomEventCapture>();
            }
            customEvents.Add(new CustomEventCapture(timeProvider.CurrentTime(), name, contents));
        }

        /// <summary>
        /// If you want to keep up with something special that occured at a certain time in your recording, then you can call this function with the details of the special event.
        /// </summary>
        /// <param name="name">Name of the event.</param>
        /// <param name="contents">Details of the event.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when the recorder is stopped.</exception>
        /// <remarks>Will be ignored if the recorder is currently paused.</remarks>
        public void CaptureCustomEvent(string name, Dictionary<string, string> contents)
        {
            if (CurrentlyPaused())
            {
                return;
            }
            if (CurrentlyStopped())
            {
                throw new System.InvalidOperationException("Not recording anything! Can't capture event!");
            }
            if (customEvents == null)
            {
                customEvents = new List<CustomEventCapture>();
            }
            customEvents.Add(new CustomEventCapture(timeProvider.CurrentTime(), name, contents));
        }

        /// <summary>
        /// Starts recording the subjects.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when a recording is already in progress.</exception>
        public void Start()
        {
            if (CurrentlyStopped() == false)
            {
                throw new System.InvalidOperationException("Can't start a new recording while one is in progress. Please finish the current recording before tryign to start a new one");
            }
            pauseSlices = new List<Vector2>();
            customEvents = new List<CustomEventCapture>();
            currentState = RecordingState.Recording;
            timeStarted = timeProvider.CurrentTime();
        }

        /// <summary>
        /// Removes all registered subjects that where going to be recorded, removing any chance that their data will make it to the final recording. 
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when a recording is already in progress.</exception>
        public void ClearSubjects()
        {
            if (CurrentlyStopped() == false)
            {
                throw new System.InvalidOperationException("Can't clear subjects while in the middle of a recording.");
            }
            subjectsToRecord.Clear();
        }


        /// <summary>
        /// The different subjects to include in our recording.
        /// </summary>
        /// <returns>The different subjects to include in our recording.</returns>
        public List<SubjectRecorder> SubjectRecorders()
        {
            return this.subjectsToRecord;
        }

        /// <summary>
        /// Clears all meta data that has been set up to this point.
        /// </summary>
        public void ClearMetadata()
        {
            metadata = new Dictionary<string, string>();
        }

    }

}