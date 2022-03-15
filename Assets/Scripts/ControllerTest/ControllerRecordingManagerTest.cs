using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecordAndPlay.Playback;
using RecordAndPlay.Record;
using RecordAndPlay;
using RecordAndPlay.IO;


public class ControllerRecordingManagerTest : MonoBehaviour, IActorBuilder, IPlaybackCustomEventHandler
{
    private Recorder recorder;

    private PlaybackBehavior playback;

    [SerializeField]
    private BasicCharacterController player;

    [SerializeField]
    private GameObject playerActor;

    [SerializeField]
    List<GameObject> recordedObjects;

    private void Start()
    {
        recorder = ScriptableObject.CreateInstance<Recorder>();
        InputManager.Instance.recorder = recorder;
        player.Initialize(recorder);
    }

    public Actor Build(int actorId, string actorName, Dictionary<string, string> metadata)
    {
        switch (actorName)
        {
            case "Player":
                return new Actor(Instantiate(playerActor), this);
        }
        return null;
    }

    public void OnCustomEvent(SubjectRecording subject, CustomEventCapture customEvent)
    {
        switch (customEvent.Name)
        {
            case "Input":
                string inputName = customEvent.Contents["input"];
                InputManager.Instance.ReadAction(inputName);
                break;

            case "VectorInput":
                string vectorInputName = customEvent.Contents["input"];
                Vector2 vector = new Vector2(float.Parse(customEvent.Contents["x"]), float.Parse(customEvent.Contents["y"]));
                InputManager.Instance.ReadAction(vectorInputName, vector);
                break;

            default:
                Debug.LogWarningFormat("Don't know how to handle event type: {0}", customEvent.Name);
                break;
        }
    }

    private void OnGUI()
    {
        switch (recorder.CurrentState())
        {
            case RecordingState.Recording:
                if (recorder.CurrentlyRecording() && GUI.Button(new Rect(10, 10, 120, 25), "Pause"))
                {
                    recorder.Pause();
                }

                if (GUI.Button(new Rect(10, 40, 120, 25), "Finish"))
                {
                    Recording rec = recorder.Finish();
                    playback = PlaybackBehavior.Build(rec, this, this, true);
                }
                break;

            case RecordingState.Paused:
                if (recorder.CurrentlyPaused() && GUI.Button(new Rect(10, 10, 120, 25), "Resume"))
                {
                    recorder.Resume();
                }

                if (GUI.Button(new Rect(10, 40, 120, 25), "Finish"))
                {
                    playback = PlaybackBehavior.Build(recorder.Finish(), this, this, true);
                }
                break;

            case RecordingState.Stopped:
                if (GUI.Button(new Rect(10, 10, 120, 25), "Start Recording"))
                {
                    recorder.ClearSubjects();
                    recorder.Register(player.gameObject.GetComponent<SubjectBehavior>().GetSubjectRecorder());
                    recorder.Start();
                    if (playback != null)
                    {
                        playback.Stop();
                        Destroy(playback.gameObject);
                    }
                }
                if (playback != null)
                {
                    GUI.Box(new Rect(10, 50, 120, 250), "Playback");
                    if (playback.CurrentlyPlaying() == false && GUI.Button(new Rect(15, 75, 110, 25), "Start"))
                    {
                        foreach (GameObject targetObject in recordedObjects)
                            targetObject.SetActive(false);

                        playback.Play();
                    }

                    if (playback.CurrentlyPlaying())
                    {
                        if (GUI.Button(new Rect(15, 75, 110, 25), "Pause"))
                        {
                            foreach (GameObject targetObject in recordedObjects)
                                targetObject.SetActive(true);

                            playback.Pause();
                        }
                    }

                    GUI.Label(new Rect(55, 105, 100, 30), "Time");
                    GUI.Label(new Rect(55, 125, 100, 30), playback.GetTimeThroughPlayback().ToString("0.00"));
                    playback.SetTimeThroughPlayback(GUI.HorizontalSlider(new Rect(15, 150, 100, 30), playback.GetTimeThroughPlayback(), 0.0F, playback.RecordingDuration()));

                    GUI.Label(new Rect(20, 170, 100, 30), "Playback Speed");
                    GUI.Label(new Rect(55, 190, 100, 30), playback.GetPlaybackSpeed().ToString("0.00"));
                    playback.SetPlaybackSpeed(GUI.HorizontalSlider(new Rect(15, 215, 100, 30), playback.GetPlaybackSpeed(), -8, 8));
                }
                break;
        }
    }
}
