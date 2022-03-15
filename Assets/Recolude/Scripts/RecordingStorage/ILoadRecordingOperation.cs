using RecordAndPlay;

namespace Recolude.RecordingStorage
{
    /// <summary>
    /// Encapsulates an operation that will attempt to load a recording when Run() is called.
    /// </summary>
    public interface ILoadRecordingOperation : IRecordingOperation
    {
        /// <summary>
        /// The recording that was returned when the operation was run succesfully (is null if an error occurred or the operation has not be ran).
        /// </summary>
        /// <returns>Recording that got loaded into memory</returns>
        Recording Recording();
    }

}