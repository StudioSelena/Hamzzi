using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class StealSeedUI : ViewBase
{
    [Header("버튼")]
    [SerializeField] private UIButton Button_CloseBack;
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_StealSeed;

    [Header("숫자 텍스트")]
    [SerializeField] private TMP_Text Text_FirstNumber;
    [SerializeField] private TMP_Text Text_SecondNumber;
    [SerializeField] private TMP_Text Text_ThirdNumber;

    private CancellationTokenSource _cancelToken;
    private int _stealSeedCount;

    private void OnEnable()
    {
        Button_CloseBack.BindOnClickButtonEvent(OnClick_Close);
        Button_Close.BindOnClickButtonEvent(OnClick_Close);
        Button_StealSeed.BindOnClickButtonEvent(OnClick_StealSeed);
    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseStealSeedUI();
    }

    private void OnClick_StealSeed()
    {
        PlayRandomNumber().Forget();
    }

    private async UniTask PlayRandomNumber()
    {
        _cancelToken = new CancellationTokenSource();

        float duration = 5f;
        float interval = 0.08f;
        float elapsedTime = 0f;

        int firstnumber = 0;
        int secondnumber = 0;
        int thirdnumber = 0;

        while (elapsedTime < duration)
        {
            firstnumber = UnityEngine.Random.Range(0, 10);
            secondnumber = UnityEngine.Random.Range(0, 10);
            thirdnumber = UnityEngine.Random.Range(0, 10);

            Text_FirstNumber.text = firstnumber.ToString();
            Text_SecondNumber.text = secondnumber.ToString();
            Text_ThirdNumber.text = thirdnumber.ToString();

            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: _cancelToken.Token);

            elapsedTime += interval;
        }

        _stealSeedCount = (firstnumber * 100) + (secondnumber * 10) + thirdnumber;

        await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: _cancelToken.Token);
        UIManager.Instance.OpenStealSeedResultUI(_stealSeedCount);
    }
}


