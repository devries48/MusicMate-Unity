using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ToolbarImportController : ToolbarControllerBase
{
    [SerializeField] ToolbarButtonAnimator _scanFolderButton;
    [SerializeField] ToolbarButtonAnimator _scanLastFmButton;

    readonly float _checkRunningInterval = 10f;

    #region MusicMate Base Class Methods
    protected override void RegisterEventHandlers()
    {
        _scanFolderButton.OnButtonClick.AddListener(OnScanFolderClicked);
    }

    protected override void UnregisterEventHandlers()
    {
        CancelInvoke();

        _scanFolderButton.OnButtonClick.RemoveListener(OnScanFolderClicked);
    }
    #endregion

    #region ToolbarController Base Class Methods
    protected override void InitElements() => _scanFolderButton.IsInteractable=true;

    protected override void SetElementStates()
    {
        if (_scanFolderButton.CanShowSpinner)
        {
            _scanFolderButton.ShowSpinner();
            ApiService.FolderImportStart((callback) => FolderImportCallback(callback));
            InvokeRepeating(nameof(CheckImportRunning), _checkRunningInterval, _checkRunningInterval);
        }
        else if (_scanFolderButton.CanHideSpinner)
        {
            CancelInvoke(nameof(CheckImportRunning));
            _scanFolderButton.HideSpinner();
        }
    }
    #endregion

    void OnScanFolderClicked()
    {
        _scanFolderButton.IsSpinning = true;
        SetElementStates();
    }

    void CheckImportRunning()
    {
        ApiService.IsFolderImportRunning((isRunning) =>
        {
            if (_scanFolderButton.IsSpinning != isRunning)
            {
                _scanFolderButton.IsSpinning = isRunning;
                SetElementStates();
            }
        });
    }

    void FolderImportCallback(string result)
    {
        print("Folder import: " + result);
    }
}