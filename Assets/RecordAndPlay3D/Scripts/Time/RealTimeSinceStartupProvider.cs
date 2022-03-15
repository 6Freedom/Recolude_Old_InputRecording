
namespace RecordAndPlay.Time
{

    /// <summary>
    /// Uses UnityEngine.Time.realtimeSinceStartup. Good for if you want to do  stuff inside the editor without having to actually having the game playing.
    /// Unlike Time.time, this value doesn't get scaled by <a href="https://docs.unity3d.com/ScriptReference/Time-timeScale.html">timeScale</a>.
    /// </summary>
    public class RealTimeSinceStartupProvider : ITimeProvider
    {

        public float CurrentTime()
        {
            return UnityEngine.Time.realtimeSinceStartup;
        }

    }

}