using UnityEngine;
using RecordAndPlay.Time;
using System.Collections.Generic;
using RecordAndPlay.Playback.ActorBuilderStrategies;

namespace RecordAndPlay.Playback
{

    /// <summary>
    /// A class meant for providing a no-code solution for playback. You really
    /// won't get any benefit using this class programatically, and are better
    /// off just directly interfacting with 
    /// [PlaybackBehavior](xref:RecordAndPlay.Playback.PlaybackBehavior).
    /// </summary>
    public class PlaybackBuilderBehavior : MonoBehaviour, ISerializationCallbackReceiver
    {

        public enum ActorBuilderStrategy
        {
            NameMapping,
            ResourcesFolder
        }

        [SerializeField]
        private ActorBuilderStrategy strategy;

        [SerializeField]
        private Recording recording;

        [SerializeField]
        private bool loop;

        [SerializeField]
        private string resourcesSubpath;

        /// <summary>
        /// The subpath that will be pre-pended to a subject actor's name when
        /// the builder is using the ResourcesFolder strategy.
        /// </summary>
        /// <returns>Resource folder subpath.</returns>
        public string GetResourceSubpath()
        {
            if (resourcesSubpath == null)
            {
                resourcesSubpath = "";
            }
            return resourcesSubpath;
        }

        /// <summary>
        /// Set the subpath that get's pre-pended to a subject actors name when
        /// the builder is using the ResourcesFolder strategy.
        /// </summary>
        /// <param name="resourcesSubpath">
        /// The subpath to use in the ResourcesFolder Strategy.
        /// </param>
        public void SetResourceSubpath(string resourcesSubpath)
        {
            this.resourcesSubpath = resourcesSubpath;
        }

        /// <summary>
        /// The strategy this builder will use
        /// </summary>
        /// <returns>The strategy this builder will use</returns>
        public ActorBuilderStrategy Strategy()
        {
            return this.strategy;
        }

        /// <summary>
        /// Set the new actor builder strategy the builder will use.
        /// </summary>
        /// <param name="newStrat">
        /// The new actor builder strategy the builder will use.
        /// </param>
        public void SetStrategy(ActorBuilderStrategy newStrat)
        {
            this.strategy = newStrat;
        }

        public Recording GetRecording()
        {
            return recording;
        }

        public void SetRecording(Recording rec)
        {
            recording = rec;
            nameMapping = new Dictionary<string, GameObject>();

            if (rec == null)
            {
                return;
            }

            foreach (var subj in rec.SubjectRecordings)
            {
                if (nameMapping.ContainsKey(subj.SubjectName) == false)
                {
                    nameMapping.Add(subj.SubjectName, null);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in nameMapping)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            nameMapping = new Dictionary<string, GameObject>();

            for (int i = 0; i != Mathf.Min(_keys.Count, _values.Count); i++)
            {
                nameMapping.Add(_keys[i], _values[i]);
            }
        }

        [SerializeField]
        private List<string> _keys;

        [SerializeField]
        private List<GameObject> _values;

        private Dictionary<string, GameObject> nameMapping;

        private IActorBuilder BuildActorBuilder()
        {
            switch (strategy)
            {
                case ActorBuilderStrategy.NameMapping:
                    return new DictionaryActorBuilder(nameMapping);

                case ActorBuilderStrategy.ResourcesFolder:
                    return new ResourceActorBuilder(GetResourceSubpath());

                default:
                    return null;
            }
        }

        public Dictionary<string, GameObject> GetNameMappingForDictionaryBuilder()
        {
            return nameMapping;
        }

        public void SetActorForSubjectName(string name, GameObject actor)
        {
            this.nameMapping[name] = actor;
        }

        PlaybackBehavior curPlayback;

        void Start()
        {
            curPlayback = PlaybackBehavior.Build(recording, BuildActorBuilder(), null, loop);
            curPlayback.Play();
        }

        void Update()
        {
            if (curPlayback.GetTimeThroughPlayback() >= curPlayback.RecordingDuration())
            {
                curPlayback.Stop(ActorCleanupMethod.Destroy);
                Destroy(gameObject);
            }
        }

    }

}