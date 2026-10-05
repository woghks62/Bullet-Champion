using System.Collections.Generic;
using UnityEngine;

public class BuffUIHandler : MonoBehaviour
{
    [Header("UI Parents & Prefabs")]
    public Transform buffParent;

    [System.Serializable]
    public class BuffPrefabEntry
    {
        public string type;
        public GameObject prefab;
    }

    public List<BuffPrefabEntry> buffPrefabList = new(); // 인스펙터에서 할당
    private Dictionary<string, GameObject> prefabMap = new();

    [Header("Icons (Optional)")]
    public Sprite[] buffIcons;
    public string[] buffTypes = { "bleeding", "curse", "burn", "blind", "holy" };

    private Dictionary<string, GameObject> activeBuffs = new();

    void Awake()
    {
        // 프리팹 등록
        foreach (var entry in buffPrefabList)
        {
            if (!prefabMap.ContainsKey(entry.type))
                prefabMap.Add(entry.type, entry.prefab);
        }
    }

    public void UpdateBuff(string type, int amount)
    {
        if (amount <= 0)
        {
            if (activeBuffs.ContainsKey(type))
            {
                Destroy(activeBuffs[type]);
                activeBuffs.Remove(type);
            }
        }
        else
        {
            if (!activeBuffs.ContainsKey(type))
            {
                // 타입별 프리팹 선택
                if (prefabMap.TryGetValue(type, out GameObject selectedPrefab))
                {
                    GameObject obj = Instantiate(selectedPrefab, buffParent);
                    BuffDisplay display = obj.GetComponent<BuffDisplay>();

                    int index = System.Array.IndexOf(buffTypes, type);
                    if (index >= 0 && index < buffIcons.Length)
                        display.iconImage.sprite = buffIcons[index];

                    activeBuffs[type] = obj;
                }
                else
                {
                    Debug.LogWarning($"[BuffUIHandler] {type}에 대한 프리팹이 등록되지 않았습니다.");
                }
            }

            if (activeBuffs.TryGetValue(type, out GameObject go))
            {
                BuffDisplay display = go.GetComponent<BuffDisplay>();
                display.SetBuff(amount);
            }
        }
    }
}
