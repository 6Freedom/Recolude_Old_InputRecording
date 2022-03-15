using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using RecordAndPlay;
using RecordAndPlay.IO;

/// <summary>
/// Functionality for keeping up with the failed operations that occurred around saving recordings to an arbitrary storage device.
/// </summary>
namespace Recolude.RecordingStorage.FailSafe
{

    /// <summary>
    /// Attempts to perform the recording storage operation on the desired object, if fails then the operation becomes cached for later.
    /// 
    /// The operations that get's saved only applies to Save and Delete, as Load would not make sense.
    /// 
    /// <b>Not thread safe.</b> Errors can be introduced if multiple Fail Safes are operating on the same binary storage fallback, or if the same instance is being used across threads
    /// </summary>
    public class RecordingStorageFailSafe : IRecordingStorage
    {

        IRecordingStorage desiredStorage;

        IBinaryStorage fallback;

        public RecordingStorageFailSafe(IRecordingStorage desiredStorage, IBinaryStorage fallback)
        {
            this.desiredStorage = desiredStorage;
            this.fallback = fallback;
        }

        public IDeleteRecordingOperation DeleteRecording(string id)
        {
            return new FailSafeDeleteRecordingOperation(desiredStorage.DeleteRecording(id), id, this, null);
        }

        public ILoadRecordingOperation LoadRecording(string id)
        {
            return desiredStorage.LoadRecording(id);
        }

        public ISaveRecordingOperation SaveRecording(Recording recording)
        {
            return new FailSafeSaveRecordingOperation(desiredStorage.SaveRecording(recording), recording, this, -1, null);
        }

        private FailSafeSaveRecordingOperation SaveRecording(Recording recording, int incrementEntry, FailSafeOutline outline)
        {
            return new FailSafeSaveRecordingOperation(desiredStorage.SaveRecording(recording), recording, this, incrementEntry, outline);
        }

        private IDeleteRecordingOperation DeleteRecording(string id, FailSafeOutline outline)
        {
            return new FailSafeDeleteRecordingOperation(desiredStorage.DeleteRecording(id), id, this, outline);
        }

        public CustomYieldInstruction LogFailedDeletion(string id, FailSafeOutline outline)
        {
            return new LogFailedDeletionInstruction(id, fallback, outline);
        }

        public CustomYieldInstruction LogFailedSave(Recording recording, int incrementEntry, FailSafeOutline outline)
        {
            return new LogFailedSaveInstruction(recording, fallback, incrementEntry, outline);
        }

        public IEnumerator RetryFailedSaves()
        {
            FailSafeOutline outline = null;
            if (fallback.Exists("failsafe.json"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fallback.Read("failsafe.json").CopyTo(memoryStream);
                    outline = JsonUtility.FromJson<FailSafeOutline>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                }
            }

            if (outline == null)
            {
                yield break;
            }

            List<int> toRemove = new List<int>();
            foreach (var filedSave in outline.GetFailedSaves())
            {
                var recs = RecordAndPlay.IO.Unpackager.Unpackage(fallback.Read(filedSave.GetRecEntry().ToString()));
                var saveInstruction = SaveRecording(recs[0], filedSave.GetRecEntry(), outline);
                yield return saveInstruction.Run();
                if (string.IsNullOrEmpty(saveInstruction.Error()))
                {
                    toRemove.Add(filedSave.GetRecEntry());
                    fallback.Delete(filedSave.GetRecEntry().ToString());
                }
            }

            outline.RemoveFailedSavesWithMatchingRecIDs(toRemove);
            fallback.Write(new MemoryStream(Encoding.UTF8.GetBytes(JsonUtility.ToJson(outline))), "failsafe.json");
        }

        public IEnumerator RetryFailedDeletions()
        {
            FailSafeOutline outline = null;
            if (fallback.Exists("failsafe.json"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fallback.Read("failsafe.json").CopyTo(memoryStream);
                    outline = JsonUtility.FromJson<FailSafeOutline>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                }
            }

            if (outline == null)
            {
                yield break;
            }

            List<string> toRemove = new List<string>();
            foreach (var failedDeletion in outline.GetFailedDeletions())
            {
                var deleteInstruction = DeleteRecording(failedDeletion.RecordingID(), outline);
                yield return deleteInstruction.Run();
                if (string.IsNullOrEmpty(deleteInstruction.Error()))
                {
                    toRemove.Add(failedDeletion.RecordingID());
                }
            }

            outline.RemoveFailedDeletionsWithMatchingRecIDs(toRemove);
            fallback.Write(new MemoryStream(Encoding.UTF8.GetBytes(JsonUtility.ToJson(outline))), "failsafe.json");
        }

        public void Reset()
        {
            FailSafeOutline outline = null;
            if (fallback.Exists("failsafe.json"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fallback.Read("failsafe.json").CopyTo(memoryStream);
                    outline = JsonUtility.FromJson<FailSafeOutline>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                }
            }

            if (outline == null)
            {
                return;
            }

            foreach (var filedSave in outline.GetFailedSaves())
            {
                fallback.Delete(filedSave.GetRecEntry().ToString());
            }

            fallback.Delete("failsafe.json");
        }

    }

}