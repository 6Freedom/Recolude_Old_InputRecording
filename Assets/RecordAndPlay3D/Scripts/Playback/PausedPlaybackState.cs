
namespace RecordAndPlay.Playback
{

    internal class PausedPlaybackState : PlaybackState
    {
        public PausedPlaybackState(PlaybackStateInfo playbackStateInfo) : base(playbackStateInfo)
        {
        }

        public override PlaybackState Pause()
        {
            return this;
        }

        public override PlaybackState Play()
        {
            return new PlayingPlaybackState(playbackStateInfo, playbackStateInfo.GetTimeThroughPlayback());
        }

        public override PlaybackState Stop(ActorCleanupMethod cleanupMethod)
        {
            playbackStateInfo.ClearActors(cleanupMethod);
            return new StoppedPlaybackState(playbackStateInfo);
        }

        public override PlaybackState Update()
        {
            return this;
        }

        public override void SetTimeThroughPlayback(float time)
        {
            foreach (var actor in playbackStateInfo.GetActors())
            {
                 actor.MoveTo(time);
            }
            playbackStateInfo.SetTime(time);
        }
    }

}