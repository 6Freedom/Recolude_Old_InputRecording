using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using RecordAndPlay.Record;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Dictionary<string, UnityAction> InputActions = new Dictionary<string, UnityAction>();
    public Dictionary<string, UnityAction<Vector2>> DirectionalInputActions = new Dictionary<string, UnityAction<Vector2>>();

    public Recorder recorder;

    public void AddInput(InputActionReference newInput)
    {
        if (InputActions.ContainsKey(newInput.action.name))
            return;

        InputActions.Add(newInput.action.name, null);
        newInput.action.Enable();
        newInput.action.performed += ReadAction;
    }

    public void AddVectorInput(InputActionReference newInput)
    {
        if (DirectionalInputActions.ContainsKey(newInput.action.name))
            return;

        DirectionalInputActions.Add(newInput.action.name, null);
        newInput.action.Enable();
        newInput.action.performed += ReadVectorAction;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ReadAction(InputAction.CallbackContext context)
    {
        ReadAction(context.action.name);
        if (recorder.CurrentlyRecording())
            recorder.CaptureCustomEvent("Input", new Dictionary<string, string> { {"input", context.action.name } });
    }

    public void ReadVectorAction(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        ReadAction(context.action.name, value);

        if (recorder.CurrentlyRecording())
            recorder.CaptureCustomEvent("VectorInput", new Dictionary<string, string> { { "input", context.action.name }, { "x", value.x.ToString() }, { "y", value.y.ToString() } });
    }

    public void ReadAction(string actionName)
    {
        if (InputActions.ContainsKey(actionName))
            InputActions[actionName].Invoke();
    }

    public void ReadAction(string actionName, Vector2 value)
    {
        if (DirectionalInputActions.ContainsKey(actionName))
            DirectionalInputActions[actionName].Invoke(value);
    }
}
