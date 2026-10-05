using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlotButton : MonoBehaviour
{
    public int slotNumber;
    public Text slotText;
    public Button loadBtn;
    public Button deleteBtn;

    void Start()
    {
        RefreshSlotDisplay();

        loadBtn.onClick.AddListener(() =>
        {
            Debug.Log($"[SlotButton] 슬롯 {slotNumber} 로드 시도");

            if (GameManager.instance != null)
            {
                GameManager.instance.LoadGame(slotNumber); //  전체 데이터 로딩 및 적용
            }
            else
            {
                Debug.LogError("[SlotButton] GameManager 인스턴스가 null입니다! GameManager가 씬에 존재하는지 확인하세요.");
            }
        });

        deleteBtn.onClick.AddListener(() =>
        {
            SaveLoadManager.Instance.Delete(slotNumber);
            RefreshSlotDisplay();
        });
    }

    public void RefreshSlotDisplay()
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_slot{slotNumber}.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            slotText.text = $"소지금: {data.playerMoney} / 스테이지: {data.StageLevel + 1}";
        }
        else
        {
            slotText.text = "빈 슬롯";
        }
    }
}
