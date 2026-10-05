using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    public int id;
    private ShopManager inv;
    

    void Start()
    {
        
        inv = GameObject.Find("ShopManager").GetComponent<ShopManager>();
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[OnDrop] 호출됨 - 슬롯 ID: {id}, 태그: {this.tag}");
        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning("OnDrop: pointerDrag가 null입니다.");
            return;
        }
        DragItem droppedItem = eventData.pointerDrag.GetComponent<DragItem>(); //pointerDrag**라는 필드는 현재 드래그되고 있는 게임 오브젝트를 나타냄
        if (droppedItem == null)
        {
            Debug.LogWarning("OnDrop: DragItem 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        if (this.tag.Contains("Equip"))
        {
            EquipType slotType = GetSlotEquipTypeFromTag(this.tag);
            if (droppedItem.item.equipType != slotType)
            {
                Debug.LogWarning($"[Slot] 장비 타입 불일치! {droppedItem.item.equipType} 아이템은 {slotType} 슬롯에 장착 불가.");
                droppedItem.ResetToOriginalSlot(); //  복귀 처리 (DragItem.cs에 추가해야 함)
                return;
            }
            if (droppedItem.item.id == 18)
            {
                Debug.Log("머스킷 장착됨");
                if (!InventoryManager.InventoryInstance.hasBandollerEquipped)
                {
                    Debug.Log("밴돌리어 없음");
                    droppedItem.ResetToOriginalSlot();
                    return;
                }
            }
            Image icon = droppedItem.transform.Find("ItemIcon")?.GetComponent<Image>();
            if (icon != null)
            {
                RectTransform iconRect = icon.GetComponent<RectTransform>();
                if (iconRect != null)
                    iconRect.sizeDelta = new Vector2(40, 40); // 장비 슬롯용 크기
            }
        }
        Debug.Log($"[OnDrop] inv.items[{id}].id = {inv.items[id].id}");
        if (inv.items[id].id == -1)
        {
            Debug.Log("OnDrop메서드"); //slot.cs 가 있는 오브젝트에만 이 메서드가 반응 한다..?
            /*droppedItem.transform.SetParent(this.transform);
            droppedItem.transform.position = this.transform.position;*/

            inv.items[droppedItem.slot] = new ShopItem(); //여기서 원래있던곳에 다시 -1만들어야 한다.
            inv.items[droppedItem.slot].id = -1;
            inv.items[id] = droppedItem.item;
            droppedItem.slot = id; //slot 마다 아이디가 있고 그 슬롯의 아이디를 넣게 되는 방식인거 같다.
            
            if (this.tag.Contains("Equip"))
            {
                SoundManager.Instance.PlayEquipSound();
                
                InventoryManager.InventoryInstance.EquipItem(droppedItem.item);
                //Debug.Log($"{droppedItem.item.itemName}이 장비창 아이템의 이름");
                //InventoryManager.InventoryInstance.UnequipItem(droppedItem.item);
            }
            else //else이면 슬롯 포함된것은 인벤토리 뿐
            {
                InventoryManager.InventoryInstance.UnequipItem(droppedItem.item);
            }


        }
        else
        {
            if (this.transform.childCount > 0)
            {
                
                Transform targetItemTransform = this.transform.GetChild(0);
                DragItem targetItem = targetItemTransform.GetComponent<DragItem>();

                int fromSlot = droppedItem.slot; // 드래그하던 아이템의 원래 슬롯
                int toSlot = id; // 현재 드롭된 슬롯

                bool fromIsEquip = inv.slots[fromSlot].tag.Contains("Equip");
                bool toIsEquip = this.tag.Contains("Equip");

                if (fromIsEquip || toIsEquip)
                {
                    EquipType toSlotType = GetSlotEquipTypeFromTag(this.tag);
                    EquipType fromSlotType = GetSlotEquipTypeFromTag(inv.slots[fromSlot].tag);

                    if (toIsEquip && droppedItem.item.equipType != toSlotType)
                    {
                        Debug.LogWarning($"[Slot] 교체 불가 - {droppedItem.item.equipType}는 {toSlotType} 슬롯에 장착 불가.");
                        droppedItem.ResetToOriginalSlot();
                        return;
                    }
                    if (fromIsEquip && targetItem.item.equipType != fromSlotType)
                    {
                        Debug.LogWarning($"[Slot] 교체 불가 - {targetItem.item.equipType}는 {fromSlotType} 슬롯에 장착 불가.");
                        droppedItem.ResetToOriginalSlot();
                        return;
                    }
                }
                if (toIsEquip)
                {
                    InventoryManager.InventoryInstance.UnequipItem(targetItem.item);
                }
                if (fromIsEquip)
                {
                    InventoryManager.InventoryInstance.UnequipItem(droppedItem.item);
                }
                SoundManager.Instance.PlayEquipSound();

                // 1. 슬롯 정보 바꾸기
                droppedItem.slot = toSlot;
                targetItem.slot = fromSlot;

                // 2. 슬롯 데이터 교환
                ShopItem tempItem = inv.items[toSlot];
                inv.items[toSlot] = inv.items[fromSlot];
                inv.items[fromSlot] = tempItem;

               
                // 3. 위치 재배치
                targetItemTransform.SetParent(inv.slots[fromSlot].transform);
                targetItemTransform.position = inv.slots[fromSlot].transform.position;

                droppedItem.transform.SetParent(this.transform);
                droppedItem.transform.position = this.transform.position;
            }
        }
    }
    private EquipType GetSlotEquipTypeFromTag(string tag) // 이 메서드가 장착
    {
        if (tag.Contains("Head")) return EquipType.Head;
        if (tag.Contains("Top")) return EquipType.Top;
        if (tag.Contains("Bottom")) return EquipType.Bottom;
        if (tag.Contains("Gloves")) return EquipType.Gloves;
        if (tag.Contains("Shoes")) return EquipType.Shoes;
        if (tag.Contains("Weapon")) return EquipType.Weapon;
        if (tag.Contains("Accessory")) return EquipType.Accessory;
        if (tag.Contains("Special")) return EquipType.Special;
        return EquipType.None;
    }

}
