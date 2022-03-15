using System.Collections.Generic;
using UnityEngine;
using RecordAndPlay.Util;
using RecordAndPlay.Time;


namespace RecordAndPlay.Record
{

    /// <summary>
    /// Implement this interface to allow something to be recorded by the system.
    /// </summary>
    public interface ISubject
    {

        /// <summary>
        /// The current position of the subject
        /// </summary>
        /// <returns>The current position of the subject</returns>
        Vector3 GetPosition();

        /// <summary>
        /// The current rotation of the subject in euler angles.
        /// </summary>
        /// <returns>The current rotation of the subject in euler angles.</returns>
        Vector3 GetRotation();

        /// <summary>
        /// The name of the subject
        /// </summary>
        /// <returns>The name of the subject</returns>
        string GetName();

        /// <summary>
        /// The unique ID (on a per recording basis) of the subject.
        /// </summary>
        /// <returns>The unique ID (on a per recording basis) of the subject.</returns>
        int GetID();

    }

}