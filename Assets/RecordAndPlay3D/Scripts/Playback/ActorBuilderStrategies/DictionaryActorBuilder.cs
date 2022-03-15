using System.Collections.Generic;
using UnityEngine;

namespace RecordAndPlay.Playback.ActorBuilderStrategies
{

    /// <summary>
    /// Actor builder strategy that trys to match the subject's name to a key in the dictionary for loading actors. If a key exists for an actor, but the value is null, an error is thrown.
    /// </summary>
    public class DictionaryActorBuilder : IActorBuilder
    {

        private Dictionary<string, GameObject> mapping;

        private GameObject fallback;

        private IPlaybackCustomEventHandler customEventHandler;

        /// <summary>
        /// Creates a actor builer that trys to match the subject's name to a key in the dictionary for loading actors.
        /// </summary>
        /// <param name="mapping">The dictionary we look up actor representations from.</param>
        /// <param name="fallback">Object to use whenever the dictionary fails to give us anything.</param>
        /// <param name="customEventHandler">The event handler the actors will use</param>
        public DictionaryActorBuilder(Dictionary<string, GameObject> mapping, GameObject fallback, IPlaybackCustomEventHandler customEventHandler)
        {
            this.mapping = mapping;
            this.fallback = fallback;
            this.customEventHandler = customEventHandler;
        }

        /// <summary>
        /// Creates a actor builer that trys to match the subject's name to a key in the dictionary for loading actors.
        /// </summary>
        /// <param name="mapping">The dictionary we look up actor representations from.</param>
        public DictionaryActorBuilder(Dictionary<string, GameObject> mapping) : this(mapping, null, null)
        {
        }

        public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
        {
            if (mapping.ContainsKey(subjectName) == false)
            {
                GameObject actor = null;
                if (fallback != null)
                {
                    actor = GameObject.Instantiate(fallback);
                }
                return new Actor(actor);
            }

            if (mapping[subjectName] == null)
            {
                throw new System.InvalidOperationException("A mapping for the actor exists but the value is null.");
            }

            return new Actor(GameObject.Instantiate(mapping[subjectName]), customEventHandler);
        }

    }

}