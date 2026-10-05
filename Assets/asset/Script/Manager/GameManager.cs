using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameover = false;
    public int StageLevel = 0;
    public int EnemyState = 0;
    public int playerMoney = 1000;
    public int saveSlotCount = 3;
    public bool isTest;

    public List<ShopItem> globalItemList;

    public static bool isReturningFromGame = false;

    void Awake()
    {
        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Debug.LogWarning("이미 게임 매니저 존재");
        //    Destroy(gameObject);
        //}
        Debug.Log("GameManager Awake 실행됨");
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"{this.name} 중복 매니저 감지. 파괴됨.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 슬롯 저장
    public void SaveGame(int slot)
    {
        GameData data = new GameData
        {
            StageLevel = this.StageLevel,
            playerMoney = this.playerMoney,
            playerItems = new List<ShopItem>(InventoryManager.InventoryInstance.playerInventory.playerItems),
            equippedItems = new List<ShopItem>(InventoryManager.InventoryInstance.playerInventory.currentPlayerItems)
        };

        SaveLoadManager.Instance.Save(data, slot);
    }

    // 슬롯 불러오기
    public void LoadGame(int slot)
    {
        Debug.Log("[검사] LoadGame 함수 진입");
        Debug.Log($"[LoadGame] 슬롯 {slot} 로드 시도됨");

        GameData data = SaveLoadManager.Instance.Load(slot);
        if (data != null)
        {
            Debug.Log($"[LoadGame] 데이터 불러오기 성공 - StageLevel: {data.StageLevel}, Money: {data.playerMoney}");
            Debug.Log($"[LoadGame] 인벤토리 아이템 수: {data.playerItems?.Count ?? 0}, 장착 아이템 수: {data.equippedItems?.Count ?? 0}");

            this.StageLevel = data.StageLevel;
            this.playerMoney = data.playerMoney;

            var inventory = InventoryManager.InventoryInstance.playerInventory;
            Debug.Log($"[LoadGame] InventoryManager 접근됨: {inventory != null}");
            Debug.Log($"[LoadGame] 적용 전 인벤 주소: {inventory.GetHashCode()}");

            inventory.playerItems = new List<ShopItem>(data.playerItems);
            inventory.currentPlayerItems = new List<ShopItem>(data.equippedItems);

            Debug.Log($"[LoadGame] 데이터 적용 완료 - 인벤토리 아이템 수: {inventory.playerItems.Count}, 장착 아이템 수: {inventory.currentPlayerItems.Count}");

           // InventoryManager.InventoryInstance.RebuildInventoryUI();
            //Debug.Log("[LoadGame] RebuildInventoryUI 호출 완료");

            SoundManager.Instance?.PlayGameStartSound();
            SceneManager.LoadScene("Store");
            Debug.Log("[LoadGame] 씬 이동: Store");
        }
        else
        {
            Debug.LogWarning($"[LoadGame] 슬롯 {slot} 에 저장된 데이터가 없습니다.");
        }
    }


    // 슬롯 삭제 (필요 시)
    public void DeleteSave(int slot)
    {
        SaveLoadManager.Instance.Delete(slot);
    }
}
