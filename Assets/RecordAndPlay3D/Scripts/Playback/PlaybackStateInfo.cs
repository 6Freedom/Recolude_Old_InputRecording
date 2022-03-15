using UnityEngine;
using RecordAndPlay.Time;

namespace RecordAndPlay.Playback
{

    internal class PlaybackStateInfo
    {

        /// <summary>
        /// How many seconds have passed in the playback recording
        /// </summary>
        private float timeThroughPlayback = 0;

        /// <summary>
        /// The currrent loaded recording that we may or maynot be playing
        /// </summary>
        private Recording recording = null;

        /// <summary>
        /// Function for building the actors of the playback. Given a name and the meta data
        /// it will give us some GameObject.
        /// </summary>
        private IActorBuilder actorBuilder;

        /// <summary>
        /// Called whenever an event occurs in the playback
        /// </summary>
        private IPlaybackCustomEventHandler[] eventHandler;

        /// <summary>
        /// The actors currentely being animated from the recording
        /// </summary>
        private ActorPlaybackControl[] actors;

        /// <summary>
        /// How fast the recording is being played back. +1 means normal speed,
        /// -1 means normal speed but backwards. 0 is stopped.
        /// </summary>
        private float playbackSpeed;

        /// <summary>
        /// The parent all actors will be parented too
        /// </summary>
        private Transform actorsParent;

        /// <summary>
        /// Whether or not the recorind should loop during playback.
        /// </summary>
        private bool loop;

        private ITimeProvider timeProvider;

        public PlaybackStateInfo(Recording recording, IActorBuilder actorBuilder, IPlaybackCustomEventHandler[] eventHandler, Transform actorsParent, bool loop, ITimeProvider timeProvider)
        {
            this.recording = recording;
            this.actorBuilder = actorBuilder;
            this.eventHandler = eventHandler;
            this.actorsParent = actorsParent;
            this.timeProvider = timeProvider;
            this.loop = loop;
            playbackSpeed = 1;
        }

        public bool ShouldLoop()
        {
            return loop;
        }

        public Recording GetRecording()
        {
            return recording;
        }

        public ITimeProvider GetTimeProvider()
        {
            return this.timeProvider;
        }

        public IPlaybackCustomEventHandler[] GetCustomEventHandlers()
        {
            return this.eventHandler;
        }

        public float GetDuration()
        {
            return recording == null ? -1 : recording.GetDuration();
        }

        public float GetPlaybackSpeed()
        {
            return playbackSpeed;
        }

        public void SetPlaybackSpeed(float playbackSpeed)
        {
            this.playbackSpeed = playbackSpeed;
        }

        public void SetTime(float time)
        {
            timeThroughPlayback = time;
        }

        public float GetTimeThroughPlayback()
        {
            return timeThroughPlayback;
        }

        public bool HasCustomEventHandlers()
        {
            return this.eventHandler != null && eventHandler.Length > 0;
        }

        public float IncrementTimeThroughPlayback(float amount)
        {
            timeThroughPlayback += amount;
            return timeThroughPlayback;
        }

        /// <summary>
        /// Destroys all actors that exist in playback and overrides them
        /// </summary>
        /// <returns>The new actors created</returns>
        public ActorPlaybackControl[] LoadActorsFromRecording()
        {
            ClearActors(ActorCleanupMethod.Destroy);
            actors = recording.BuildActors(actorBuilder, actorsParent);
            return actors;
        }

        /// <summary>
        /// Get the actors currenctly present in the scene
        /// </summary>
        /// <returns></returns>
        public ActorPlaybackControl[] GetActors()
        {
            return actors;
        }

        /// <summary>
        /// Clears the current loaded recording of all actors in the scene
        /// </summary>
        public void ClearActors(ActorCleanupMethod cleanupMethod)
        {
            if (actors != null)
            {
                foreach (ActorPlaybackControl actor in actors)
                {
                    actor.DestroyRepresentation(cleanupMethod);
                }
            }
            actors = null;
        }

    }

}