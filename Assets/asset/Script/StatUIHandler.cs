using UnityEngine;
using UnityEngine.UI;


public class StatUIHandler : MonoBehaviour
{
    public Text statText;  // 인스펙터에서 연결할 UI 텍스트

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStatUI()
    {
        if (InventoryManager.InventoryInstance == null)
        {
            Debug.LogWarning("InventoryManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        var s = InventoryManager.InventoryInstance.GetTotalStats();

        statText.text =
            $"신성: {s.holy}     화염: {s.burn}     출혈: {s.bleeding}\n" +
            $"실명: {s.blind}     저주: {s.curse}   체력: 200 + {s.hp}\n" +
            $"턴당 발사 가능 발수: {s.bang}\n" +
            $"턴당 생성 총알 수: {s.bullet}";
    }

}
