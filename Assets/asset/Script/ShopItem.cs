using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public EquipType equipType; // 장비 타입 지정위해
    public int IfEquip = 0; //인벤토리에 있으면 1, 장비창에 있으면 2, 디폴트는 0

    public int id;
    public string itemName;  // 아이템 이름

    public Sprite itemIcon;

  
    public int price;        // 아이템 가격
    public int HP = 0;
    public int bleeding = 0;
    public int curse = 0;
    public int burn = 0;
    public int blind = 0;
    public int holy = 0;
    public int bang = 0;        // 무기 관련.
    public int bullet = 0;





    // 구매한 아이템 상점에서 등장 안하게 하는 로직 삭제 x

    public int count = 1; // 기본값: 1이면 상점에 표시됨

    public bool IsAvailableInShop()
    {
        return count > 0;
    }

    public void MarkAsBought()
    {
        count = 0;
    }

    public void MarkAsSold()
    {
        count = 1;
    }

    // 구매한 아이템 상점에서 등장 안하게 하는 로직 삭제 x

}