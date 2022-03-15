using System.IO;
using UnityEngine;

namespace RecordAndPlay.IO
{

    /// <summary>
    /// Uses the file system to store binary data.
    /// </summary>
    public class FSBinaryStorage : IBinaryStorage
    {

        private string basePath;

        /// <summary>
        /// Create a storage that will use <a href="https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html">Application.persistentDataPath</a> to store data.
        /// </summary>
        public FSBinaryStorage()
        {
            basePath = Application.persistentDataPath;
        }

        /// <summary>
        /// Create a storage that will write data to the base path provided.
        /// </summary>
        /// <param name="basePath">Base path for all files to be written to.</param>
        public FSBinaryStorage(string basePath)
        {
            this.basePath = basePath;
        }

        /// <summary>
        /// Deletes the file located at the combination of base path + location provided.
        /// </summary>
        /// <param name="location">Location of file.</param>
        public void Delete(string location)
        {
            File.Delete(Path.Combine(basePath, location));
        }

        /// <summary>
        /// Determines whether or not a file exists at the location passed in.
        /// </summary>
        /// <param name="location">Location of file to check existance.</param>
        /// <returns>Whether or not the file exists.</returns>
        public bool Exists(string location)
        {
            return File.Exists(Path.Combine(basePath, location));
        }

        /// <summary>
        /// Opens a stream to the file to be read. It is the responsibility of who calls this method to close the stream when they are done.
        /// </summary>
        /// <param name="location">Location of the file to open.</param>
        /// <returns>Stream to the file.</returns>
        public Stream Read(string location)
        {
            return File.OpenRead(Path.Combine(basePath, location));
        }

        /// <summary>
        /// Writes the data found in the stream at the current seek to the location specified.
        /// </summary>
        /// <param name="data">Data to write to disk.</param>
        /// <param name="location">Location to write data to.</param>
        public void Write(Stream data, string location)
        {
            using (var fileStream = File.Create(Path.Combine(basePath, location)))
            {
                data.CopyTo(fileStream);
            }
        }
 
    }

}