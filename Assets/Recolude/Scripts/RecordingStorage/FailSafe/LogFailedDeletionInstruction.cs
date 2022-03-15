using UnityEngine;
using System.IO;
using System.Text;
using RecordAndPlay.IO;

namespace Recolude.RecordingStorage.FailSafe
{

    public class LogFailedDeletionInstruction : CustomYieldInstruction
    {
        string id;

        bool finished;

        public LogFailedDeletionInstruction(string id, IBinaryStorage fallback, FailSafeOutline outlineToEdit)
        {
            this.finished = false;
            this.id = id;

            FailSafeOutline outline = outlineToEdit;
            if (outline == null)
            {
                if (fallback.Exists("failsafe.json"))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fallback.Read("failsafe.json").CopyTo(memoryStream);
                        outline = JsonUtility.FromJson<FailSafeOutline>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                    }
                }
            }


            if (outline == null)
            {
                outline = new FailSafeOutline();
            }

            outline.AddFailedDeletion(id);
            fallback.Write(new MemoryStream(Encoding.UTF8.GetBytes(JsonUtility.ToJson(outline))), "failsafe.json");
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