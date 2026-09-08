using System;
using UnityEngine;

public class KickUI : MonoBehaviour
{
    [SerializeField] private UIButton KickButton;
    [SerializeField] private UIButton ExitButton;

    private CollectionViewModel _collectionViewModel;

    private void Start()
    {
        long userUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        _collectionViewModel = ServiceManager.Instance.CollectionService.GetCollectionViewModel(userUID);
    }

    private void OnEnable()
    {
        KickButton.BindOnClickButtonEvent(OnClickKickButton);
        ExitButton.BindOnClickButtonEvent(OnClickExitButton);
    }

    private void OnDisable()
    {
        KickButton.UnBindOnClickButtonEvent(OnClickKickButton);
        ExitButton.UnBindOnClickButtonEvent(OnClickExitButton);
    }

    private void OnClickKickButton()
    {
        string currentHamsterId = _collectionViewModel.CurrentSelectHamsterId;
        string currentfaceId = _collectionViewModel.CurrentSelectedHamsterFaceId;

        _collectionViewModel.RemoveCollectedHamsterList(currentHamsterId, currentfaceId);

        gameObject.SetActive(false);
    }

    private void OnClickExitButton()
    {
        gameObject.SetActive(false);
    }
}
