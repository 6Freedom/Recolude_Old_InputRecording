using System.Collections.Generic;

namespace Recolude.RecordingStorage.FailSafe
{

    /// <summary>
    /// Acts as a DTO for keeping up with what all operations have failed in the context of interfacing with a storage device.
    /// </summary>
    [System.Serializable]
    public class FailSafeOutline
    {

        [UnityEngine.SerializeField]
        private int version;

        [UnityEngine.SerializeField]
        private FailedDeletionEntry[] failedDeletions;

        [UnityEngine.SerializeField]
        private FailedSaveEntry[] failedSaves;

        public FailSafeOutline(FailedDeletionEntry[] failedDeletions, FailedSaveEntry[] failedSaves)
        {
            version = 1;
            this.failedDeletions = failedDeletions;
            this.failedSaves = failedSaves;
        }

        public FailSafeOutline()
        {
            version = 1;
            this.failedDeletions = null;
            this.failedSaves = null;
        }

        /// <summary>
        /// All failed deletion operations.
        /// </summary>
        /// <returns>All failed deletion operations.</returns>
        public FailedDeletionEntry[] GetFailedDeletions()
        {
            return failedDeletions;
        }

        /// <summary>
        /// All failed save operations.
        /// </summary>
        /// <returns>All failed save operations.</returns>
        public FailedSaveEntry[] GetFailedSaves()
        {
            return failedSaves;
        }

        /// <summary>
        /// Finds the largest ID for a failed saved recording.
        /// </summary>
        /// <returns>The largest ID for a failed saved recording.</returns>
        public int GetLargestFailedSaveRecEntry()
        {
            if (failedSaves == null || failedSaves.Length == 0)
            {
                return 0;
            }
            int max = 0;
            foreach (var failedSave in failedSaves)
            {
                if (failedSave.GetRecEntry() > max)
                {
                    max = failedSave.GetRecEntry();
                }
            }
            return max;
        }

        /// <summary>
        /// Adds to the list of failed deletions, or increments the number of times a specific recording has failed.
        /// </summary>
        /// <param name="recordingID">ID of the recording we want to delete.</param>
        public void AddFailedDeletion(string recordingID)
        {
            if (failedDeletions == null || failedDeletions.Length == 0)
            {
                failedDeletions = new FailedDeletionEntry[] { new FailedDeletionEntry(recordingID, 1) };
                return;
            }

            List<FailedDeletionEntry> termsList = new List<FailedDeletionEntry>(failedDeletions.Length);
            bool added = false;
            for (int entryIndex = 0; entryIndex < failedDeletions.Length; entryIndex++)
            {
                if (failedDeletions[entryIndex].MatchesID(recordingID))
                {
                    termsList.Add(failedDeletions[entryIndex].IncrementAttempt());
                    added = true;
                }
                else
                {
                    termsList.Add(failedDeletions[entryIndex]);
                }
            }

            if (!added)
            {
                termsList.Add(new FailedDeletionEntry(recordingID, 1));
            }

            failedDeletions = termsList.ToArray();
        }

        /// <summary>
        /// Adds to the list of failed saves, or increments the number of times a specific recording has failed.
        /// </summary>
        /// <param name="recEntry">ID of failed save.</param>
        public void AppendFailedSave(int recEntry)
        {
            if (failedSaves == null || failedSaves.Length == 0)
            {
                failedSaves = new FailedSaveEntry[] { new FailedSaveEntry(1, recEntry) };
                return;
            }

            List<FailedSaveEntry> termsList = new List<FailedSaveEntry>(failedSaves.Length);
            bool added = false;
            for (int entryIndex = 0; entryIndex < failedSaves.Length; entryIndex++)
            {
                if (failedSaves[entryIndex].MatchesPath(recEntry))
                {
                    termsList.Add(failedSaves[entryIndex].IncrementAttempt());
                    added = true;
                }
                else
                {
                    termsList.Add(failedSaves[entryIndex]);
                }
            }

            if (!added)
            {
                termsList.Add(new FailedSaveEntry(1, recEntry));
            }

            failedSaves = termsList.ToArray();
        }

        internal void RemoveFailedSavesWithMatchingRecIDs(List<int> toRemove)
        {
            if (failedSaves == null || failedSaves.Length == 0 || toRemove == null || toRemove.Count == 0)
            {
                return;
            }
            List<FailedSaveEntry> termsList = new List<FailedSaveEntry>();
            int startingRemoveIndex = 0;
            for (int entryIndex = 0; entryIndex < failedSaves.Length; entryIndex++)
            {
                if (startingRemoveIndex >= toRemove.Count)
                {
                    termsList.Add(failedSaves[entryIndex]);
                }

                bool found = false;
                for (int removeIndex = startingRemoveIndex; removeIndex < toRemove.Count && !found; removeIndex++)
                {
                    if (failedSaves[entryIndex].GetRecEntry().Equals(toRemove[removeIndex]))
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    startingRemoveIndex++;
                }
                else
                {
                    termsList.Add(failedSaves[entryIndex]);
                }
            }

            failedSaves = termsList.ToArray();
        }

        internal void RemoveFailedDeletionsWithMatchingRecIDs(List<string> toRemove)
        {
            if (failedDeletions == null || failedDeletions.Length == 0 || toRemove == null || toRemove.Count == 0)
            {
                return;
            }

            List<FailedDeletionEntry> termsList = new List<FailedDeletionEntry>();
            int startingRemoveIndex = 0;
            for (int entryIndex = 0; entryIndex < failedSaves.Length; entryIndex++)
            {
                if (startingRemoveIndex >= toRemove.Count)
                {
                    termsList.Add(failedDeletions[entryIndex]);
                }

                bool found = false;
                for (int removeIndex = startingRemoveIndex; removeIndex < toRemove.Count && !found; removeIndex++)
                {
                    if (failedSaves[entryIndex].GetRecEntry().Equals(toRemove[removeIndex]))
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    startingRemoveIndex++;
                }
                else
                {
                    termsList.Add(failedDeletions[entryIndex]);
                }
            }

            failedDeletions = termsList.ToArray();
        }

    }

}