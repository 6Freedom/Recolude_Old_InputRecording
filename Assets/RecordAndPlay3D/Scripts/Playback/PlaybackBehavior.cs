using UnityEngine;
using RecordAndPlay.Time;

namespace RecordAndPlay.Playback
{

    /// <summary>
    /// This is used for animating a recording inside the scene.
    /// </summary>
    /// <remarks>
    /// I would recommend using 
    /// [Build()](xref:RecordAndPlay.Playback.PlaybackBehavior.Build*) for
    /// setting up the playback for a recording.
    /// </remarks>
    public class PlaybackBehavior : MonoBehaviour
    {

        /// <summary> 
        /// The current state we're in that determines whether or not we're animating actors
        /// </summary>
        private PlaybackState currentPlaybackState;

        private PlaybackStateInfo stateInfo;

        private static IPlaybackCustomEventHandler[] emptyHandlers = new IPlaybackCustomEventHandler[0];
        private static IPlaybackCustomEventHandler[] ToHandlers(IPlaybackCustomEventHandler onEvent)
        {
            if (onEvent == null)
            {
                return emptyHandlers;
            }
            return new IPlaybackCustomEventHandler[] { onEvent };
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        /// <param name="actorBuilder">What is used to build the actors.</param>
        /// <param name="onEvent">What to call when an event occurs.</param>
        /// <param name="loop">Whether or not for the recoding playback to start over when it reaches the end.</param>
        /// <returns>A controller for playback.</returns>
        public static PlaybackBehavior Build(Recording recording, IActorBuilder actorBuilder, IPlaybackCustomEventHandler onEvent, bool loop)
        {
            return Build(recording, actorBuilder, loop, ToHandlers(onEvent), new BasicTimeProvider());
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        /// <param name="actorBuilder">What is used to build the actors.</param>
        /// <param name="onEvent">What to call when an event occurs.</param>
        /// <param name="loop">Whether or not for the recoding playback to start over when it reaches the end.</param>
        /// <param name="timeProvider">How the playback will percieve time.</param>
        /// <returns>A controller for playback.</returns>
        public static PlaybackBehavior Build(Recording recording, IActorBuilder actorBuilder, IPlaybackCustomEventHandler onEvent, bool loop, ITimeProvider timeProvider)
        {
            return Build(recording, actorBuilder, loop, ToHandlers(onEvent), timeProvider);
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        /// <param name="actorBuilder">What is used to build the actors.</param>
        /// <param name="onEventHandlers">Array of event handlers that will get called whenever an event occurs.</param>
        /// <param name="loop">Whether or not for the recoding playback to start over when it reaches the end.</param>
        /// <param name="timeProvider">How the playback will percieve time.</param>
        /// <returns>A controller for playback.</returns>
        public static PlaybackBehavior Build(Recording recording, IActorBuilder actorBuilder, bool loop, IPlaybackCustomEventHandler[] onEvent, ITimeProvider timeProvider)
        {
            PlaybackBehavior playbackBehavior = new GameObject("PLAYBACK OBJECT").AddComponent<PlaybackBehavior>();

            playbackBehavior.stateInfo = new PlaybackStateInfo(recording, actorBuilder, onEvent, playbackBehavior.transform, loop, timeProvider);
            playbackBehavior.currentPlaybackState = new StoppedPlaybackState(playbackBehavior.stateInfo);

            return playbackBehavior;
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        public static PlaybackBehavior Build(Recording recording)
        {
            return Build(recording, null, null, false);
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        /// <param name="loop">Whether or not for the recoding playback to start over when it reaches the end.</param>
        public static PlaybackBehavior Build(Recording recording, bool loop)
        {
            return Build(recording, null, null, loop);
        }

        /// <summary>
        /// Start reenacting a recording.
        /// </summary>
        public void Play()
        {
            currentPlaybackState = currentPlaybackState.Play();
        }

        /// <summary>
        /// Start reenacting a recording at a specified time.
        /// </summary>
        /// <param name="timeThroughRecording">The time we want to start playing at.</param>
        public void Play(float timeThroughRecording)
        {
            currentPlaybackState = currentPlaybackState.Play();
            SetTimeThroughPlayback(timeThroughRecording);
        }

        /// <summary>
        /// Multiplies the current playback speed by -1 (or by nothing if it's already negative).
        /// </summary>
        public void PlayBackwards(float timeThroughRecording)
        {
            // Only reverse if it's going forward in time.
            if (GetPlaybackSpeed() > 0)
            {
                SetPlaybackSpeed(GetPlaybackSpeed() * -1);
            }

            currentPlaybackState = currentPlaybackState.Play();

            SetTimeThroughPlayback(timeThroughRecording);
        }

        /// <summary>
        /// Multiplies the current playback speed by -1 (or by nothing if it's already negative) and sets the current time through the playback to the end of the recording if we're at the begining of the recording.
        /// </summary>
        public void PlayBackwards()
        {
            // Only reverse if it's going forward in time.
            if (GetPlaybackSpeed() > 0)
            {
                SetPlaybackSpeed(GetPlaybackSpeed() * -1);
            }

            currentPlaybackState = currentPlaybackState.Play();

            if (GetTimeThroughPlayback() == 0)
            {
                SetTimeThroughPlayback(RecordingDuration());
            }
        }

        /// <summary>
        /// Stops the loaded recording. Actor representations are removed through <a page="https://docs.unity3d.com/ScriptReference/Object.Destroy.html">Destroy()</a>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public void Stop()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Trying to stop a recording when one isn't loaded yet!");
            }
            Stop(ActorCleanupMethod.Destroy);
        }

        /// <summary>
        /// Stops the loaded recording.
        /// </summary>
        /// <param name="cleanupMethod">How we want to cleanup the actors used in playback</param>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public void Stop(ActorCleanupMethod cleanupMethod)
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Trying to stop a recording when one isn't loaded yet!");
            }
            currentPlaybackState = currentPlaybackState.Stop(cleanupMethod);
        }

        /// <summary>
        /// If a recording is playing, then it will become paused.
        /// </summary>
        public void Pause()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Trying to pause a recording when one isn't loaded yet!");
            }
            currentPlaybackState = currentPlaybackState.Pause();
        }

        /// <summary>
        /// Whether or not the playback service is currentely active.
        /// </summary>
        /// <returns><c>true</c>, if currentely playing, <c>false</c> otherwise.</returns>
        public bool CurrentlyPlaying()
        {
            return currentPlaybackState.GetType() == typeof(PlayingPlaybackState);
        }

        /// <summary>
        /// Returns whether or not the current recording loaded is paused or not.
        /// </summary>
        /// <returns><c>true</c>, if currentely paused, <c>false</c> otherwise.</returns>
        public bool CurrentlyPaused()
        {
            return currentPlaybackState.GetType() == typeof(PausedPlaybackState);
        }

        /// <summary>
        /// Returns whether or not the current recording loaded is stopped or not.
        /// </summary>
        /// <returns><c>true</c>, if currentely stopped, <c>false</c> otherwise.</returns>
        public bool CurrentlyStopped()
        {
            return currentPlaybackState.GetType() == typeof(StoppedPlaybackState);
        }

        /// <summary>
        /// The current playback speed.
        /// </summary>
        /// <returns>The current playback speed.</returns>
        public float GetPlaybackSpeed()
        {
            return stateInfo.GetPlaybackSpeed();
        }

        /// <summary>
        /// Changes the speed in which the recording will be played back as. A positive speed means the recording processes normally. A negative speend means the recording processes backwards. A speed of 0 means the recording does not budge. A speed of 1 is default.
        /// </summary>
        /// <param name="newSpeed">The speed for the playback to run at.</param>
        public void SetPlaybackSpeed(float newSpeed)
        {
            stateInfo.SetPlaybackSpeed(newSpeed);
        }

        /// <summary>
        /// Sets the time we want the playback to be at in the recording.
        /// Useful for seeking.
        /// </summary>
        /// <param name="time">Time.</param>
        public void SetTimeThroughPlayback(float time)
        {
            currentPlaybackState.SetTimeThroughPlayback(time);
        }

        /// <summary>
        /// Gets the length of recording.
        /// </summary>
        /// <returns>The length of recording.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public float RecordingDuration()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Can't get durration if there's no recording");
            }
            return currentPlaybackState.GetDuration();
        }

        /// <summary>
        /// The time in seconds of how long the recording has been playing.
        /// </summary>
        /// <returns>The time through recording.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public float GetTimeThroughPlayback()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Can't get durration if there's no recording");
            }
            return currentPlaybackState.GetTimeThroughPlayback();
        }

        /// <summary>
        /// The recording that is being played back.
        /// </summary>
        /// <returns>The recording being animated.</returns>
        public Recording GetRecording()
        {
            return stateInfo.GetRecording();
        }

        /// <summary>
        /// Progresses the playback. You probably don't want to call this, as 
        /// this is normally handeled by the monobehaviors, Update() lifecycle
        /// event. The one notable exception is when you're writing unit tests
        /// around a playback.
        /// </summary>
        public void UpdateState()
        {
            currentPlaybackState = currentPlaybackState.Update();
        }

        void Update()
        {
            if (currentPlaybackState == null)
            {
                throw new System.Exception("Somehow got null playback state");
            }
            UpdateState();
        }

    }

}