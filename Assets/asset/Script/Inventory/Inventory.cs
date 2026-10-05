using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour //인벤 정보 저장하는 백엔드 느낌
{
    

    [Header("플레이어 아이템 목록")]
    public List<ShopItem> playerItems = new List<ShopItem>(); // 실제 보유 아이템(인벤토리)
    public List<ShopItem> currentPlayerItems = new List<ShopItem>(); //장비창에서 장착한 아이템(현재 장착중인 아이템)



    /*
    public void AddItem(ShopItem itemToAdd)
    {
        Debug.Log($"{slots.Count}Inventory클래스의 Additem 메서드 실행");
        for (int i = 0; i < slots.Count; i++)
        {
            Transform slotTransform = slots[i].transform;
            

            // 슬롯에 아이템이 없으면
            if (slotTransform.childCount == 0)
            {
                Debug.Log("실험");
                playerItems.Add(itemToAdd); // 내부 데이터에 추가

                // 아이템 UI 생성
                GameObject itemObj = Instantiate(inventoryItem);
                itemObj.transform.SetParent(slots[i].transform); // 슬롯에 추가
                itemObj.transform.position = Vector2.zero;

                // 아이콘 이미지 설정
                Image icon = itemObj.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = itemToAdd.itemIcon;
                }

                // ItemData 데이터 설정 (선택)
                /*
                ItemData itemData = itemObj.GetComponent<ItemData>();
                if (itemData != null)
                {
                    itemData.item = itemToAdd;
                    itemData.slot = i;
                }

                itemObj.name = itemToAdd.itemName;

                Debug.Log($"[인벤토리] {itemToAdd.itemName} 추가됨 (슬롯 {i})");

                break;
            }
        }
    }

    public void RemoveItem(ShopItem item)
    {
        if (playerItems.Contains(item))
        {
            playerItems.Remove(item);
            Debug.Log($"{item.itemName} 제거됨");

            // 슬롯에서도 제거하기 (옵션)
            for (int i = 0; i < slots.Count; i++)
            {
                Transform slotTransform = slots[i].transform;
                if (slotTransform.childCount > 0)
                {
                    ItemData data = slotTransform.GetChild(0).GetComponent<ItemData>();
                    if (data != null && data.item == item)
                    {
                        Destroy(slotTransform.GetChild(0).gameObject);
                        break;
                    }
                }
            }
        }
    }*/
}
