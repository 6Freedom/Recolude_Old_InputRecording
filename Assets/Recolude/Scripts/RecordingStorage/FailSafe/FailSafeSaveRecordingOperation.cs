using System.Collections;
using RecordAndPlay;

namespace Recolude.RecordingStorage.FailSafe
{
    public class FailSafeSaveRecordingOperation : ISaveRecordingOperation
    {
        ISaveRecordingOperation planA;

        bool finished;

        RecordingStorageFailSafe failsafe;

        Recording recording;

        string error;

        int incrementEntry;

        FailSafeOutline outline;

        public FailSafeSaveRecordingOperation(ISaveRecordingOperation planA, Recording recording, RecordingStorageFailSafe failsafe, int incrementEntry, FailSafeOutline outline)
        {
            this.planA = planA;
            this.recording = recording;
            this.failsafe = failsafe;
            this.finished = false;
            this.error = null;
            this.incrementEntry = incrementEntry;
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

        public string RecordingID()
        {
            return null;
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
            yield return failsafe.LogFailedSave(this.recording, incrementEntry, outline);
        }

    }

}