using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RecordAndPlay.Playback;

namespace RecordAndPlay.Editor.Extension
{

    [CustomEditor(typeof(PlaybackBuilderBehavior))]
    public class PlaybackBuilderBehaviorEditor : UnityEditor.Editor
    {
        public void NameMappingRender(PlaybackBuilderBehavior builder)
        {
            foreach (var keyval in builder.GetNameMappingForDictionaryBuilder())
            {
                var newobj = EditorGUILayout.ObjectField(keyval.Key, keyval.Value, typeof(GameObject), true) as GameObject;
                if (newobj != keyval.Value)
                {
                    builder.SetActorForSubjectName(keyval.Key, newobj);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public void ResourceFolderRender(PlaybackBuilderBehavior builder)
        {
            var newPath = EditorGUILayout.TextField("Resource Path", builder.GetResourceSubpath());
            if (newPath != builder.GetResourceSubpath())
            {
                builder.SetResourceSubpath(newPath);
                EditorUtility.SetDirty(this);
            }

            var found = new HashSet<string>();
            foreach (var subj in builder.GetRecording().SubjectRecordings)
            {
                if (found.Contains(subj.SubjectName) == false)
                {
                    var resourcePath = System.IO.Path.Combine(builder.GetResourceSubpath(), subj.SubjectName);
                    if (Resources.Load(resourcePath) == null)
                    {
                        EditorGUILayout.HelpBox("Resources folder does not contain the asset: " + resourcePath, MessageType.Warning);
                    }
                    found.Add(subj.SubjectName);
                }
            }
        }

        private bool RenderSelectRecordingSelection(PlaybackBuilderBehavior builder)
        {
            var newRec = (Recording)EditorGUILayout.ObjectField("Recording To Replay", builder.GetRecording(), typeof(Recording), true);
            if (newRec != builder.GetRecording())
            {
                builder.SetRecording(newRec);
                EditorUtility.SetDirty(this);
            }
            return builder.GetRecording() != null;
        }

        public override void OnInspectorGUI()
        {
            var builder = (PlaybackBuilderBehavior)target;
            if (builder == null)
            {
                return;
            }

            if (RenderSelectRecordingSelection(builder) == false)
            {
                EditorGUILayout.HelpBox("Specify A Recording", MessageType.Error);
                return;
            }

            var newStrat = (PlaybackBuilderBehavior.ActorBuilderStrategy)EditorGUILayout.EnumPopup("Actor Builder Strategy", builder.Strategy());
            if (newStrat.Equals(builder.Strategy()) == false)
            {
                builder.SetStrategy(newStrat);
                EditorUtility.SetDirty(this);
            }

            switch (builder.Strategy())
            {
                case PlaybackBuilderBehavior.ActorBuilderStrategy.NameMapping:
                    NameMappingRender(builder);
                    break;

                case PlaybackBuilderBehavior.ActorBuilderStrategy.ResourcesFolder:
                    ResourceFolderRender(builder);
                    break;
            }

        }

    }

}