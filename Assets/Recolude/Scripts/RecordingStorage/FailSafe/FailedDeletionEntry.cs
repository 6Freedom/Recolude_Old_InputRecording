namespace Recolude.RecordingStorage.FailSafe
{

    [System.Serializable]
    public class FailedDeletionEntry
    {

        [UnityEngine.SerializeField]
        private string recordingID;

        [UnityEngine.SerializeField]
        private int failedAttempts;

        public FailedDeletionEntry(string recordingID, int failedAttempts)
        {
            this.recordingID = recordingID;
            this.failedAttempts = failedAttempts;
        }

        public bool MatchesID(string recordingID)
        {
            return this.recordingID.Equals(recordingID);
        }

        public string RecordingID()
        {
            return recordingID;
        }

        public FailedDeletionEntry IncrementAttempt()
        {
            return new FailedDeletionEntry(recordingID, failedAttempts + 1);
        }

    }

}