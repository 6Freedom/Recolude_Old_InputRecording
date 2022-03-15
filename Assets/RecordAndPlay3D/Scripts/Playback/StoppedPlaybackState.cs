namespace RecordAndPlay.Playback
{

    internal class StoppedPlaybackState : PlaybackState
    {
        public StoppedPlaybackState(PlaybackStateInfo playbackStateInfo) : base(playbackStateInfo)
        {
        }

        public override PlaybackState Pause()
        {
            return this;
        }

        public override PlaybackState Play()
        {
            return new PlayingPlaybackState(playbackStateInfo);
        }

        public override PlaybackState Stop(ActorCleanupMethod cleanupMethod)
        {
            return this;
        }

        public override PlaybackState Update()
        {
            return this;
        }

        public override void SetTimeThroughPlayback(float time)
        {
        }
    }

}