using Cysharp.Threading.Tasks;
using System.ComponentModel;
using UnityEngine;

internal class Hamster3DModelView : MonoBehaviour
{
    [Header("카메라 관련")]
    [SerializeField] private Camera Camera;

    [Header("햄스터 관련")]
    [SerializeField] private Transform _hamsterTransform;
    [SerializeField] private Animator _animator;
    [SerializeField] private SkinnedMeshRenderer FaceMesh;
    [SerializeField] private SkinnedMeshRenderer HamsterMesh;

    private HamsterModelViewModel _hamsterModelViewModel;

    private void Start()
    {
        var hamsterRender = ServiceManager.Instance.HamsterModelService.HamsterTexture;
        Camera.targetTexture = hamsterRender;
    }

    private void OnEnable()
    {
        _hamsterModelViewModel = ServiceManager.Instance.HamsterModelService.GetHamsterModelViewModel();
        _hamsterModelViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnDisable()
    {
        _hamsterModelViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(HamsterModelViewModel.HamsterId):
                SetHamsterMesh();
                break;
            case nameof(HamsterModelViewModel.FaceId):
                SetFaceMesh();
                break;
        }
    }

    private void SetFaceMesh()
    {
        string faceId = _hamsterModelViewModel.FaceId;
        LoadFaceMesh(faceId).Forget();
    }

    private async UniTaskVoid LoadFaceMesh(string faceId)
    {
        var faceData = GameDataManager.Instance.GetData<FaceData>(faceId);
        string facePath = faceData.MaterialPath;

        var faceMarteiral = await ResourceManager.Instance.LoadAsset<Material>(facePath);
        FaceMesh.material = faceMarteiral;
    }

    private void SetHamsterMesh()
    {
        string hamsterId = _hamsterModelViewModel.HamsterId;
        LoadHamsterMesh(hamsterId).Forget();
    }

    private async UniTaskVoid LoadHamsterMesh(string hamsterId)
    {
        var HamsterData = GameDataManager.Instance.GetData<HamsterData>(hamsterId);
        string HamsterPath = HamsterData.MaterialPath;

        var hamsterMarteiral = await ResourceManager.Instance.LoadAsset<Material>(HamsterPath);
        HamsterMesh.material = hamsterMarteiral;
    }

    public Transform GetHamsterTransform()
    {
        return _hamsterTransform;
    }

    public void SetHamsterAnimator(string parameter)
    {
        _animator.SetTrigger(parameter);
    }
}
