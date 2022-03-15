using UnityEngine;
using UnityEditor;
using System.IO;
using RecordAndPlay.IO;

namespace RecordAndPlay.Editor.Extension.Import
{
    public interface ILoadSelection
    {
        void Render();

        string Error();

        string SuggestedFolderName();

        void Import(string dir); 

    }

}