namespace Recolude.RecordingStorage.FailSafe
{
    [System.Serializable]
    public class FailedSaveEntry
    {
        [UnityEngine.SerializeField]
        private int failedAttempts;

        [UnityEngine.SerializeField]
        private int recEntry;

        public FailedSaveEntry(int failedAttempts, int recEntry)
        {
            this.failedAttempts = failedAttempts;
            this.recEntry = recEntry;
        }

        public bool MatchesPath(int recEntry)
        {
            return this.recEntry.Equals(recEntry);
        }

        public int GetRecEntry()
        {
            return this.recEntry;
        }

        public FailedSaveEntry IncrementAttempt()
        {
            return new FailedSaveEntry(failedAttempts + 1, recEntry);
        }
    }
}