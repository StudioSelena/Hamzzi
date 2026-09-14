using DG.Tweening;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class FurnitureView : MonoBehaviour
{
    [SerializeField] private Renderer[] Renderers;
    [SerializeField] private float YOffset = 0f;

    public FurnitureViewModel FurnitureVM { get; private set; }

    private Dictionary<Renderer, Material[]> _originMaterial = new Dictionary<Renderer, Material[]>();
    private FeverTimeWheel _feverTimeWheel;
    private Vector3 _originScale;

    public float Offset
    {
        get => YOffset;
    }

    private void Awake()
    {
        _originScale = transform.localScale;
        InitRederers();
        _feverTimeWheel = GetComponent<FeverTimeWheel>();
    }

    public void Bind(FurnitureViewModel furnitureVM)
    {
        if (FurnitureVM != null)
        {
            FurnitureVM.PropertyChanged -= OnPropertyChanged_VM;
        }

        FurnitureVM = furnitureVM;

        if (FurnitureVM != null)
        {
            FurnitureVM.PropertyChanged += OnPropertyChanged_VM;
            UpdateAssignHamster();
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();

        if (FurnitureVM != null)
        {
            FurnitureVM.PropertyChanged -= OnPropertyChanged_VM;
        }
    }

    private void OnPropertyChanged_VM(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(FurnitureVM.AssignHamsterID):
                UpdateAssignHamster();
                break;
        }
    }

    public void PlayPlaceAnimation()
    {
        transform.DOKill();
        transform.localScale = _originScale;

        transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0.15f), 0.25f, 7, 1f);

        SoundManager.Instance.PlaySFX("Place_Furniture");
    }

    public void PlayRotationAnimation(float targetAngleY)
    {
        transform.DOKill();
        transform.DORotate(new Vector3(0f, targetAngleY, 0f), 0.2f).SetEase(Ease.OutBack);

        SoundManager.Instance.PlaySFX("Rotate_Furniture");
    }

    private void UpdateAssignHamster()
    {
        if (_feverTimeWheel == null || FurnitureVM == null)
        {
            return;
        }

        string hamsterVal = FurnitureVM.AssignHamsterID;

        if (string.IsNullOrEmpty(hamsterVal))
        {
            _feverTimeWheel.SetHamster(null);
        }
        else
        {
            string targetHamsterID = hamsterVal;

            if (long.TryParse(hamsterVal, out long uid))
            {
                long userUID = ServiceManager.Instance.VisitedUserService.CurrentVisitedUid != 0 ? ServiceManager.Instance.VisitedUserService.CurrentVisitedUid : ServiceManager.Instance.LoginService.GetViewModel().UserUID;
                var collectionVM = ServiceManager.Instance.CollectionService?.GetCollectionViewModel(userUID);

                if (collectionVM != null && collectionVM.CollectedHamsterList.TryGetValue(uid, out var save))
                {
                    targetHamsterID = save.HamsterId;
                }
            }

            HamsterData hamsterData = GameDataManager.Instance.GetData<HamsterData>(targetHamsterID);
            _feverTimeWheel.SetHamster(hamsterData);
        }
    }

    private void InitRederers()
    {
        if (_originMaterial.Count == 0)
        {
            foreach (Renderer renderer in Renderers)
            {
                _originMaterial[renderer] = renderer.sharedMaterials;
            }
        }
    }

    public void SetGhostMode(Material ghost)
    {
        InitRederers();

        foreach (Renderer renderer in Renderers)
        {
            int count = renderer.sharedMaterials.Length;

            Material[] ghostMat = new Material[count];

            for (int i = 0; i < count; i++)
            {
                ghostMat[i] = ghost;
            }

            renderer.materials = ghostMat;
        }
    }

    public void ResetMaterial()
    {
        foreach (var pair in _originMaterial)
        {
            pair.Key.materials = pair.Value;
        }
    }

    public Vector2Int GetFurnitureSize(float subCellSize = 0.25f)
    {
        InitRederers();

        if (Renderers == null || Renderers.Length == 0)
        {
            return Vector2Int.one;
        }

        Bounds bounds = Renderers[0].bounds;

        for (int i = 1; i < Renderers.Length; i++)
        {
            if (Renderers[i] != null)
            {
                bounds.Encapsulate(Renderers[i].bounds);
            }
        }

        int sizeX = Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / subCellSize));
        int sizeY = Mathf.Max(1, Mathf.RoundToInt(bounds.size.z / subCellSize));

        return new Vector2Int(sizeX, sizeY);
    }
}
