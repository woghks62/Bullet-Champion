using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class EndText : MonoBehaviour
{
    // 게임 오버 관련
    public GameObject gameOverPopup;
    public Text moneyText;
    public Text stageText;
    //

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // GameManager에서 정보 가져와서 표시
        moneyText.text = $"소유 금액: {GameManager.instance.playerMoney} G";
        stageText.text = $"클리어한 스테이지: {GameManager.instance.StageLevel}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Titlescene()
    {
        GameManager.isReturningFromGame = true;
        SceneManager.LoadScene("Title");

    }
}
