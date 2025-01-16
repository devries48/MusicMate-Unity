using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RowTrackAnimator : MusicMateBehavior, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI _nrText;
    [SerializeField] Marquee _titleMarque;
    [SerializeField] TextMeshProUGUI _durationText;

    //public delegate void TrackClickHandler(TrackResult track, int position);
    //public event TrackClickHandler OnTrackClicked;
    //public event TrackClickHandler OnTrackDoubleClicked;

    public TrackResult m_track;

    Image _backgroundImage;
    bool _isActive;
    bool _isHover;

    GridTrackController _parent;
    Coroutine _clickCoroutine;

    const float DoubleClickThreshold = 0.3f; // Time in seconds to detect a double-click

    public bool IsActive { get { return _isActive; } set { _isActive = value; SetColors(); } }
    public bool IsSelected { get; set; } = false;

    protected override void InitializeComponents() => _backgroundImage = GetComponent<Image>();

    public void Initialize(TrackResult track, int pos, GridTrackController parent)
    {
        m_track = track;
        _parent = parent;

        _nrText.text = pos.ToString();
        _titleMarque.SetText(track.Title);
        _durationText.text = track.DurationString;
    }

    void SetColors()
    {
        var textcolor = _isHover ? Manager.AccentTextColor : IsActive ? Manager.AccentColor : Manager.TextColor;

        _nrText.color = textcolor;
        _titleMarque.SetColor(textcolor);
        _durationText.color = textcolor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHover = true;
        _backgroundImage.color = IsActive ? Manager.AccentColor : Manager.TextColor;
        _backgroundImage.enabled = true;

        SetColors();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHover = false;
        _backgroundImage.enabled = false;

        SetColors();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            if (_clickCoroutine != null)
                StopCoroutine(_clickCoroutine);

            HandleDoubleClick();
        }
        else
            _clickCoroutine = StartCoroutine(HandleSingleClick());
    }

    IEnumerator HandleSingleClick()
    {
        yield return new WaitForSeconds(DoubleClickThreshold);

        IsSelected = !IsSelected;

        Animations.RowClick(this);
        _parent.ChangeSelection(this); 
    }

    void HandleDoubleClick()
    {
        _parent.ClearSelection();

        Animations.RowClick(this);
        PlayerService.Play(m_track);
    }
}
