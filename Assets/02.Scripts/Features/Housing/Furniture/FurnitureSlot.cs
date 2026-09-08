using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureSlot : MonoBehaviour
{
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Button Button_Select;
    [SerializeField] private TextMeshProUGUI Text_Count;

    private ItemData _data;
    private HousingViewModel _housingVM;
    private FurnitureSlotViewModel _furnitureSlotVm;

    private void Awake()
    {
        Button_Select.onClick.AddListener(OnClickSelect);
    }

    private void OnDisable()
    {
        if (_furnitureSlotVm != null)
        {
            _furnitureSlotVm.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    public void Bind(FurnitureSlotViewModel furnitureSlotVm, HousingViewModel housingVM)
    {
        if (_furnitureSlotVm != null)
        {
            _furnitureSlotVm.PropertyChanged -= OnPropertyChanged_View;
        }

        var itemData = GameDataManager.Instance.GetData<ItemData>(furnitureSlotVm.ItemDataId);
        if(itemData == null)
        {
            return;
        }

        _data = itemData;
        _housingVM = housingVM;
        _furnitureSlotVm = furnitureSlotVm;

        _furnitureSlotVm.PropertyChanged += OnPropertyChanged_View;

        Image_Icon.sprite = furnitureSlotVm.IconSprite;
        Text_Count.text = furnitureSlotVm.StackCount.ToString();
    }

    private void OnPropertyChanged_View(object sender,PropertyChangedEventArgs e)
    {
        switch(e.PropertyName)
        {
            case nameof(FurnitureSlotViewModel.StackCount):
                Text_Count.text = _furnitureSlotVm.StackCount.ToString();
                break;
        }
    }

    private void OnClickSelect()
    {
        if (_housingVM.CurrentViewMode == HousingViewMode.Garden)
        {
            _housingVM.SelectGardenFurniture(_data, Vector2Int.one);
        }
        else if (_housingVM.TargetRoom != null)
        {
            _housingVM.SelectFurniture(_data, Vector2Int.one, _housingVM.TargetRoom);
        }
    }
}
