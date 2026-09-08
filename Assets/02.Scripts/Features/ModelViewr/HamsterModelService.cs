using Cysharp.Threading.Tasks;
using UnityEngine;

public class HamsterModelService
{
    private HamsterModelViewModel _modelViewrViewModel;
    private Hamster3DModelView _modelView;

    public RenderTexture HamsterTexture;

    public HamsterModelViewModel GetHamsterModelViewModel()
    {
        if (_modelViewrViewModel == null)
        {
            var modelViewrViewModel = new HamsterModelViewModel();
            LoadHamsterTexture().Forget();
            _modelViewrViewModel = modelViewrViewModel;
        }

        return _modelViewrViewModel;
    }

    private async UniTask LoadHamsterTexture()
    {
        HamsterTexture = await ResourceManager.Instance.LoadAsset<RenderTexture>("HamsterTexutre");
        Debug.Log($"Texture : {HamsterTexture}");
    }

    public async UniTask LoadHamsterModel()
    {
        if (_modelView != null)
            return;

        var modelObject = await GameObjectManager.Instance.CreateObjectAsync(string.Empty, "Hamster3DModel", Vector3.one * 1000);
        var view = modelObject.GetComponent<Hamster3DModelView>();
        _modelView = view;
    }

    public Transform GetModelTransform()
    {
        return _modelView.GetHamsterTransform();
    }

    public void SetHamsterAnimator(string parameter)
    {
        _modelView.SetHamsterAnimator(parameter);
    }
}