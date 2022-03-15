using System.Text;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using RecordAndPlay.Playback;
using RecordAndPlay.Util;

namespace RecordAndPlay
{

    public static class RecordingCollectionExtensions
    {

        /// <summary>
        /// Creates a set of tables that represent the recordings in CSV form.
        /// </summary>
        /// <param name="recordings">The recordings to convert to CSV</param>
        /// <returns>CSV representation of the recordings</returns>
        public static IO.CSVPage[] ToCSV(this Recording[] recordings)
        {
            var pages = new IO.CSVPage[6][];
            for (int p = 0; p < pages.Length; p++)
            {
                pages[p] = new IO.CSVPage[recordings.Length];
            }

            string[,] recordingsPage = new string[recordings.Length + 1, 2];
            recordingsPage[0, 0] = "ID";
            recordingsPage[0, 1] = "Name";

            for (int recIndex = 0; recIndex < recordings.Length; recIndex++)
            {
                recordingsPage[recIndex +1, 0] = recIndex.ToString();
                recordingsPage[recIndex +1, 1] = recordings[recIndex].RecordingName;

                var recPages = recordings[recIndex].ToCSV();
                for (int pageIndex = 0; pageIndex < recPages.Length; pageIndex++)
                {
                    pages[pageIndex][recIndex] = recPages[pageIndex];
                }
            }

            return new IO.CSVPage[7]{
                new IO.CSVPage("Recordings", recordingsPage),
                IO.CSVPage.Combine("Subjects", "Recording", pages[0]),
                IO.CSVPage.Combine("MetaData", "Recording", pages[1]),
                IO.CSVPage.Combine("CustomEvents", "Recording", pages[2]),
                IO.CSVPage.Combine("PositionData", "Recording", pages[3]),
                IO.CSVPage.Combine("RotationData", "Recording", pages[4]),
                IO.CSVPage.Combine("LifeCycleEvents", "Recording", pages[5])
            };
        }

    }

}