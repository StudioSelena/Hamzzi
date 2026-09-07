using DG.Tweening;
using TMPro;
using UnityEngine;

public class FeverTextSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_Count;
    [SerializeField] private CanvasGroup CanvasGroup;

    private void OnEnable()
    {
        transform.DOKill();
        CanvasGroup.DOKill();

        CanvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    public void PlayBurst(string text)
    {
        CanvasGroup.DOKill();

        Text_Count.text = $"+ {text}";

        transform.localScale = Vector3.one * 0.7f;
        transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(DestroyText);
    }

    private void DestroyText()
    {
        GameObjectManager.Instance.RequestDestroyObject(gameObject);
    }
}
