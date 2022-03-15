using System.Collections.Generic;
using UnityEngine;

namespace RecordAndPlay.Playback.ActorBuilderStrategies
{

    /// <summary>
    /// This Actor Builder strategy attempts to look up the recorded subject's name inside a resrouces folder and use that as the actor representation in playback. WARNING: Unity recomends avoiding the use of the resources folder. Consider using the AssetBundleActorBuilder.
    /// </summary>
    public class ResourceActorBuilder : IActorBuilder
    {

        private string pathPrefix;

        private GameObject fallback;

        private IPlaybackCustomEventHandler customEventHandler;

        /// <summary>
        /// Creates a Actor Builder that loads actor representations from the resource's folder in the unity project.
        /// </summary>
        /// <param name="pathPrefix">String that get's prepended to the recorded subject's name when loading from the resources folder</param>
        /// <param name="fallback">Object to use whenever the resource folder fails to give us anything</param>
        /// <param name="customEventHandler">The event handler the actors will use</param>
        public ResourceActorBuilder(string pathPrefix, GameObject fallback, IPlaybackCustomEventHandler customEventHandler)
        {
            this.pathPrefix = pathPrefix;
            this.fallback = fallback;
            this.customEventHandler = customEventHandler;
        }

        /// <summary>
        /// Creates a Actor Builder that loads actor representations from the resource's folder in the unity project.
        /// </summary>
        /// <param name="pathPrefix">String that get's prepended to the recorded subject's name when loading from the resources folder</param>
        public ResourceActorBuilder(string pathPrefix) : this(pathPrefix, null, null)
        {
        }

        /// <summary>
        /// Creates a Actor Builder that loads actor representations from the resource's folder in the unity project.
        /// </summary>
        /// <param name="fallback">Object to use whenever the resource folder fails to give us anything</param>
        /// <returns></returns>
        public ResourceActorBuilder(GameObject fallback) : this("", fallback, null)
        {
        }

        /// <summary>
        /// Creates a Actor Builder that loads actor representations from the resource's folder in the unity project.
        /// </summary>
        public ResourceActorBuilder() : this("", null, null)
        {
        }

        public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
        {
            var path = subjectName;
            if (string.IsNullOrEmpty(pathPrefix) == false) {
                path = string.Format("{0}/{1}", pathPrefix, subjectName);
            }

            GameObject reference = Resources.Load<GameObject>(path);

            if (reference == null)
            {
                GameObject actor = null;
                if (fallback != null)
                {
                    actor = GameObject.Instantiate(fallback);
                }
                return new Actor(actor);
            }

            return new Actor(GameObject.Instantiate(reference), customEventHandler);
        }

    }

}