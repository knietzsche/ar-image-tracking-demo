using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugOverlay : MonoBehaviour
{
    private const float ShowOutputDuration = 10f;

    private static Action<string> _log;

    private List<string> _messageList = new List<string>();
    private Text _text;

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        _log += OnLog;
    }

    private void OnDisable()
    {
        _log -= OnLog;

        StopAllCoroutines();
        _text.text = string.Empty;
    }

    public static void Log(string message)
    {
        _log?.Invoke(message);
    }

    private void OnLog(string value)
    {
        StartCoroutine(ShowMessage(value));
    }

    private IEnumerator ShowMessage(string message)
    {
        _messageList.Add($"[ {DateTime.Now.TimeOfDay.ToString().Substring(0, 8)} ] {message}");
        UpdateText();

        yield return new WaitForSeconds(ShowOutputDuration);

        _messageList.Remove(message);
        UpdateText();
    }

    private void UpdateText()
    {
        var text = string.Empty;

        foreach (var message in _messageList)
        {
            if (!string.IsNullOrEmpty(text)) { text += Environment.NewLine; }
            text += message;
        }
        _text.text = text;
    }
}
