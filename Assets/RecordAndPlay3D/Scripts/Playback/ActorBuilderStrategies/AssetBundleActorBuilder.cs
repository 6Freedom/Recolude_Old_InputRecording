using System.Collections.Generic;
using UnityEngine;

namespace RecordAndPlay.Playback.ActorBuilderStrategies
{

    /// <summary>
    /// This Actor Builder strategy attempts to look up the recorded subject's name inside the asset bundle provided and use that as the actor representation in playback.
    /// </summary>
    public class AssetBundleActorBuilder : IActorBuilder
    {

        private AssetBundle bundleToLoadFrom;

        private GameObject fallback;

        private IPlaybackCustomEventHandler customEventHandler;

        /// <summary>
        /// Builds a actor builder that loads actor representation from asset bundles. If the asset bundle fails to load the desired actor representation, then no actor will be used in playback. 
        /// </summary>
        /// <returns>A actor builder that loads actors from asset bundles.</returns>
        /// <param name="bundleToLoadFrom">The asset bundle to pull actors from</param>
        /// <exception cref="System.ArgumentException">Thrown when the bundle to load from is null.</exception>
        public AssetBundleActorBuilder(AssetBundle bundleToLoadFrom) : this(bundleToLoadFrom, null, null)
        {

        }

        /// <summary>
        /// Builds a actor builder that loads actor representation from asset bundles. If the asset bundle fails to load the desired actor representation, then the fallback object provided will be instantiated to be used. 
        /// </summary>
        /// <param name="bundleToLoadFrom">The asset bundle to pull actors from</param>
        /// <param name="fallback">Object to use whenever the asset bundles fails to give us anything</param>
        /// <param name="customEventHandler">The event handler the actors will use</param>
        /// <exception cref="System.ArgumentException">Thrown when the bundle to load from is null.</exception>
        public AssetBundleActorBuilder(AssetBundle bundleToLoadFrom, GameObject fallback, IPlaybackCustomEventHandler customEventHandler)
        {
            if (bundleToLoadFrom == null)
            {
                throw new System.ArgumentException("need a bundle to load actors from", "bundleToLoadFrom");
            }
            this.bundleToLoadFrom = bundleToLoadFrom;
            this.customEventHandler = customEventHandler;
            this.fallback = fallback;
        }

        public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
        {
            if (bundleToLoadFrom.Contains(subjectName) == false)
            {
                GameObject actor = null;
                if (fallback != null)
                {
                    actor = GameObject.Instantiate(fallback);
                }
                return new Actor(actor);
            }
            return new Actor(GameObject.Instantiate(bundleToLoadFrom.LoadAsset<GameObject>(subjectName)), customEventHandler);
        }

    }

}