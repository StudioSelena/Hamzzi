using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CrossResultView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIButton ExitButton;

    [Header("햄스터 외형")]
    [SerializeField] private RawImage HamsterModelImage;
    [SerializeField] private RectTransform rawImageRect;
    [SerializeField] private CanvasGroup rawImageCanvasGroup;

    private HamsterModelViewModel _hamsterModelViewModel;

    private void Awake()
    {
        _hamsterModelViewModel = ServiceManager.Instance.HamsterModelService.GetHamsterModelViewModel();
        HamsterModelImage.texture = ServiceManager.Instance.HamsterModelService.HamsterTexture;
    }

    private void OnEnable()
    {
        ExitButton.BindOnClickButtonEvent(OnClickExitButton);
    }

    private void OnDisable()
    {
        ExitButton.UnBindOnClickButtonEvent(OnClickExitButton);
    }

    private void OnClickExitButton()
    {
        gameObject.SetActive(false);
    }

    public void PlayGachaResult(string hamsterId, string faceId)
    {
        _hamsterModelViewModel.HamsterId = hamsterId;
        _hamsterModelViewModel.FaceId = faceId;

        ServiceManager.Instance.HamsterModelService.SetHamsterAnimator("DanceTrigger");

        rawImageRect.localScale = Vector3.zero;
        rawImageCanvasGroup.alpha = 1;

        Sequence crossSequence = DOTween.Sequence();

        crossSequence.Append(rawImageRect.DOScale(Vector3.one, 0.7f).SetEase(Ease.OutBack));
    }
}