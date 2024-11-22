﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public interface IMusicMateManager
{
    Color32 AccentColor { get; }
    Color32 ForegroundColor { get; }

    void ChangeState(Button button, bool enabled, bool? isPlaying);
    void ChangeState(Image image, bool enabled, bool? isPlaying = null);
    void ChangeState(TextMeshProUGUI text, bool enabled);
    void ChangeStates(Button[] buttons, bool enabled, bool? isPlaying = null);
    void ChangeStates(Image[] images, bool enabled, bool isPlaying);
    void ChangeStates(TextMeshProUGUI[] texts, bool enabled);
    void ChangeStates(Slider[] sliders, bool enabled);
    void HideSpinner();
    void ShowError(ErrorType error, string message, string description = "");
    void ShowLogin();
    void ShowRelease(ReleaseResult releaseModel);
    void ChangeVisiblePart(VisiblePart part);
    void SubscribeToVisiblePartChanged(VisiblePartChangedEventHandler handler);
    void UnsubscribeFromVisiblePartChanged(VisiblePartChangedEventHandler handler);
    void QuitApplication();
}

#region EventHandlers & EventArgs
public delegate void VisiblePartChangedEventHandler(object sender, VisiblePartChangedEventArgs e);

public class VisiblePartChangedEventArgs : EventArgs
{
    public VisiblePartChangedEventArgs(VisiblePart part)
    {
        Part = part;
    }

    public VisiblePart Part { get; }
}
#endregion
