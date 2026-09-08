using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconSettingSlotView : MonoBehaviour
{
    [SerializeField] private Image Image_Icon;
    [SerializeField] private GameObject GameObject_Checkmark; 
    [SerializeField] private UIButton Button_Select;

    private ProfileSettingViewModel _vm;
    private string _myIconPath = "";

    private void OnEnable()
    {
        Button_Select.BindOnClickButtonEvent(OnClickSelect);
    }

    private void OnDisable()
    {
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChanged_Slot;
        }
    }

    public async void SetData(string iconPath, ProfileSettingViewModel vm)
    {
        if (iconPath == "" || vm == null) return;

        _myIconPath = iconPath;
        _vm = vm;

        Sprite loadedSprite = await ResourceManager.Instance.LoadAsset<Sprite>(_myIconPath);
        if (Image_Icon != null && loadedSprite != null)
        {
            Image_Icon.sprite = loadedSprite;
        }

        UpdateCheckmark();

        _vm.PropertyChanged += OnPropChanged_Slot;
    }

    private void OnClickSelect()
    {
        if (_vm != null)
        {
            _vm.RequestChangeIcon(_myIconPath);
        }
    }

    private void OnPropChanged_Slot(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProfileSettingViewModel.SelectedIconPath))
        {
            UpdateCheckmark();
        }
    }

    private void UpdateCheckmark()
    {
        if (_vm == null || GameObject_Checkmark == null) return;

        bool isSelected = (_vm.SelectedIconPath == _myIconPath);
        GameObject_Checkmark.SetActive(isSelected);
    }
}