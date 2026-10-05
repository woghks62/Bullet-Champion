using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class ShopManager : MonoBehaviour
{
    public bool isTest => GameManager.instance != null && GameManager.instance.isTest;

    public List<ShopItem> allItems;
    public Transform itemParent;
    public GameObject itemPrefab;
    private List<ShopItem> currentItems = new List<ShopItem>();

    public static ShopManager instance;
    public GameObject tooltipPanel;
    public Text tooltipText;
    public Button buyButton;
    public Text playerMoneyText;
    public Button reloadButton;
    private Text reloadButtonText;
    private int refreshCost = 30;
    public GameObject purchaseMessagePanel;
    public GameObject savePanel;

    [Header("인벤 가득찼을때 팝업 프리팹")]
    public GameObject ItemFullPopUp;
    public Transform popupParent;

    [Header("인벤토리 전용 프리팹")]
    public GameObject inventoryItemPrefab;
    [Header("슬롯 설정")]
    public GameObject slotPrefab;
    public int slotCount;
    [Header("인벤토리 패널")]
    public GameObject inventoryPanel;

    GameObject slotPanel;
    [Header("장비 설정")]
    public GameObject HeadEqip;
    public GameObject TopEqip;
    public GameObject GlovesEqip;
    public GameObject WeaponEqip;
    public GameObject BottomEqip;
    public GameObject ShoesEqip;
    public GameObject Accessory1Eqip;
    public GameObject Accessory2Eqip;
    public GameObject Accessory3Eqip;
    public GameObject SpecialEqip;
    public GameObject Weapone2Eqip;

    public GameObject inventoryItem;

    int slotAmount;
    public List<ShopItem> items = new List<ShopItem>();
    public List<GameObject> slots = new List<GameObject>();

    void Awake()
    {
        Debug.Log("[TitleManager] isTest 값: " + GameManager.instance.isTest);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        tooltipPanel = GameObject.Find("Canvas/ItemTooltip") ?? FindObjectOfType<Canvas>().transform.Find("ItemTooltip")?.gameObject;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        else Debug.LogError("tooltipPanel을 찾을 수 없습니다!");

        if (purchaseMessagePanel != null) purchaseMessagePanel.SetActive(false);
        else Debug.LogWarning("purchaseMessagePanel이 연결되지 않았습니다!");

        slotCount = 32;
    }

    void Start()
    {
        if (GameManager.instance.globalItemList == null || GameManager.instance.globalItemList.Count == 0)
        {
            GameManager.instance.globalItemList = allItems;
        }
        allItems = GameManager.instance.globalItemList;

        GenerateShopItems();
        UpdatePlayerMoneyUI();

        Debug.Log(inventoryPanel);
        slotPanel = inventoryPanel.transform.Find("Right Inventory/Scroll View/Viewport/SlotContent").gameObject;
        CreateSlots();
        CreateEquipSlot();

        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(RefreshShop);
            reloadButtonText = reloadButton.GetComponentInChildren<Text>();
            if (reloadButtonText != null) UpdateReloadButtonText();
        }
        else
        {
            Debug.LogError("새로고침 버튼을 찾을 수 없습니다!");
        }

        if (InventoryManager.InventoryInstance != null)
        {
            InventoryManager.InventoryInstance.RebuildInventoryUI();
            Debug.Log("[ShopManager] 인벤토리 UI 복원 완료");
        }
    }

    void GenerateShopItems()
    {
        foreach (Transform child in itemParent)
        {
            Destroy(child.gameObject);
        }

        currentItems.Clear();
        List<ShopItem> availableItems = allItems.FindAll(item => item.IsAvailableInShop());

        for (int i = 0; i < availableItems.Count - 1; i++)
        {
            int randIndex = Random.Range(i, availableItems.Count);
            (availableItems[i], availableItems[randIndex]) = (availableItems[randIndex], availableItems[i]);
        }

        int itemCount = Mathf.Min(6, availableItems.Count);
        for (int i = 0; i < itemCount; i++)
        {
            ShopItem item = availableItems[i];
            currentItems.Add(item);

            GameObject newItem = Instantiate(itemPrefab, itemParent);
            Button button = newItem.GetComponent<Button>();

            newItem.transform.Find("ItemName").GetComponent<Text>().text = item.itemName;
            newItem.transform.Find("ItemPrice").GetComponent<Text>().text = item.price.ToString();
            newItem.transform.Find("ItemIcon").GetComponent<Image>().sprite = item.itemIcon;

            ShopItemUI itemUI = newItem.AddComponent<ShopItemUI>();
            itemUI.itemData = item;

            button.onClick.AddListener(() => OnBuyButtonClicked(item, newItem));
        }
    }

    public void UpdatePlayerMoneyUI()
    {
        if (playerMoneyText != null)
        {
            playerMoneyText.text = "Gold: " + GameManager.instance.playerMoney.ToString();
        }
    }

    public bool CanAffordItem(int price)
    {
        return GameManager.instance.playerMoney >= price;
    }

    void CreateSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            items.Add(new ShopItem { id = -1 });
            slots.Add(Instantiate(slotPrefab));
            slots[i].GetComponent<Slot>().id = i;
            slots[i].transform.SetParent(slotPanel.transform, false);
        }
    }

    void CreateEquipSlot()
    {
        AddEquipSlot(HeadEqip, "Head_Equip");
        AddEquipSlot(TopEqip, "Top_Equip");
        AddEquipSlot(GlovesEqip, "Gloves_Equip");
        AddEquipSlot(WeaponEqip, "Weapon_Equip");
        AddEquipSlot(BottomEqip, "Bottom_Equip");
        AddEquipSlot(ShoesEqip, "Shoes_Equip");
        AddEquipSlot(Accessory1Eqip, "Accessory_Equip");
        AddEquipSlot(Accessory2Eqip, "Accessory_Equip");
        AddEquipSlot(Accessory3Eqip, "Accessory_Equip");
        AddEquipSlot(SpecialEqip, "Special_Equip");
        AddEquipSlot(Weapone2Eqip, "Weapon_Equip");
    }

    void AddEquipSlot(GameObject slotObj, string tag)
    {
        items.Add(new ShopItem { id = -1 });
        slots.Add(slotObj);
        int index = slots.Count - 1;
        slotObj.GetComponent<Slot>().id = index;
        slotObj.tag = tag;
    }

    public void OnBuyButtonClicked(ShopItem item, GameObject itemObject)
    {
        if (item != null && !InventoryManager.InventoryInstance.ItemFull)
        {
            if (CanAffordItem(item.price))
            {
                SoundManager.Instance.PlayBuySound();
                GameManager.instance.playerMoney -= item.price;
                item.MarkAsBought();

                Destroy(itemObject);

                UpdatePlayerMoneyUI();
                InventoryManager.InventoryInstance.AddItemToInventory(item);

                StartCoroutine(ShowPurchaseMessage());
            }
            else
            {
                Debug.Log("돈이 부족합니다!");
            }
        }
        else if (item == null)
        {
            Debug.LogError("item is null!");
        }
        else
        {
            Debug.LogWarning("인벤토리 자리가 부족해서 아이템 구매를 못합니다.");
            if (ItemFullPopUp != null && popupParent != null)
            {
                GameObject popupInstance = Instantiate(ItemFullPopUp, popupParent);
                Text text = popupInstance.GetComponentInChildren<Text>();
                if (text != null) text.text = "인벤토리가 가득 찼습니다!";
                StartCoroutine(DestroyPopupAfterDelay(popupInstance, 0.5f));
            }
        }
    }

    private IEnumerator DestroyPopupAfterDelay(GameObject popup, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (popup != null)
        {
            Destroy(popup);
        }
    }

    void RefreshShop()
    {
        if (GameManager.instance.playerMoney >= refreshCost)
        {
            SoundManager.Instance.PlayRefreshSound();
            GameManager.instance.playerMoney -= refreshCost;
            UpdatePlayerMoneyUI();
            refreshCost *= 10;
            UpdateReloadButtonText();
            GenerateShopItems();
        }
        else
        {
            Debug.Log("돈이 부족하여 새로고침할 수 없습니다!");
        }
    }

    void UpdateReloadButtonText()
    {
        if (reloadButtonText != null)
        {
            reloadButtonText.text = $"Reload ({refreshCost})";
        }
    }

    IEnumerator ShowPurchaseMessage()
    {
        if (purchaseMessagePanel != null)
        {
            purchaseMessagePanel.transform.SetAsLastSibling();
            purchaseMessagePanel.SetActive(true);
            yield return new WaitForSeconds(1f);
            purchaseMessagePanel.SetActive(false);
        }
    }

    public void nextscene()
    {
        if (isTest)
        {
            SceneManager.LoadScene("BossScene");
            return;
        }

        GameManager.instance.StageLevel++;
        int stage = GameManager.instance.StageLevel;

        if (stage == 5 || stage == 6 || stage == 9)
        {
            SceneManager.LoadScene("BossScene");
        }
        else
        {
            SceneManager.LoadScene("Choice");
        }
    }

    public void Titlescene()
    {
        GameManager.isReturningFromGame = true;
        SceneManager.LoadScene("Title");
    }

    public int GetEquipSlotIndex(EquipType type, int accessoryOrder = 0)
    {
        int baseIndex = slotCount;
        switch (type)
        {
            case EquipType.Head: return baseIndex;
            case EquipType.Top: return baseIndex + 1;
            case EquipType.Gloves: return baseIndex + 2;
            case EquipType.Weapon: return baseIndex + 3;
            case EquipType.Bottom: return baseIndex + 4;
            case EquipType.Shoes: return baseIndex + 5;
            case EquipType.Accessory: return baseIndex + 6 + accessoryOrder;
            case EquipType.Special: return baseIndex + 9;
            default: return -1;
        }
    }

    public void SaveProgress()
    {
        int emptySlot = FindFirstEmptySlot();
        if (emptySlot != -1)
        {
            GameManager.instance.SaveGame(emptySlot);
            Debug.Log($"[ShopManager] 슬롯 {emptySlot}에 게임 저장 완료!");
        }
        else
        {
            Debug.LogWarning("[ShopManager] 저장 가능한 빈 슬롯이 없습니다.");
        }

        StartCoroutine(ShowSaveMessage());
    }

    IEnumerator ShowSaveMessage()
    {
        if (savePanel != null)
        {
            savePanel.transform.SetAsLastSibling();
            savePanel.SetActive(true);
            yield return new WaitForSeconds(1f);
            savePanel.SetActive(false);
        }
    }



    private int FindFirstEmptySlot()
    {
        int maxSlot = GameManager.instance.saveSlotCount;
        for (int i = 1; i <= maxSlot; i++)
        {
            string path = Path.Combine(Application.persistentDataPath, $"save_slot{i}.json");
            if (!File.Exists(path))
                return i;
        }
        return -1;
    }
}
