using System.Collections.Generic;
using UnityEngine;

using RecordAndPlay.Record;

namespace RecordAndPlay.Playback.ActorBuilderStrategies
{

    /// <summary>
    /// This Actor Builder strategy attempts to look up the subject that was originally recorded using the recorder. For this to work the original subject must still exist in the scene.
    /// </summary>
    public class RecorderActorBuilder : IActorBuilder
    {

        private GameObject fallback;

        private Recorder recorder;

        private IPlaybackCustomEventHandler customEventHandler;

        public RecorderActorBuilder(Recorder recorder, GameObject fallback, IPlaybackCustomEventHandler customEventHandler)
        {
            if (recorder == null)
            {
                throw new System.ArgumentException("need a recorder to load actors from", "recorder");
            }
            this.recorder = recorder;
            this.fallback = fallback;
            this.customEventHandler = customEventHandler;
        }

        public RecorderActorBuilder(Recorder recorder) : this(recorder, null, null)
        {
        }

        public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
        {
            foreach (var subjRecorder in recorder.SubjectRecorders())
            {
                var subject = subjRecorder.GetSubject();
                if (subject != null && subject.GetID().Equals(subjectId))
                {
                    var myObjRef = subject as GameObjectSubject;
                    if (myObjRef != null)
                    {
                        return new Actor(myObjRef.GetGameObject());
                    }
                }
            }

            GameObject actor = null;
            if (fallback != null)
            {
                actor = GameObject.Instantiate(fallback);
            }
            return new Actor(actor, customEventHandler);
        }

    }

}