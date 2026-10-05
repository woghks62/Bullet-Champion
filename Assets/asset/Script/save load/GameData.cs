using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int StageLevel;
    public int playerMoney;

    // 인벤토리 및 장착 아이템 저장용
    public List<ShopItem> playerItems;
    public List<ShopItem> equippedItems;
}