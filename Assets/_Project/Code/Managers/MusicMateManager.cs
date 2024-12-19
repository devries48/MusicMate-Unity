using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MusicMateManager : SceneSingleton<MusicMateManager>, IMusicMateManager
{
    [Header("Configuration")]
    [SerializeField] AppConfiguration _appConfig;

    [Header("Windows & Animators")]
    [SerializeField] ErrorWindow _errorController;
    [SerializeField] LoginWindow _loginController;
    [SerializeField] MainWindow _mainPage;
    [SerializeField] LogoAnimator _logoAnimator;

    [Header("Elements")]
    [SerializeField] GameObject _connectionSpinner;
    [SerializeField] GameObject[] _inactivateOnStart;

    public AppConfiguration AppConfiguration => _appConfig;

    public IAppState AppState
    {
        get
        {
            _appState ??= new AppState(_appConfig);
            return _appState;
        }
    }

    public Color32 AccentColor => _appConfig.Colors.AccentColor;
    public Color32 DefaultColor => _appConfig.Colors.DefaultColor;
    public Color32 AccentTextColor => _appConfig.Colors.BackgroundColor;
    public Color32 TextColor => _appConfig.Colors.TextColor;
    public Color32 BackgroundColor => _appConfig.Colors.BackgroundColor;


    //readonly float _popupTime = .5f;
    IApiService _service;
    IAppState _appState;
    AnimationManager _animations;

    void Awake()
    {
        InactivateGameObjects();

        _service = ApiService.Instance.GetClient();
        _animations = AnimationManager.Instance;
    }

    void OnEnable() => _service.SubscribeToConnectionChanged(OnConnectionChanged);

    void OnDisable() => _service.UnsubscribeFromConnectionChanged(OnConnectionChanged);

    void Start() => StartCoroutine(DelayAndConnect(.5f));

    public IMusicMateManager GetClient() => this;

    public void Connect()
    {
        if (_loginController.gameObject.activeInHierarchy)
            ShowOrHideLoginPanel(false);

        if (!_connectionSpinner.gameObject.activeInHierarchy)
            ShowSpinner();

        _service.SignIn(_appConfig.ApiServiceUrl, "admin", "123");
    }

    public void ShowError(ErrorType error, string message, string description = "")
    {
        _errorController.SetError(error, message, description);
        ShowOrHideErrorPanel(true);
    }

    public void ShowLogin()
    {
        float delay = 0f;
        if (_errorController.gameObject.activeSelf)
        {
            ShowOrHideErrorPanel(false);
            delay = .5f;
        }

        ShowOrHideLoginPanel(true, delay);
    }

    public void ShowRelease(ReleaseResult releaseModel)
    {
        _mainPage.ShowRelease(releaseModel);
        AppState.ChangeVisiblePart(VisiblePart.ReleaseDetails);
    }

    void HideLogo(bool quit = false) => _logoAnimator.HideLogo(() =>
        {
            if (quit)
                QuitApp();

        });

    void ShowSpinner()
    {
        _connectionSpinner.SetActive(true);

        var images = _connectionSpinner.GetComponentsInChildren<Image>(true);
        var seq = DOTween.Sequence();

        for (int i = 0; i < images.Length; ++i)
        {
            var img = images[i];
            seq.Join(img.DOFade(1f, .2f));
        }
    }

    public void HideSpinner()
    {
        var images = _connectionSpinner.GetComponentsInChildren<Image>(true);
        var seq = DOTween.Sequence();

        for (int i = 0; i < images.Length; ++i)
        {
            var img = images[i];
            seq.Join(img.DOFade(0f, .5f));
        }
        seq.OnComplete(()=>_connectionSpinner.SetActive(false));
    }

    // Hide GameObjects initially not shown.
    void InactivateGameObjects()
    {
        for (int i = 0; i < _inactivateOnStart?.Length; i++)
        {
            _inactivateOnStart[i].SetActive(false);
        }
    }

    void ShowOrHideErrorPanel(bool show) => _animations.WindowErrorVisible(_errorController.gameObject, show);

    void ShowOrHideLoginPanel(bool show, float delay = 0f) => _animations.WindowLoginVisible(_loginController.gameObject, show, delay);

    //void MovePanel(bool show, GameObject panelObj, float hidePivot, float showPivot, float delay = 0f)
    //{
    //    if (show)
    //        panelObj.SetActive(true);

    //    var pivotTo = show ? showPivot : hidePivot;
    //    var easing = show ? Ease.OutBack : Ease.InBack;
    //    var rect = panelObj.GetComponent<RectTransform>();

    //    rect.DOPivotY(pivotTo, _popupTime).SetEase(easing)
    //        .SetDelay(delay)
    //        .OnComplete(() =>
    //        {
    //            if (!show)
    //                panelObj.SetActive(false);
    //        });
    //}

    public void QuitApplication()
    {
        if (_errorController.gameObject.activeInHierarchy)
            ShowOrHideErrorPanel(false);
        else if (_loginController.gameObject.activeInHierarchy)
            ShowOrHideLoginPanel(false);

        if (_logoAnimator.IsLogoActive())
            HideLogo(true);
        else
            QuitApp();
    }

    IEnumerator DelayAndConnect(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Connect();
    }

    /// <summary>
    /// Application.Quit() does not work in the editor so
    /// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
    /// </summary>
    void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    // 
    void OnConnectionChanged(object sender, ConnectionChangedEventArgs e)
    {
        _mainPage.ConnectionChanged(e.Connected);

        if (!e.Connected)
        {
            ShowError(ErrorType.Connection, e.Error, _appConfig.ApiServiceUrl);
            HideSpinner();
        }
        else
            HideLogo();
    }

}