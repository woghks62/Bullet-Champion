using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 저장
    public void Save(GameData data, int slot)
    {
        string path = GetSlotPath(slot);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[저장 완료] 슬롯 {slot} 저장됨: {path}");
    }

    // 불러오기
    public GameData Load(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"[불러오기 완료] 슬롯 {slot}");
            return data;
        }

        Debug.LogWarning($"[로드 실패] 슬롯 {slot}에 데이터 없음");
        return null;
    }

    // 삭제
    public void Delete(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[삭제 완료] 슬롯 {slot} 데이터 삭제됨");
        }
        else
        {
            Debug.LogWarning($"[삭제 실패] 슬롯 {slot}에 삭제할 파일 없음");
        }
    }

    // 슬롯 경로 생성
    private string GetSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");
    }

    // 게임 종료 및 타이틀 화면 이동
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭됨!");
        SoundManager.Instance?.PlayGameQuitSound(); // null 체크 권장
        SceneManager.LoadScene("Title");
    }
}
