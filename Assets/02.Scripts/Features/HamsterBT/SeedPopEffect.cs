// 씨앗 채집 시 제자리에서 튀어올랐다 사라지는 팝업 이펙트
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SeedPopEffect : MonoBehaviour
{
    private const float PunchScaleMultiplier = 1.15f;
    private const float PunchDurationSeconds = 0.7f;
    private const float MoveFadeDurationSeconds = 1.5f;
    private const float MoveUpDistance = 1f;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _originalScale;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;

        Color color = _spriteRenderer.color;
        color.a = 1f;
        _spriteRenderer.color = color;

        WaitAndPlayPopEffect().Forget();
    }

    private void OnDisable()
    {
        transform.DOKill();
        _spriteRenderer.DOKill();
    }

    private async UniTaskVoid WaitAndPlayPopEffect()
    {
        // GameObjectManager가 풀에서 꺼낸 직후 위치를 나중에 세팅하기 때문에
        // 한 프레임 기다렸다가 최종 위치 기준으로 연출을 시작해야 함
        await UniTask.Yield(this.GetCancellationTokenOnDestroy());

        PlayPopEffect();
    }

    private void PlayPopEffect()
    {
        float targetY = transform.position.y + MoveUpDistance;

        Sequence popSequence = DOTween.Sequence();
        popSequence.Append(transform.DOScale(_originalScale * PunchScaleMultiplier, PunchDurationSeconds).SetEase(Ease.OutBack));
        popSequence.Join(transform.DOMoveY(targetY, MoveFadeDurationSeconds).SetEase(Ease.OutQuad));
        popSequence.Join(_spriteRenderer.DOFade(0f, MoveFadeDurationSeconds));
        popSequence.OnComplete(Despawn);
    }

    private void Despawn()
    {
        GameObjectManager.Instance.RequestDestroyObject(gameObject);
    }
}