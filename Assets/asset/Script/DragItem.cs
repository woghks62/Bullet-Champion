using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{   
    public ShopItem item;
    public int amount;
    public int slot;

    private Transform originalParent;
    private Vector3 originalPosition;
    private ShopManager inv;
    private Vector2 offset;

    

    void Start()
    {
        inv = GameObject.Find("ShopManager").GetComponent<ShopManager>();
    }
    public void OnBeginDrag(PointerEventData eventData) //드래그를 시작할때 발생하는 메서드, 이것만 있으면 한 프레임정도만 드래그 되고 콘솔창에 오류메세지 발생, 한프레임씩만 움직일 수 있다(끊기는거지)
    {
        eventData.pointerDrag = gameObject;
        if (item != null)
        {
            offset = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y); //offset을 사용하여 아이템을 클릭할때 튀는것을 방지한다.
            originalParent = this.transform.parent; //inv = GameObject.Find("Inventory").GetComponent<Inventory>(); 이 줄 추가후 필요가 없어졌다.
            originalPosition = this.transform.position;
            this.transform.SetParent(this.transform.parent.parent);
            this.transform.position = eventData.position - offset;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            this.transform.position = eventData.position - offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData) //드래그가 종료될때 실행되는 메서드
    {   
        

        this.transform.SetParent(inv.slots[slot].transform);
        //this.transform.SetParent(originalParent); //드래그가 종료되면 원래있던 레이아웃(하이어라키창에서 원래있던 계층)으로 돌아가란뜻
        this.transform.position = inv.slots[slot].transform.position;
        //this.transform.position = originalParent.position; // 이 문장으로 드래그가 끝나면 원래 있던곳으로 돌아가게 한다.
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    public void OnPointerClick(PointerEventData eventData)
    {   
        // 마우스 오른쪽 클릭 감지
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            
            //Debug.Log($"[DragItem] {item.itemName} 아이템 제거됨 (오른쪽 클릭)");

            // InventoryManager에서 아이템 정보 제거
            InventoryManager.InventoryInstance.RequestRemoveItem(slot, this.gameObject);

            // 오브젝트 삭제
            //Destroy(this.gameObject);
        }
    }
    public void ResetToOriginalSlot()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    public void AdjustItemSizeForEquipSlot(Transform parent)
    {
        transform.SetParent(parent);

        RectTransform rect = GetComponent<RectTransform>();
        //rect.localScale = Vector3.one; // 혹은 원하는 크기
        rect.sizeDelta = new Vector2(34, 34); // 원하는 크기 (예: 100x100)

        // 위치 초기화 (선택사항)
        //rect.anchoredPosition = Vector2.zero;
    }

}
