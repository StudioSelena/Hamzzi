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

        while(elapsedTime < duration)
        {
            int firstnumber = UnityEngine.Random.Range(0, 10);
            int secondnumber = UnityEngine.Random.Range(0, 10);
            int thirdnumber = UnityEngine.Random.Range(0, 10);

            Text_FirstNumber.text = firstnumber.ToString();
            Text_SecondNumber.text = secondnumber.ToString();
            Text_ThirdNumber.text = thirdnumber.ToString();

            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: _cancelToken.Token);

            elapsedTime += interval;
        }
        
    }
}


