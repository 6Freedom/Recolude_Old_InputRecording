using RecordAndPlay;

/// <summary>
/// Responsible for saving and loading recordings from any arbitrary storage. Many different things can act as a valid storage device such as Disk, Memory, or even a remote service like <a href="https://recolude.com/">Recolude</a>.
/// </summary>
namespace Recolude.RecordingStorage
{

    /// <summary>
    /// Interface for dealing with any type of storage that binary recordings can be saved to. 
    /// 
    /// These calls may block so it is in your best interest to wrap them in a coroutine to prevent blocking the main thread.
    /// </summary>
    public interface IRecordingStorage
    {
        /// <summary>
        /// Saves the recording and returns an ID to it for retrieval later
        /// </summary>
        /// <param name="recording">The recording to save.</param>
        /// <returns>ID to the recording to use for loading it.</returns>
        ISaveRecordingOperation SaveRecording(Recording recording);

        /// <summary>
        /// Attempts to load the recording with the given ID.
        /// </summary>
        /// <param name="id">ID of the recording we want to load.</param>
        /// <returns>The recording who's ID matches what was passed in.</returns>
        ILoadRecordingOperation LoadRecording(string id);

        /// <summary>
        /// Attempts to delete the recording with the given ID.
        /// </summary>
        /// <param name="id">ID of the recording we want to delete.</param>
        /// <returns>The recording who's ID matches what was passed in.</returns>
        IDeleteRecordingOperation DeleteRecording(string id);
    }

}