using System.IO;

namespace RecordAndPlay.IO
{
    /// <summary>
    /// An interface for saving and loading binaries to an arbitrary storage
    /// </summary>
    public interface IBinaryStorage
    {
        /// <summary>
        /// Read the contents of a binary at a specific location.
        /// </summary>
        /// <param name="location">Location of the binary.</param>
        /// <returns>The binary.</returns>
        Stream Read(string location);

        /// <summary>
        /// Write binary data to a specific location.
        /// </summary>
        /// <param name="data">Data to store.</param>
        /// <param name="location">Location to store it.</param>
        void Write(Stream data, string location);

        /// <summary>
        /// Delete the binary stored at a specific location
        /// </summary>
        /// <param name="location">Location of the binary</param>
        void Delete(string location);

        /// <summary>
        /// Determines whether or not a binary exists at the specified location.
        /// </summary>
        /// <param name="location">Location of the binary.</param>
        /// <returns>Whether or not the binary exists there.</returns>
        bool Exists(string location);

    }

}