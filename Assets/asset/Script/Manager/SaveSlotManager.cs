using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotManager : MonoBehaviour
{
    public GameObject slotButtonPrefab;
    public Transform slotParent;

    private int slotCount;

    void Start()
    {
        // GameManager에서 슬롯 개수 가져오기
        slotCount = GameManager.instance != null ? GameManager.instance.saveSlotCount : 3;

        for (int i = 1; i <= slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotButtonPrefab, slotParent);
            SlotButton btn = slotObj.GetComponent<SlotButton>();
            btn.slotNumber = i;
            btn.RefreshSlotDisplay();
        }
    }
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭됨!");
        SoundManager.Instance?.PlayGameQuitSound(); // null 체크 권장
        SceneManager.LoadScene("Title");
    }
}
