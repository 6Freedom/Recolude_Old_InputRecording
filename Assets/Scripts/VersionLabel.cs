using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionLabel : MonoBehaviour
{
    private TextMeshProUGUI label; 
    // Start is called before the first frame update
    void Start()
    {
        label = GetComponent<TextMeshProUGUI>();
        label.text = $"V{Application.version}";
    }
}
