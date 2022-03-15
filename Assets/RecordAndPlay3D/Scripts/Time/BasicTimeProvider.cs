
namespace RecordAndPlay.Time
{

    /// <summary>
    /// Uses [UnityEngine.Time.time](https://docs.unity3d.com/ScriptReference/Time-time.html)
    /// </summary>
    public class BasicTimeProvider : ITimeProvider
    {

        /// <summary>
        /// Unity's [Time.time](https://docs.unity3d.com/ScriptReference/Time-time.html).
        /// </summary>
        /// <returns>Time.time</returns>
        public float CurrentTime(){
            return UnityEngine.Time.time;
        }

    }

}