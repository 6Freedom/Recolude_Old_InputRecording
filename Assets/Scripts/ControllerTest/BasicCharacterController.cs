using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecordAndPlay.Record;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BasicCharacterController : MonoBehaviour
{
    [SerializeField]
    float speed = 10.0f;

    private Recorder _recorder;

    SubjectBehavior _subjectBehavior;

    [SerializeField]
    private GameObject collisionEffect;

    [SerializeField]
    InputActionReference pressAction;

    [SerializeField]
    InputActionReference mouseAction;

    public void Initialize(Recorder p_recorder)
    {
        _recorder = p_recorder;
        _subjectBehavior = SubjectBehavior.Build(gameObject, _recorder, 5, "Player", new Dictionary<string, string>() { }, 0.0001f);
    }

    private void Start()
    {
        InputManager.Instance.AddInput(pressAction);
        InputManager.Instance.InputActions[pressAction.action.name] += LaunchParticles;

        InputManager.Instance.AddVectorInput(mouseAction);
        InputManager.Instance.DirectionalInputActions[mouseAction.action.name] += ReceivePosition;
    }

    void LaunchParticles()
    {
        // Methods subscribed to events are still called when the gameObject is inactive, so need to do an extra check
        if (gameObject.activeInHierarchy)
            Destroy(Instantiate(collisionEffect, transform.position, Quaternion.identity), 3f);
    }

    void ReceivePosition(Vector2 position)
    {
        if (gameObject.activeInHierarchy)
            SixFreedom.Debug.Log("Position is " + position.x.ToString() + " ; " + position.y.ToString());
    }

    public void Move(Vector3 movement)
    {
        transform.position += (movement * Time.deltaTime * speed);
    }
}
