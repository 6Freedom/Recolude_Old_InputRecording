namespace Recolude.RecordingStorage
{
    /// <summary>
    /// Encapsulates an operation that will attempt to save a recording when Run() is called.
    /// </summary>
    public interface ISaveRecordingOperation: IRecordingOperation
    {
        /// <summary>
        /// The ID of the recording that was returned when the operation was run succesfully.
        /// </summary>
        /// <returns>ID of the saved recording.</returns>
        string RecordingID();
    }

}