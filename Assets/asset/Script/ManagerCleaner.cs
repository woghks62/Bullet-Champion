using UnityEngine;
using System.Collections;

public class ManagerCleaner : MonoBehaviour
{
    public GameObject gameManagerPrefab;
    public GameObject inventoryManagerPrefab;
    //public GameObject soundManagerPrefab;
    //public GameObject bgmManagerPrefab;
    //public GameObject saveLoadManagerPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstantiateAndMark(gameManagerPrefab);
        InstantiateAndMark(inventoryManagerPrefab);
        //InstantiateAndMark(soundManagerPrefab);
        //InstantiateAndMark(bgmManagerPrefab);
        //InstantiateAndMark(saveLoadManagerPrefab);

        // "게임에서 돌아온 경우에만" 매니저 초기화
        if (GameManager.isReturningFromGame)
        {
            StartCoroutine(CleanupAndReinitialize());
            GameManager.isReturningFromGame = false; // 초기화 후 리셋
        }
    }

    IEnumerator CleanupAndReinitialize()
    {
        yield return null; // 씬 내 오브젝트들이 완전히 로드될 때까지 1프레임 대기

        // 1. 기존 DontDestroyOnLoad 매니저 제거
        DestroyIfExists("GameManager");
        DestroyIfExists("InventoryManager");
        //DestroyIfExists("SoundManager");
        //DestroyIfExists("BGMManager");
        //DestroyIfExists("Save_Load Manager");
        Debug.Log("매니저 삭제");

        yield return null;
        yield return null;

        // 2. 새 매니저 프리팹 생성 및 등록
        InstantiateAndMark(gameManagerPrefab);
        InstantiateAndMark(inventoryManagerPrefab);
        //InstantiateAndMark(soundManagerPrefab);
        //InstantiateAndMark(bgmManagerPrefab);
        //InstantiateAndMark(saveLoadManagerPrefab);
        Debug.Log("매니저 생성");

    }

    void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null && obj.transform.parent == null)
        {
            Debug.Log($"{name} 제거 요청됨");
            Destroy(obj);
        }
    }

    void InstantiateAndMark(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("프리팹이 null임");
            return;
        }

        GameObject instance = Instantiate(prefab);
        Debug.Log($"{prefab.name} 인스턴스 생성됨: {instance.GetInstanceID()}");
        instance.name = prefab.name;
        DontDestroyOnLoad(instance);
    }
}
