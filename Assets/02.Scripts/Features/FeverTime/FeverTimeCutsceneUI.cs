// 피버타임 컷신(영상 재생 자리)과 연타 버튼을 담당하는 UI
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FeverTimeCutsceneUI : UIBase
{
    [SerializeField] private RawImage RawImage_Cutscene;
    [SerializeField] private Image Image_Panel;
    [SerializeField] private TMP_Text Text_TapGuide;
    [SerializeField] private UIButton Button_Tap;
    [SerializeField] private Image Image_Fill;
    [SerializeField] private TMP_Text Text_Timer;

    private VideoPlayer _cutsceneVideoPlayer;
    private Tween _pulseTween;

    private Camera _mainCamera;

    private void Awake()
    {
        _cutsceneVideoPlayer = RawImage_Cutscene.GetComponent<VideoPlayer>();
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        Button_Tap.BindOnClickButtonEvent(OnClickTap);
        _cutsceneVideoPlayer.Play();

        StartPulse();
    }

    private void OnDisable()
    {
        _cutsceneVideoPlayer.Stop();
    }

    private void Update()
    {
        var currentFeverTimeData = FeverTimeManager.Instance.GetCurrentFeverTimeData();
        if (currentFeverTimeData == null)
        {
            return;
        }

        float elapsedTime = FeverTimeManager.Instance.GetTapInputElapsedTime();
        float duration = currentFeverTimeData.TapDurationSec;

        Image_Fill.fillAmount = elapsedTime / duration;
        Text_Timer.text = $"남은시간 : {Mathf.CeilToInt(duration - elapsedTime)}초";
    }

    private void OnClickTap()
    {
        FeverTimeManager.Instance.RegisterTap();

        SpawnFeverText().Forget();
    }

    private async UniTask SpawnFeverText()
    {
        RectTransform canvasRect = transform as RectTransform;

        Camera cam = null;
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = rootCanvas.worldCamera;
        }

        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out localPoint))
        {
            GameObject prefab = await GameObjectManager.Instance.CreateObjectAsync("FeverText", "Prefabs/FeverText", Vector3.zero);

            if (prefab.TryGetComponent<RectTransform>(out var rectTransform))
            {
                rectTransform.SetParent(canvasRect, false);
                rectTransform.anchoredPosition = localPoint;
                rectTransform.localScale = Vector3.one;

                if (prefab.TryGetComponent<FeverTextSpawner>(out var textSpawner))
                {
                    textSpawner.PlayBurst(FeverTimeManager.Instance.GetCurrentFeverTimeData().SeedPerTap.ToString());
                }
            }
        }
    }

    private void StartPulse()
    {
        _pulseTween?.Kill();

        _pulseTween = Image_Panel.transform.DOScale(1.025f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}