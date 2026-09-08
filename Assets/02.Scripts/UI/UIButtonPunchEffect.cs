// 버튼 클릭 시 순간적으로 커졌다가 줄어드는 펀치 스케일 이펙트 (공용 컴포넌트)
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonPunchEffect : MonoBehaviour
{
    [SerializeField] private Button Button_Target;
    [SerializeField] private float _punchScaleMultiplier = 1.05f;
    [SerializeField] private float _scaleUpDurationSeconds = 0.03f;
    [SerializeField] private float _scaleDownDurationSeconds = 0.05f;

    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private Sequence _punchSequence;

    private void Awake()
    {
        if (Button_Target == null)
        {
            Button_Target = this.GetComponentInChildren<Button>(true);
        }

        _rectTransform = this.GetComponent<RectTransform>();

        if (_rectTransform != null)
        {
            _originalScale = _rectTransform.localScale;
        }
    }

    private void OnEnable()
    {
        if (Button_Target == null)
        {
            return;
        }

        Button_Target.onClick.AddListener(OnClickButton);
    }

    private void OnDisable()
    {
        if (Button_Target != null)
        {
            Button_Target.onClick.RemoveListener(OnClickButton);
        }

        KillPunchSequence();

        if (_rectTransform != null)
        {
            _rectTransform.localScale = _originalScale;
        }
    }

    private void OnClickButton()
    {
        PlayPunchEffect();
    }

    private void PlayPunchEffect()
    {
        if (_rectTransform == null)
        {
            return;
        }

        KillPunchSequence();
        _rectTransform.localScale = _originalScale;

        _punchSequence = DOTween.Sequence();
        _punchSequence.Append(_rectTransform.DOScale(_originalScale * _punchScaleMultiplier, _scaleUpDurationSeconds).SetEase(Ease.OutQuad));
        _punchSequence.Append(_rectTransform.DOScale(_originalScale, _scaleDownDurationSeconds).SetEase(Ease.InQuad));
    }

    private void KillPunchSequence()
    {
        if (_punchSequence == null)
        {
            return;
        }

        _punchSequence.Kill();
        _punchSequence = null;
    }
}