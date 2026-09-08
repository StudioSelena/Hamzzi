using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkGachaService
{
    private GachaViewModel _gachaViewModel;

    public GachaViewModel GetGachaViewModel()
    {
        if(_gachaViewModel == null)
        {
            var gachaViewModel = new GachaViewModel();
            SetGachaViewModel(gachaViewModel);
            _gachaViewModel = gachaViewModel;
        }

        return _gachaViewModel;
    }

    private void SetGachaViewModel(GachaViewModel vm)
    {
        // 뷰모데 초기화
        HamsterViewModel hamsterViewModel = ServiceManager.Instance.CollectionService.GetHamsterViewModel();
        List<string> allHamsterList = hamsterViewModel.AllHamsterIdList;

        foreach(string hamsterId in allHamsterList)
        {
            HamsterData hamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);
            switch (hamsterData.HamsterTier)
            {
                case HamsterTier.SS:
                    vm.AddHasterIdByTierList(HamsterTier.SS, hamsterId);
                    break;
                case HamsterTier.S:
                    vm.AddHasterIdByTierList(HamsterTier.S, hamsterId);
                    break;
                case HamsterTier.A:
                    vm.AddHasterIdByTierList(HamsterTier.A, hamsterId);
                    break;
            }
        }
    }

    public HamsterSave DrawGacha()
    {
        HamsterTier drawTier = DrawTier();

        List<string> hamsterList = _gachaViewModel.HamsterIdByTierList[drawTier];
        if (hamsterList == null)
            return null;

        // 햄스터 몸 랜덤 뽑기
        int hamsterCount = hamsterList.Count;
        int randomIndex = Random.Range(0, hamsterCount);
        string drawHamsterId = hamsterList[randomIndex];

        // 햄스터 얼굴 랜덤 뽑기
        var faceList = ServiceManager.Instance.CollectionService.GetHamsterViewModel().AllFaceIdList;
        int faceCount = faceList.Count;
        randomIndex = Random.Range(0, faceCount);
        string drawFaceId = faceList[randomIndex];

        HamsterSave hamsterSave = new HamsterSave();
        hamsterSave.HamsterUID = GameUtil.GenerateUID();
        hamsterSave.UserUID = ServiceManager.Instance.LoginService.GetViewModel().UserUID;
        hamsterSave.HamsterId = drawHamsterId;
        hamsterSave.FaceId = drawFaceId;

        return hamsterSave;
    }

    private HamsterTier DrawTier()
    {
        int randomValue = Random.Range(0, 100);

        if (randomValue < GachaViewModel.SSProbability)
        {
            return HamsterTier.SS;
        }
        else if(randomValue < GachaViewModel.SSProbability + GachaViewModel.SProbability)
        {
            return HamsterTier.S;
        }
        else
        {
            return HamsterTier.A;
        }
    }
}