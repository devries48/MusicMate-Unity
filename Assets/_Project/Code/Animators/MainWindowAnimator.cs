using UnityEngine;

public class MainWindowAnimator : MusicMateBehavior
{
    [Header("Panel Controllers")]
    [SerializeField] AudioPlayerController _audioPlayer;
    [SerializeField] GridReleaseController _releaseResult;
    [SerializeField] DetailsAnimator _showDetails;
    [SerializeField] ProvidersController _providers;

    [Header("Toolbar Controllers")]
    [SerializeField] ToolbarApplicationController _applicationToolbar;
    [SerializeField] ToolbarModeController _modeToolbar;
    [SerializeField] ToolbarPartController _searchToolbar;
    [SerializeField] ToolbarImportController _importToolbar;

    State _state;
    float _fadeTime = 0;

    #region Base Class Methods
    protected override void InitializeComponents() => _state = new State();

    protected override void InitializeValues() => ActivatePanels(false);

    protected override void RegisterEventHandlers()
    {
        PlayerService.SubscribeToExpandedChanged(OnAudioPlayerExpandedChanged);
        Manager.AppState.SubscribeToVisiblePartChanged(OnVisiblePartChanged);
    }

    protected override void UnregisterEventHandlers()
    {
        PlayerService.UnsubscribeFromExpandedChanged(OnAudioPlayerExpandedChanged);
        Manager.AppState.UnsubscribeFromVisiblePartChanged(OnVisiblePartChanged);
    }
    #endregion

    public void ConnectionChanged(bool connected)
    {
        if (connected && _fadeTime == 0)
        {
            _fadeTime = 1f;
            ActivatePanels(true);
        }

        Animations.Panel.PlayPanelVisibility(connected, _fadeTime, 0f,
            _audioPlayer.m_canvasGroupExpanded,
            _applicationToolbar.m_CanvasGroup,
            _modeToolbar.m_CanvasGroup,
            _searchToolbar.m_CanvasGroup,
            _importToolbar.m_CanvasGroup);

        // Delay the display of the result for a better experience.
        Animations.Panel.PlayPanelVisibility(connected, _fadeTime, 1.5f, _releaseResult.m_canvasGroup);

        if (connected)
            ApiService.GetInitialReleases(GetInitialReleasesCallback);
    }

    public void ShowRelease(ReleaseResult release)
    {
        if (!_showDetails.isActiveAndEnabled)
        {
            Animations.Panel.PlayDetailsVisibility(true, _showDetails);
            _state.ReleaseDetails = State.States.visible;
        }

        _showDetails.SetRelease(release);
    }

    void ActivatePanels(bool activate)
    {
        _audioPlayer.gameObject.SetActive(activate);
        _releaseResult.gameObject.SetActive(activate);
        _applicationToolbar.gameObject.SetActive(activate);
        _searchToolbar.gameObject.SetActive(activate);
        _importToolbar.gameObject.SetActive(activate);
    }

    void VisibleReleaseResult(bool show)
    {
        Animations.Panel.PlayReleaseGridVisiblity(show, _releaseResult);
        _state.ReleaseResult = show ? State.States.visible : State.States.hidden;
    }

    /// <summary>
    /// Initial result on startup.
    /// </summary>
    void GetInitialReleasesCallback(PagedResult<ReleaseResult> result)
    {
        _releaseResult.SetResult(result);
        Manager.HideSpinner();
    }

    /// <summary>
    /// Change the right margin of the result when the audio players expanded/collapsed state changes.
    /// </summary>
    void OnAudioPlayerExpandedChanged(object sender, ExpandedChangedEventArgs e) => _releaseResult.SetRightMargin(e.IsExpanded);

    /// <summary>
    /// Show or hide the result when a visible part has changed.
    /// </summary>
    void OnVisiblePartChanged(object sender, VisiblePartChangedEventArgs e)
    {
        switch (e.Part)
        {
            case VisiblePart.ReleaseResult:
                VisibleReleaseResult(true);
                break;

            case VisiblePart.ReleaseDetails:
            case VisiblePart.ArtistDetails:
                if (_state.ReleaseResult == State.States.visible)
                    VisibleReleaseResult(false);
                break;

            default:
                break;
        }
    }

    class State
    {
        public State()
        {
            ReleaseResult = States.visible;
            ReleaseDetails = States.hidden;
            ReleaseDetailsArtist = States.hidden;
        }

        public enum States { visible, hidden, moved }

        public States ReleaseResult;
        public States ReleaseDetails;
        public States ReleaseDetailsArtist;
    }
}
