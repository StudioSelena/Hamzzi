using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ShopCategory
{
    None,
    All,
    Furniture,
    Play,
    Decor
}

public class ShopUI : ViewBase
{
    [Header("아이템 슬롯 정보")]
    [SerializeField] private GameObject Prefab_ItemSlot;
    [SerializeField] private Transform Transform_SlotRoot;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_DefaultIcon;
    [SerializeField] private TMP_Text Text_ItemName;
    [SerializeField] private TMP_Text Text_ItemDescription;
    [SerializeField] private TMP_Text Text_ItemEffect;
    [SerializeField] private TMP_Text Text_ItemPrice;

    [Header("상위 카테고리")]
    [SerializeField] private UIButton Button_AllCategory;
    [SerializeField] private UIButton Button_FurnitureCategory;
    [SerializeField] private UIButton Button_PlayCategory;
    [SerializeField] private UIButton Button_DecorCategory;

    [Header("카테고리 선택 이미지")]
    [SerializeField] private Image Image_SelectedAllCategory;
    [SerializeField] private Image Image_SelectedFurnitureCategory;
    [SerializeField] private Image Image_SelectedPlayCategory;
    [SerializeField] private Image Image_SelectedDecorCategory;

    [Header("하위 카테고리")]
    [SerializeField] private GameObject SubCategoryArea;
    [SerializeField] private GameObject Button_SubCategory;
    [SerializeField] private Transform Transform_ButtonRoot;

    [Header("버튼")]
    [SerializeField] private UIButton Button_CloseShopUI;
    [SerializeField] private UIButton Button_BuyItem;

    private ShopViewModel _shopVm;
    private string _selectedSubCategory;

    private ShopCategory _selectedCategory = ShopCategory.All;

    private Dictionary<long, ShopSlotUI> _itemSlotList = new Dictionary<long, ShopSlotUI>();
    private List<string> _subCategoryNameList = new List<string>();
    private List<ShopSubCategoryUI> _subCategoryButtonList = new List<ShopSubCategoryUI>();

    private void OnEnable()
    {
        Button_AllCategory.BindOnClickButtonEvent(OnClick_AllCategory);
        Button_FurnitureCategory.BindOnClickButtonEvent(OnClick_FurnitureCategory);
        Button_PlayCategory.BindOnClickButtonEvent(OnClick_PlayCategory);
        Button_DecorCategory.BindOnClickButtonEvent(OnClick_DecorCategory);

        Button_CloseShopUI.BindOnClickButtonEvent(OnClick_CloseShopUI);
        Button_BuyItem.BindOnClickButtonEvent(OnClick_BuyItem);

        InitShopUI();
    }

    private void OnDisable()
    {
        if(_shopVm != null)
        {
            _shopVm.PropertyChanged -= OnPropChanged_ShopView;
        }

        ClearSlotList();
        ClearSubCategoryList();
        ResetItemInfo();
        ResetSelectedData();
    }

    private void InitShopUI()
    {
        _selectedCategory = ShopCategory.All;
        SetSelectedCategory(_selectedCategory);
        SetSubCategory(_selectedCategory);
        Button_BuyItem.SetInteractable(false);
        SetShopItemSlotOnEnable();
    }

    private void OnClick_AllCategory()
    {
        SetShopLayoutByCategory(ShopCategory.All);
        Image_SelectedAllCategory.gameObject.SetActive(true);
    }

    private void OnClick_FurnitureCategory()
    {
        SetShopLayoutByCategory(ShopCategory.Furniture);
    }

    private void OnClick_PlayCategory()
    {
        SetShopLayoutByCategory(ShopCategory.Play);
    }

    private void OnClick_DecorCategory()
    {
        SetShopLayoutByCategory(ShopCategory.Decor);
    }

    private void OnClick_CloseShopUI()
    {
        UIManager.Instance.CloseShopUI();
        UIManager.Instance.OpenInGameUI();
    }

    private void OnClick_BuyItem()
    {
        if(_shopVm.SelectedSlot == null)
        {
            return;
        }

        ServiceManager.Instance.ShopService.BuyItem();
    }

    private void SetShopLayoutByCategory(ShopCategory category)
    {
        _selectedCategory = category;
        SetSelectedCategory(_selectedCategory);
        SetSubCategory(category);

        ResetItemSlotAndCreateAll();
    }
    private void SetSelectedCategory(ShopCategory category)
    {
        Image_SelectedAllCategory.gameObject.SetActive(category == ShopCategory.All);
        Image_SelectedFurnitureCategory.gameObject.SetActive(category == ShopCategory.Furniture);
        Image_SelectedPlayCategory.gameObject.SetActive(category == ShopCategory.Play);
        Image_SelectedDecorCategory.gameObject.SetActive(category == ShopCategory.Decor);
    }

    private void SetSubCategory(ShopCategory category)
    {
        if(category == ShopCategory.All)
        {
            SubCategoryArea.SetActive(false);
        }
        else
        {
            SubCategoryArea.SetActive(true);
            CreateSubCategoryButton(category);
        }
    }

    private void CreateSubCategoryButton(ShopCategory category)
    {
        ClearSubCategoryList();

        //[TODO] 뷰모델로 빼보자!
        foreach (var itemKv in _shopVm.ItemList)
        {
            var slotVm = itemKv.Value;

            if (slotVm.Category != category.ToString())
            {
                continue;
            }

            if (_subCategoryNameList.Contains(slotVm.SubCategory))
            {
                continue;
            }

            _subCategoryNameList.Add(slotVm.SubCategory);
        }

        foreach(var subCategory in _subCategoryNameList)
        {
            var gObj = Instantiate(Button_SubCategory, Transform_ButtonRoot);
            if(gObj == null)
            {
                return;
            }

            var component = gObj.GetComponent<ShopSubCategoryUI>();
            if(component == null)
            {
                return;
            }

            component.SetSubCategory(subCategory);
            component.BindSubCategorySelectEvent(OnSubCategorySelected);
            _subCategoryButtonList.Add(component);
        }

        if (_subCategoryNameList.Count > 0)
        {
            _selectedSubCategory = _subCategoryNameList[0];
            _subCategoryButtonList[0].SetSelected(true) ;
        }
    }

    private void SetShopItemSlotOnEnable()
    {
        ClearSlotList();

        FindShopViewModelAndBind();
    }

    private void FindShopViewModelAndBind()
    {
        var shopVm = ServiceManager.Instance.ShopService.GetShopViewModel();
        _shopVm = shopVm;

        if(_shopVm.ItemList == null || _shopVm.ItemList.Count == 0)
        {
            Debug.LogWarning("보유한 아이템이 없습니다");
            return;
        }

        _shopVm.PropertyChanged += OnPropChanged_ShopView;
        _shopVm.InvokeOnceOnInit();
    }

    private void OnPropChanged_ShopView(object sender, PropertyChangedEventArgs e)
    {
        switch(e.PropertyName)
        {
            case nameof(ShopViewModel.ItemList):
                ResetItemSlotAndCreateAll();
                break;
            case nameof(ShopViewModel.SelectedSlot):
                UpdateItemDetailInfo(_shopVm.SelectedSlot);
                break;
        }
    }

    private void OnSubCategorySelected(string subCategory)
    {
        _selectedSubCategory = subCategory;

        for (int i = 0; i < _subCategoryButtonList.Count; i++)
        {
            ShopSubCategoryUI subCategoryUI = _subCategoryButtonList[i];

            if (subCategoryUI.SubCategory == _selectedSubCategory)
            {
                subCategoryUI.SetSelected(true);
            }
            else
            {
                subCategoryUI.SetSelected(false);
            }
        }

        ResetItemSlotAndCreateAll();
    }

    private void ResetItemSlotAndCreateAll()
    {
        ClearSlotList();

        if (_shopVm == null || _shopVm.ItemList == null)
        {
            return;
        }

        foreach (var itemKv in _shopVm.ItemList)
        {
            var slotVm = itemKv.Value;

            if (_selectedCategory == ShopCategory.All)
            {
                CreateItemSlot(slotVm);
                continue;
            }

            if(slotVm.Category == _selectedCategory.ToString())
            {
                if (slotVm.SubCategory == _selectedSubCategory)
                {
                    CreateItemSlot(slotVm);
                }
            }
        }
    }

    private void CreateItemSlot(ShopSlotViewModel slotVm)
    {
        var gObj = Instantiate(Prefab_ItemSlot, Transform_SlotRoot);
        if(gObj == null)
        {
            return;
        }

        var slotView = gObj.GetComponent<ShopSlotUI>();
        if(slotView == null)
        {
            return;
        }

        slotView.BindSlotViewModel(slotVm);

        _itemSlotList.Add(slotVm.ItemUniqueId, slotView);
        slotView.BindSlotSelectEvent(OnChildSlotSelected);
    }

    private void OnChildSlotSelected(ShopSlotViewModel slotVm)
    {
        if(slotVm == null)
        {
            return;
        }

        _shopVm.SelectedSlot = slotVm;
        Button_BuyItem.SetInteractable(true);
        
        foreach(var itemKv in _itemSlotList)
        {
            var slotView = itemKv.Value;
            bool isSelected = (itemKv.Key == slotVm.ItemUniqueId);
            slotView.SetSelectedSlot(isSelected);
        }
    }

    private void UpdateItemDetailInfo(ShopSlotViewModel slotVm)
    {
        if (slotVm == null)
        {
            ResetItemInfo();
            return;
        }

        Image_DefaultIcon.gameObject.SetActive(false);

        Image_Icon.sprite = slotVm.IconSprite;
        Text_ItemName.text = slotVm.Name;
        Text_ItemEffect.text = $"씨앗 생산 +{slotVm.ItemEffect * 100}%";
        Text_ItemDescription.text = slotVm.Description;
        Text_ItemPrice.text = slotVm.CostAmount.ToString();
    }

    private void ClearSlotList()
    {
        if(_itemSlotList.Count > 0)
        {
            foreach (var slotKv in _itemSlotList)
            {
                var slot = slotKv.Value;
                Destroy(slot.gameObject);
            }

            _itemSlotList.Clear();
        }
    }

    private void ClearSubCategoryList()
    {
        if (_subCategoryButtonList.Count > 0)
        {
            foreach (var buttonObj in _subCategoryButtonList)
            {
                Destroy(buttonObj.gameObject);
            }

            _subCategoryButtonList.Clear();
        }

        _subCategoryNameList.Clear();
    }

    private void ResetItemInfo()
    {
        Image_DefaultIcon.gameObject.SetActive(true);
        Image_Icon.sprite = null;
        Text_ItemName.text = "";
        Text_ItemDescription.text = "";
        Text_ItemPrice.text = "";
        Text_ItemEffect.text = "";
    }

    private void ResetSelectedData()
    {
        _shopVm.SelectedSlot = null;
        _selectedSubCategory = null;
        _selectedCategory = ShopCategory.All;
    }
}
