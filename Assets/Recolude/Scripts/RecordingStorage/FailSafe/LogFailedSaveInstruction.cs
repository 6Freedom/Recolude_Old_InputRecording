using UnityEngine;
using System.IO;
using System.Text;
using RecordAndPlay.IO;
using RecordAndPlay;

namespace Recolude.RecordingStorage.FailSafe
{

    public class LogFailedSaveInstruction : CustomYieldInstruction
    {
        Recording recording;

        bool finished;

        public LogFailedSaveInstruction(Recording recording, IBinaryStorage fallback, int incrementEntry, FailSafeOutline outlineReference)
        {
            this.finished = false;
            this.recording = recording;

            FailSafeOutline loadedOutline = outlineReference;

            if (outlineReference == null)
            {
                if (fallback.Exists("failsafe.json"))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fallback.Read("failsafe.json").CopyTo(memoryStream);
                        loadedOutline = JsonUtility.FromJson<FailSafeOutline>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                    }
                }
            }

            if (loadedOutline == null)
            {
                loadedOutline = new FailSafeOutline();
            }

            var recEntry = loadedOutline.GetLargestFailedSaveRecEntry() + 1;
            if (incrementEntry != -1)
            {
                recEntry = incrementEntry;
            }

            loadedOutline.AppendFailedSave(recEntry);
            fallback.Write(new MemoryStream(Encoding.UTF8.GetBytes(JsonUtility.ToJson(loadedOutline))), "failsafe.json");

            using (MemoryStream mem = new MemoryStream())
            {
                Packager.Package(mem, recording);
                mem.Seek(0, SeekOrigin.Begin);
                fallback.Write(mem, recEntry.ToString());
            }
            this.finished = true;
        }

        public override bool keepWaiting
        {
            get
            {
                return !finished;
            }
        }

    }

}
