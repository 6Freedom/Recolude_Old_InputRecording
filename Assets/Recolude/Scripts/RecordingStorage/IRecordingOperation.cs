using System.Collections;

namespace Recolude.RecordingStorage
{

    /// <summary>
    /// Encapsulates an operation that will attempt to perform some operation on a recording when Run() is called.
    /// </summary>
    public interface IRecordingOperation
    {
        /// <summary>
        /// Runs the operation. Because this could be something like a web request, it is advised to yield to this function inside a coroutine.
        /// </summary>
        /// <returns>An IEnumerator to yield to inside a coroutine.</returns>
        IEnumerator Run();

        /// <summary>
        /// Whether or not the operation is finished. Will return false if the operation has not even begun.
        /// </summary>
        /// <returns>Whether or not the operation is finished.</returns>
        bool Finished();

        /// <summary>
        /// Any error that occured during the execution of the operation.
        /// </summary>
        /// <returns>The error message.</returns>
        string Error();
    }

}