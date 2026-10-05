
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour 
{
    public static InventoryManager InventoryInstance;

    public Inventory playerInventory;

    
    public GameObject PopupForSellPrefab;

    public int itemManyLimit; // ���� ���ѿ� �̰� ���� ������ shopmanager�� slotcount�� �޾ƿ���
    public int howMany; //���� �κ��丮�� ������ �����
    public bool ItemFull; //bool ������ ���� ���� �ٸ� Ŭ�������� �����ϱ� ����
    //�ֳ��ϸ� �� ������ true �ϰ�� UI�� ������� �׳� return �ϱ� ���ؼ�
    //private ShopManager shopManager;

    public Dictionary<string, ShopItem> equippedItems = new Dictionary<string, ShopItem>(); //string���� ��� ������ ���� �Ǵ��Ϸ���
    public bool hasHolsterEquipped; //Ȧ���� ���� �Ǿ�����
    public bool hasBandollerEquipped;

    public Text statSummaryText;

    public ShopItem secondWeaponSlotItem;
    // ��ȯ ui�׽�Ʈ
    public (int holy, int burn, int bleeding, int blind, int curse, int bang, int bullet, int hp) GetTotalStats()
    {
        int totalHoly = 0, totalBurn = 0, totalBleeding = 0, totalBlind = 0, totalCurse = 0, totalBang = 0, totalBullet = 0,totalHp=0;

        foreach (var item in playerInventory.currentPlayerItems)
        {
            if (item != null && item.id != -1)
            {
                totalHoly += item.holy;
                totalBurn += item.burn;
                totalBleeding += item.bleeding;
                totalBlind += item.blind;
                totalCurse += item.curse;
                totalBang += item.bang;
                totalBullet += item.bullet;
                totalHp += item.HP;

            }

        }

        return (totalHoly, totalBurn, totalBleeding, totalBlind, totalCurse, totalBang, totalBullet,totalHp);
    }
    //


    void Awake()
    {   
        if (InventoryInstance == null)
        {
            InventoryInstance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("�κ��丮 �Ŵ���");
        }

        else
        {
            Destroy(gameObject);
        }
        itemManyLimit = 32;
        howMany = 0;
        ItemFull = false;
        
    }

    void Start()
    {
        
        if (playerInventory != null)
        {
            playerInventory.playerItems.Clear(); //���� �����Ҷ� �κ��丮 �ʱ�ȭ
            Debug.Log("[InventoryManager] playerItems �ʱ�ȭ �Ϸ�");
        }
        if (InventoryInstance == null)
        {
            Debug.LogError("InventoryManager instance�� null�Դϴ�!");
        }

    }
    public List<ShopItem> GetEquippedItems() //�� �޼���� ��񸮽�Ʈ�� �ҷ��� �� �ִ�.
    {
        return playerInventory.currentPlayerItems;
    }
    /*
    public void AddItemToInventory(ShopItem item) //�κ��丮 â�� ������ �߰�
    {   
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory�� null�Դϴ�!");
            return;
        }

        Debug.Log("[InventoryManager] ����� playerInventory ������Ʈ �̸�: " + playerInventory.gameObject.name);
        //howMany++;
        RecalculateInventoryCount();
        if (howMany >= itemManyLimit)
        {
            Debug.LogWarning("�κ��丮 â�� ���� á���ϴ�."); //�̷��Ը� �ϸ� �κ��丮 �Ŵ��� �󿡸� ������Ʈ �ȵ�. UI���� �Ȱ��� �����
            ItemFull = true; 
            return;
        }
        playerInventory.playerItems.Add(item);
        item.IfEquip = 1;
        

        Debug.Log($"[InventoryManager] {item.itemName} �߰���. �� ����: {playerInventory.playerItems.Count}");
    }
   */
    public void AddItemToInventory(ShopItem item)
    {
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory�� null�Դϴ�!");
            return;
        }

        RecalculateInventoryCount();
        if (howMany >= itemManyLimit)
        {
            Debug.LogWarning("�κ��丮 â�� ���� á���ϴ�.");
            ItemFull = true;
            return;
        }

        // 1. ������ �߰�
        playerInventory.playerItems.Add(item);
        item.IfEquip = 1;
        RecalculateInventoryCount();
        Debug.Log($"[InventoryManager] {item.itemName} �߰���. �� ����: {playerInventory.playerItems.Count}");

        // 2. UI ���� ã�� �߰�
        ShopManager shop = ShopManager.instance;
        bool foundSlot = false;

        for (int i = 0; i < shop.slotCount; i++)
        {
            if (shop.items[i].id == -1 && shop.slots[i].transform.childCount == 0)
            {
                GameObject itemUI = Instantiate(shop.inventoryItemPrefab, shop.slots[i].transform);
                itemUI.name = item.itemName;

                // ������ ����
                Image icon = itemUI.transform.Find("ItemIcon")?.GetComponent<Image>();
                if (icon != null && item.itemIcon != null)
                    icon.sprite = item.itemIcon;

                // �巡�� ���� ����
                DragItem dragItem = itemUI.GetComponent<DragItem>();
                if (dragItem != null)
                {
                    dragItem.item = item;
                    dragItem.slot = i;

                    CanvasGroup cg = itemUI.GetComponent<CanvasGroup>();
                    if (cg != null) cg.blocksRaycasts = true;
                }

                // ����/�����Ϳ� ��ũ��Ʈ
                InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
                if (uiScript != null)
                    uiScript.itemData = item;

                // ������ ���
                shop.items[i] = item;
                foundSlot = true;
                Debug.Log($"[AddItemToInventory] {item.itemName} ���� {i}�� �߰���");
                break;
            }
        }

        if (!foundSlot)
        {
            Debug.LogWarning("[AddItemToInventory] �� ������ ã�� ���߽��ϴ�. UI �߰� ����.");
        }
    }

    public ShopItem GetEquippedItem(string slotType)
    {
        return equippedItems.ContainsKey(slotType) ? equippedItems[slotType] : null;
    }

    public void EquipItem(ShopItem item)
    {
        Debug.Log($"[EquipItem] ��û�� ������: {item.itemName}, ID: {item.id}");

        bool isNewEquip = !playerInventory.currentPlayerItems.Contains(item);

        if (isNewEquip)
        {
            item.IfEquip = 2;
            playerInventory.currentPlayerItems.Add(item);
            playerInventory.playerItems.Remove(item);
            RecalculateInventoryCount();
            Debug.Log($"������: {item.itemName}");
        }

        // 
        ShopManager shop = ShopManager.instance;

        if (item.id == 8) // Ȧ����
        {
            Debug.Log("[EquipItem] Ȧ���� ������, Weapon2 ���� Ȱ��ȭ");
            shop.Weapone2Eqip.SetActive(true);
            hasHolsterEquipped = true;
            hasBandollerEquipped = false;
            var existbandoller = playerInventory.currentPlayerItems.Find(i => i != null && i.id == 10);
            if (hasBandollerEquipped)
            {
                UnequipItem(existbandoller);
                Debug.Log("Ȧ���� �ǳ�>");
            }
            // �굹���� ���� ��ȿȭ (Ȧ���Ͱ� �켱)
            Image slotImage = shop.WeaponEqip.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = Color.white;
            }
        }

        if (item.id == 10) // �굹����
        {
            Debug.Log("[EquipItem] �굹���� ������, Weapon ���� ���� �ʷ�");
            hasBandollerEquipped = true;
            hasHolsterEquipped = false;
            var existingHolster = playerInventory.currentPlayerItems.Find(i => i != null && i.id == 8);
            if (hasHolsterEquipped)
            {
                UnequipItem(existingHolster);
                Debug.Log("�굹 �ǳ�?");
            }
            Image slotImage = shop.WeaponEqip.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = Color.green;
            }
        }

        // 
        if (item.id != 8 && hasHolsterEquipped)
        {
            Debug.Log("[EquipItem] Ȧ���� ��ü�� �� Weapon2 ���� ��Ȱ��ȭ");
            shop.Weapone2Eqip.SetActive(false);
            hasHolsterEquipped = false;
        }

        if (item.id != 10 && hasBandollerEquipped)
        {
            Debug.Log("[EquipItem] �굹���� ��ü�� �� Weapon ���� ���� �ʱ�ȭ");
            Image slotImage = shop.WeaponEqip.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = Color.white;
            }
            hasBandollerEquipped = false;
        }

        FindObjectOfType<StatUIHandler>()?.UpdateStatUI();
    }
    /*
    public void UnequipItem(ShopItem item)
    {
        if (playerInventory.currentPlayerItems.Contains(item))
        {
            
            item.IfEquip = 1;
            playerInventory.currentPlayerItems.Remove(item);
            playerInventory.playerItems.Add(item);
            Debug.Log($"������: {item.itemName}");
            //howMany++;
            RecalculateInventoryCount();
            if (item.id == 8)//Ȧ���� ���� ������
            {
                hasHolsterEquipped = false;
                Debug.Log("[Special] ���Ե��� �ٽ� ��Ȱ��ȭ�մϴ�.");
                ShopManager shop = ShopManager.instance;

                

                Transform weapon2Slot = shop.Weapone2Eqip.transform;

                if (weapon2Slot.childCount > 0)
                {
                    GameObject weaponItemObj = weapon2Slot.GetChild(0).gameObject;
                    GameObject weaponItemObj2 = weaponItemObj; //
                    DragItem weaponDragItem = weaponItemObj.GetComponent<DragItem>();
                    //DragItem weaponDragItem = weaponItemObj2.GetComponent<DragItem>();

                    if (weaponDragItem != null)
                    {
                        ShopItem weaponItem = weaponDragItem.item;

                        // ������ �̵�
                        playerInventory.currentPlayerItems.Remove(weaponItem);
                        playerInventory.playerItems.Add(weaponItem);
                        Debug.Log($"[Unequip] {weaponItem.itemName} �κ��丮�� ������. ���� �κ��丮 ��: {playerInventory.playerItems.Count}");
                        weaponItem.IfEquip = 1;
                        //howMany++;
                        RecalculateInventoryCount();
                        Debug.Log($"[Special ���� ����] Weapon2�� ������ {weaponItem.itemName} �κ��丮�� �̵�");

                        // �� ���� ã�Ƽ� UI ����
                        //  �κ��丮 UI ���� �� �� �� ã�Ƽ� ������ ��ġ
                        
                        for (int i = 0; i < shop.slotCount; i++)
                        {
                            Transform invSlot = shop.slots[i].transform;

                            if (invSlot.childCount == 0)
                            {
                                GameObject itemUI = GameObject.Instantiate(shop.inventoryItemPrefab, invSlot);
                                itemUI.name = weaponItem.itemName;

                                //  �̹��� �Ҵ�(�̹����� �ڽ��� itemicon�� �ֱ� ������)
                                Transform iconTransform = itemUI.transform.Find("ItemIcon");
                                if (iconTransform != null)
                                {
                                    Image iconImage = iconTransform.GetComponent<Image>();
                                    if (iconImage != null && weaponItem.itemIcon != null)
                                    {
                                        iconImage.sprite = weaponItem.itemIcon;
                                        iconImage.enabled = true;
                                    }
                                }

                                //  �巡�� ���� �Ҵ�
                                DragItem newDrag = itemUI.GetComponent<DragItem>();
                                if (newDrag != null)
                                {
                                    newDrag.item = weaponItem;
                                    newDrag.slot = i;

                                    //  �� ����� �巡�׵�
                                    CanvasGroup cg = itemUI.GetComponent<CanvasGroup>();
                                    if (cg != null)
                                        cg.blocksRaycasts = true;
                                }

                                //  ���� ���� ����
                                InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
                                if (uiScript != null)
                                    uiScript.itemData = weaponItem;

                                //  ShopManager.items�� ����ȭ (�߿�!!)
                                shop.items[i] = weaponItem;
                                
                                break;
                            }
                        }


                        // ���� ���â�� UI ����
                        //weaponItemObj.SetActive(false);
                        Destroy(weaponItemObj);
                        shop.items[42] = new ShopItem { id = -1 };
                        
                    }
                }
                
                shop.Weapone2Eqip.SetActive(false);
            }
            if(item.id == 10)
            {
                hasBandollerEquipped = false;
                ShopManager shop = ShopManager.instance;
                Transform weaponSlot = shop.WeaponEqip.transform;
                Image slotImage = shop.WeaponEqip.GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.color = Color.white;
                    Debug.Log("WeaponEqip ���� ȸ������ ����");
                }
                else
                {
                    Debug.LogWarning("WeaponEqip�� Image ������Ʈ ����");
                }
                if (weaponSlot.childCount > 0)
                {
                    GameObject weaponItemObj = weaponSlot.GetChild(0).gameObject;
                    DragItem weaponDragItem = weaponItemObj.GetComponent<DragItem>();

                    if (weaponDragItem != null && weaponDragItem.item.id == 18) // �ӽ�Ŷ�̶��
                    {
                        ShopItem weaponItem = weaponDragItem.item;

                        // ������ �̵�
                        playerInventory.currentPlayerItems.Remove(weaponItem);
                        playerInventory.playerItems.Add(weaponItem);
                        weaponItem.IfEquip = 1;
                        //howMany++;
                        RecalculateInventoryCount();
                        Debug.Log($"[�굹���� ���� ����] Weapon�� ������ {weaponItem.itemName} �κ��丮�� �̵�");

                        // �� ���� ã�Ƽ� UI ����
                        for (int i = 0; i < shop.slotCount; i++)
                        {
                            Transform invSlot = shop.slots[i].transform;
                            if (invSlot.childCount == 0)
                            {
                                GameObject itemUI = GameObject.Instantiate(shop.inventoryItemPrefab, invSlot);
                                itemUI.name = weaponItem.itemName;

                                Transform iconTransform = itemUI.transform.Find("ItemIcon");
                                if (iconTransform != null)
                                {
                                    Image iconImage = iconTransform.GetComponent<Image>();
                                    if (iconImage != null && weaponItem.itemIcon != null)
                                    {
                                        iconImage.sprite = weaponItem.itemIcon;
                                        iconImage.enabled = true;
                                    }
                                }

                                DragItem newDrag = itemUI.GetComponent<DragItem>();
                                if (newDrag != null)
                                {
                                    newDrag.item = weaponItem;
                                    newDrag.slot = i;
                                    CanvasGroup cg = itemUI.GetComponent<CanvasGroup>();
                                    if (cg != null)
                                        cg.blocksRaycasts = true;
                                }

                                InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
                                if (uiScript != null)
                                    uiScript.itemData = weaponItem;

                                shop.items[i] = weaponItem;
                                break;
                            }
                        }

                        Destroy(weaponItemObj); // ���� ���â UI ����
                        shop.items[shop.GetEquipSlotIndex(EquipType.Weapon)] = new ShopItem { id = -1 };
                    }
                }
            }

        }
        FindObjectOfType<StatUIHandler>()?.UpdateStatUI();
    }
    */
    public void UnequipItem(ShopItem item)
    {
        if (playerInventory.currentPlayerItems.Contains(item))
        {
            item.IfEquip = 1;
            playerInventory.currentPlayerItems.Remove(item);
            playerInventory.playerItems.Add(item);
            Debug.Log($"������: {item.itemName}");
            RecalculateInventoryCount();

            ShopManager shop = ShopManager.instance;

            // ��� Ȧ������ ��: Weapon2Eqip ó��
            if (item.id == 8)
            {
                hasHolsterEquipped = false;
                Debug.Log("[Special] ���Ե��� �ٽ� ��Ȱ��ȭ�մϴ�.");
                Transform weapon2Slot = shop.Weapone2Eqip.transform;

                if (weapon2Slot.childCount > 0)
                {
                    GameObject weaponItemObj = weapon2Slot.GetChild(0).gameObject;
                    DragItem weaponDragItem = weaponItemObj.GetComponent<DragItem>();

                    if (weaponDragItem != null)
                    {
                        ShopItem weaponItem = weaponDragItem.item;
                        playerInventory.currentPlayerItems.Remove(weaponItem);
                        playerInventory.playerItems.Add(weaponItem);
                        weaponItem.IfEquip = 1;
                        RecalculateInventoryCount();
                        Debug.Log($"[Special ���� ����] Weapon2�� ������ {weaponItem.itemName} �κ��丮�� �̵�");

                        bool foundSlot = false;
                        for (int i = 0; i < shop.slotCount; i++)
                        {
                            if (shop.items[i].id == -1 && shop.slots[i].transform.childCount == 0)
                            {
                                GameObject itemUI = Instantiate(shop.inventoryItemPrefab, shop.slots[i].transform);
                                itemUI.name = weaponItem.itemName;

                                Image iconImage = itemUI.transform.Find("ItemIcon")?.GetComponent<Image>();
                                if (iconImage != null && weaponItem.itemIcon != null)
                                {
                                    iconImage.sprite = weaponItem.itemIcon;
                                    iconImage.enabled = true;
                                }

                                DragItem newDrag = itemUI.GetComponent<DragItem>();
                                if (newDrag != null)
                                {
                                    newDrag.item = weaponItem;
                                    newDrag.slot = i;
                                    itemUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
                                }

                                InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
                                if (uiScript != null) uiScript.itemData = weaponItem;

                                shop.items[i] = weaponItem;
                                foundSlot = true;
                                break;
                            }
                        }

                        if (!foundSlot)
                        {
                            Debug.LogWarning("[UnequipItem] �κ��丮�� �� ������ ���� ���� ������ ���⸦ �߰��� �� �����ϴ�.");
                        }

                        Destroy(weaponItemObj);
                        int specialSlot = shop.GetEquipSlotIndex(EquipType.Special);
                        shop.items[specialSlot] = new ShopItem { id = -1 };
                    }
                }

                shop.Weapone2Eqip.SetActive(false);
            }

            // ��� �굹������ ��: WeaponEqip ó��
            if (item.id == 10)
            {
                hasBandollerEquipped = false;

                Transform weaponSlot = shop.WeaponEqip.transform;
                Image slotImage = shop.WeaponEqip.GetComponent<Image>();
                if (slotImage != null) slotImage.color = Color.white;

                if (weaponSlot.childCount > 0)
                {
                    GameObject weaponItemObj = weaponSlot.GetChild(0).gameObject;
                    DragItem weaponDragItem = weaponItemObj.GetComponent<DragItem>();

                    if (weaponDragItem != null && weaponDragItem.item.id == 18)
                    {
                        ShopItem weaponItem = weaponDragItem.item;
                        playerInventory.currentPlayerItems.Remove(weaponItem);
                        playerInventory.playerItems.Add(weaponItem);
                        weaponItem.IfEquip = 1;
                        RecalculateInventoryCount();

                        bool foundSlot = false;
                        for (int i = 0; i < shop.slotCount; i++)
                        {
                            if (shop.items[i].id == -1 && shop.slots[i].transform.childCount == 0)
                            {
                                GameObject itemUI = Instantiate(shop.inventoryItemPrefab, shop.slots[i].transform);
                                itemUI.name = weaponItem.itemName;

                                Image iconImage = itemUI.transform.Find("ItemIcon")?.GetComponent<Image>();
                                if (iconImage != null && weaponItem.itemIcon != null)
                                {
                                    iconImage.sprite = weaponItem.itemIcon;
                                    iconImage.enabled = true;
                                }

                                DragItem newDrag = itemUI.GetComponent<DragItem>();
                                if (newDrag != null)
                                {
                                    newDrag.item = weaponItem;
                                    newDrag.slot = i;
                                    itemUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
                                }

                                InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
                                if (uiScript != null) uiScript.itemData = weaponItem;

                                shop.items[i] = weaponItem;
                                foundSlot = true;
                                break;
                            }
                        }

                        if (!foundSlot)
                        {
                            Debug.LogWarning("[UnequipItem] �κ��丮�� �� ������ ���� ���� ������ ���⸦ �߰��� �� �����ϴ�.");
                        }

                        Destroy(weaponItemObj);
                        int weaponSlotIndex = shop.GetEquipSlotIndex(EquipType.Weapon);
                        shop.items[weaponSlotIndex] = new ShopItem { id = -1 };
                    }
                }
            }

            // �����: ���Կ� �������� ���� �ִ��� �˻�
            for (int i = 0; i < shop.slotCount; i++)
            {
                int count = shop.slots[i].transform.childCount;
                if (count > 1)
                {
                    Debug.LogError($"[UI �浹] ���� {i}�� �������� {count}�� ���� �ֽ��ϴ�!");
                }
            }
        }

        FindObjectOfType<StatUIHandler>()?.UpdateStatUI();
    }

    public void RemoveItemFromInventory(int slotIndex)
    {
        GameObject popupInstance = Instantiate(PopupForSellPrefab, transform.root);
        if (slotIndex >= 0 && slotIndex < playerInventory.playerItems.Count)
        {
            playerInventory.playerItems[slotIndex] = new ShopItem(); 
            playerInventory.playerItems[slotIndex].id = -1; 
            Debug.Log($"[InventoryManager] ���� {slotIndex} ������ ���ŵ�");
        }
    }
    public void RecalculateInventoryCount()
    {
        howMany = 0;
        foreach (var item in playerInventory.playerItems)
        {
            if (item != null && item.id != -1)
            {
                howMany++;
            }
        }

        ItemFull = howMany >= itemManyLimit;
    }
    public void RequestRemoveItem(int slotIndex, GameObject targetItemObject)
    {
        DragItem dragItem = targetItemObject.GetComponent<DragItem>();
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject popupInstance = Instantiate(PopupForSellPrefab, canvas.transform);
        Debug.Log("���� Ȯ�� �˾� ������");

        ShopItem item = dragItem.item;

        // �ؽ�Ʈ ����
        Text messageText = popupInstance.transform.Find("Message").GetComponent<Text>();
        messageText.text = $"�Ǹ��Ͻðڽ��ϱ�?\n({(int)(item.price * 0.7)} Gold)";

        // ��ư ����
        Button yesBtn = popupInstance.transform.Find("YesButton").GetComponent<Button>();
        Button noBtn = popupInstance.transform.Find("NOButton").GetComponent<Button>();

        yesBtn.onClick.AddListener(() =>
        {
            if (dragItem != null)
            {
                ShopManager shop = ShopManager.instance;
                SoundManager.Instance.PlaySellSound();
                GameManager.instance.playerMoney += (int)(item.price * 0.7);

                if (item.IfEquip == 1) // �κ��丮�� �ִ� ���
                {
                    int index = playerInventory.playerItems.IndexOf(item);
                    if (index != -1)
                    {
                        playerInventory.playerItems[index] = new ShopItem { id = -1 };
                        shop.items[index] = new ShopItem { id = -1 };
                    }
                }
                else if (item.IfEquip == 2) // ���â�� �ִ� ���
                {
                    int index = playerInventory.currentPlayerItems.IndexOf(item);
                    if (index != -1)
                    {
                        playerInventory.currentPlayerItems.RemoveAt(index);
                    }

                    // ���â ���� ���Ե� �ʱ�ȭ
                    int equipSlotIndex = slotIndex >= shop.slotCount ? slotIndex : shop.GetEquipSlotIndex(item.equipType);
                    if (equipSlotIndex != -1 && equipSlotIndex < shop.items.Count)
                    {
                        shop.items[equipSlotIndex] = new ShopItem { id = -1 };
                    }
                }

                // ���� ���� ���� ���·�
                item.MarkAsSold();

                Destroy(targetItemObject);
                Destroy(popupInstance);
                RecalculateInventoryCount();
                RebuildInventoryUI();
                shop.UpdatePlayerMoneyUI();
            }
            else
            {
                Debug.LogError("DragItem ������Ʈ�� ã�� �� �����ϴ�.");
            }
        });

        noBtn.onClick.AddListener(() =>
        {
            Destroy(popupInstance);
            Debug.Log("���� ��ҵ�");
        });
    }


    public void RebuildInventoryUI()
    {
        // ���� �ʱ�ȭ
        for (int i = 0; i < ShopManager.instance.slots.Count; i++)
        {
            Transform slot = ShopManager.instance.slots[i].transform;
            if (slot.childCount > 0)
            {
                Destroy(slot.GetChild(0).gameObject);
            }
        }

        // �κ��丮 ������ ���� (playerItems)
        for (int i = 0; i < playerInventory.playerItems.Count; i++)
        {
            ShopItem item = playerInventory.playerItems[i];
            if (item != null && item.id != -1)
            {
                GameObject itemUI = GameObject.Instantiate(ShopManager.instance.inventoryItemPrefab, ShopManager.instance.slots[i].transform);
                itemUI.name = item.itemName;

                Image icon = itemUI.transform.Find("ItemIcon")?.GetComponent<Image>();
                if (icon != null && item.itemIcon != null)
                {
                    icon.sprite = item.itemIcon;
                    icon.enabled = true;
                }

                DragItem dragItem = itemUI.GetComponent<DragItem>();
                if (dragItem != null)
                {
                    dragItem.item = item;
                    dragItem.slot = i;
                    itemUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
                }

                InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
                if (uiScript != null)
                {
                    uiScript.itemData = item;
                }
            }
        }

        // ���� ������ ���� �и�
        List<ShopItem> weapons = new List<ShopItem>();
        int accessoryCount = 0;
        foreach (var item in playerInventory.currentPlayerItems)
        {
            if (item == null || item.id == -1) continue;
            if (item.equipType == EquipType.Weapon)
            {
                weapons.Add(item);
                continue;
            }

            int slotIndex = item.equipType == EquipType.Accessory
                ? ShopManager.instance.GetEquipSlotIndex(item.equipType, accessoryCount++)
                : ShopManager.instance.GetEquipSlotIndex(item.equipType);

            if (slotIndex == -1) continue;

            CreateEquipItemUI(item, slotIndex);
        }

        // ���� ���� ����
        if (weapons.Count > 0)
        {
            int weapon1Index = ShopManager.instance.GetEquipSlotIndex(EquipType.Weapon);
            CreateEquipItemUI(weapons[0], weapon1Index);

            if (weapons.Count > 1)
            {
                int weapon2Index = ShopManager.instance.slots.IndexOf(ShopManager.instance.Weapone2Eqip);
                CreateEquipItemUI(weapons[1], weapon2Index);
                InventoryManager.InventoryInstance.secondWeaponSlotItem = weapons[1];
            }
            else
            {
                InventoryManager.InventoryInstance.secondWeaponSlotItem = null;
            }
        }

        // Ȧ����/�굹���� ���� UI ����
        bool holsterEquipped = weapons.Count > 1;
        bool bandolierEquipped = playerInventory.currentPlayerItems.Exists(i => i != null && i.id == 10);

        ShopManager shop = ShopManager.instance;
        shop.Weapone2Eqip.SetActive(holsterEquipped);
        InventoryManager.InventoryInstance.hasHolsterEquipped = holsterEquipped;
        InventoryManager.InventoryInstance.hasBandollerEquipped = bandolierEquipped;

        var slotImage = shop.WeaponEqip.GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.color = bandolierEquipped ? Color.green : Color.white;
        }

        FindObjectOfType<StatUIHandler>()?.UpdateStatUI();
    }

    private void CreateEquipItemUI(ShopItem item, int slotIndex)
    {
        GameObject itemUI = GameObject.Instantiate(ShopManager.instance.inventoryItemPrefab, ShopManager.instance.slots[slotIndex].transform);
        itemUI.name = item.itemName;

        Image icon = itemUI.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (icon != null && item.itemIcon != null)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            icon.preserveAspect = true;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            if (iconRect != null)
                iconRect.sizeDelta = new Vector2(15, 15);
        }

        DragItem dragItem = itemUI.GetComponent<DragItem>();
        if (dragItem != null)
        {
            dragItem.item = item;
            dragItem.slot = slotIndex;
            itemUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        InventoryItemUI uiScript = itemUI.GetComponent<InventoryItemUI>();
        if (uiScript != null)
        {
            uiScript.itemData = item;
        }

        ShopManager.instance.items[slotIndex] = item;
    }



    /*
    playerInventory.playerItems.Add(item);
        Debug.Log($"[InventoryManager] {item.itemName} �κ��丮�� �߰���. ���� ����: {playerInventory.playerItems.Count}");*/

    // ���� UI�� �����۵� �߰��Ϸ��� ���⼭ Instantiate(itemUI) ���ֱ�
}
/*
public static InventoryManager InventoryInstance;
public ScrollRect scrollRect; // ��ũ�Ѻ�
//public GameObject itemPrefab; // ������ ������
private List<ShopItem> inventory = new List<ShopItem>();

public Transform onDragParent;
public Transform dragLayer;


void Awake()
{
    if (InventoryInstance == null)
    {
        InventoryInstance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Debug.LogWarning("�̹� �κ��丮 �Ŵ��� ����");
        Destroy(gameObject);
    }
}
void Start()
{
    // ��ũ�Ѻ� ����
    if (scrollRect == null)
    {
        scrollRect = GameObject.Find("ScrollRect").GetComponent<ScrollRect>();
        //ó�� Ÿ��Ʋ ȭ����� �������̴ϱ� ��ũ�Ѻ並 �ִ°͵� 
    }
    dragLayer = GameObject.Find("Equipment").transform;
}
public void AddItem(ShopItem item)
{
    int j = 0;
    inventory.Add(item);
    /*
    Debug.Log($"{item.itemName}��(��) �κ��丮�� �߰��߽��ϴ�.");
    Debug.Log("���� �κ��丮:");
    foreach (ShopItem i in inventory)
    {
        Debug.Log(i.itemName);
        j++;
    }
    Debug.Log(j);
    // �κ��丮 UI ������Ʈ �ڵ� �߰�
    AddItemToScrollRect(item);
}
private void AddItemToScrollRect(ShopItem item)
{
    // ��ũ�Ѻ��� Content ��ü

    GameObject contentObject = scrollRect.content.gameObject;


    // ������ �̸� �ؽ�Ʈ ����
    GameObject itemNameObject = new GameObject("ItemName");
    itemNameObject.transform.SetParent(contentObject.transform);
    Text itemNameText = itemNameObject.AddComponent<Text>();
    itemNameText.text = item.itemName;
    itemNameText.fontSize = 24;
    itemNameText.alignment = TextAnchor.MiddleCenter;

    // ������ ������ �̹��� ����
    GameObject itemIconObject = new GameObject("ItemIcon");
    itemIconObject.transform.SetParent(contentObject.transform);
    Image itemIconImage = itemIconObject.AddComponent<Image>();
    itemIconImage.sprite = item.itemIcon;
    itemIconImage.preserveAspect = true;

    //������ ����
    GameObject itemPriceObject = new GameObject("Price");
    itemPriceObject.transform.SetParent(contentObject.transform);
    Text itemPriceText = itemPriceObject.AddComponent<Text>();
    itemPriceText.text = $"Price: {item.price}";
    itemPriceText.fontSize = 24;
    itemPriceText.alignment = TextAnchor.MiddleCenter;

    // ���̾ƿ� ���� (��: ���� ���̾ƿ�)
    GameObject itemLayoutObject = new GameObject("Item");
    itemLayoutObject.transform.SetParent(contentObject.transform);
    itemLayoutObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.5f);

    VerticalLayoutGroup VerticalLayoutGroup = itemLayoutObject.AddComponent<VerticalLayoutGroup>();
    itemNameObject.transform.SetParent(itemLayoutObject.transform);
    itemIconObject.transform.SetParent(itemLayoutObject.transform);
    itemPriceObject.transform.SetParent(itemLayoutObject.transform);

    GameObject itemButtonObject = new GameObject("ItemButton");
    itemButtonObject.transform.SetParent(itemLayoutObject.transform); ;
    Button itemButton = itemButtonObject.AddComponent<Button>();
    itemButton.onClick.AddListener(() => OnItemButtonClicked(item)); // �κ��丮�� �����ۿ� ��ư ����



    Image imageComponent = itemLayoutObject.AddComponent<Image>();
    imageComponent.color = Color.white;

    DragItem dragItem = itemLayoutObject.AddComponent<DragItem>();
    dragItem.onDragParent = dragLayer; //���̾��Ű�� equipment�� slot���� �ο�

    CanvasGroup canvasGroup = itemLayoutObject.AddComponent<CanvasGroup>();

}


public void RemoveItem(ShopItem item)
{
    inventory.Remove(item);
    Debug.Log($"{item.itemName}��(��) �κ��丮���� �����߽��ϴ�.");
    // �κ��丮 UI ������Ʈ �ڵ� �߰��ؾ� ��
}

private void OnItemButtonClicked(ShopItem item)
{
    // ������ ��ư Ŭ�� �� ������ �ൿ
    Debug.Log($"{item.itemName} ��ư Ŭ��!");

}*/




