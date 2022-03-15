using System.Collections;
using System.IO;
using UnityEngine;

namespace Recolude.RecordingStorage.FailSafe
{
    public class FailSafeDeleteRecordingOperation : IDeleteRecordingOperation
    {
        IDeleteRecordingOperation planA;

        bool finished;

        string idToDelete;

        RecordingStorageFailSafe failsafe;

        string error;

        FailSafeOutline outline;

        public FailSafeDeleteRecordingOperation(IDeleteRecordingOperation planA, string idToDelete, RecordingStorageFailSafe failsafe, FailSafeOutline outline)
        {
            this.planA = planA;
            this.idToDelete = idToDelete;
            this.failsafe = failsafe;
            this.finished = false;
            this.error = null;
            this.outline = outline;
        }

        public string Error()
        {
            return error;
        }

        public bool Finished()
        {
            return finished;
        }

        public IEnumerator Run()
        {
            // Hack to run coroutine without needing to call StartCoroutine()
            IEnumerator e = planA.Run();
            while (e.MoveNext()) yield return e.Current;

            if (planA.Error() == null)
            {
                yield break;
            }

            this.error = planA.Error();
            yield return failsafe.LogFailedDeletion(this.idToDelete, outline);
        }

    }

}