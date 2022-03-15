
namespace RecordAndPlay.Time
{

    /// <summary>
    /// The way Record and Play can retrieve the current time. Kind of 
    /// neccessary with how many different ways you can query "time" inside of
    /// Unity.
    /// </summary>
    public interface ITimeProvider
    {

        /// <summary>
        /// The current game time.
        /// </summary>
        /// <returns>The current game time.</returns>
        float CurrentTime();

    }

}