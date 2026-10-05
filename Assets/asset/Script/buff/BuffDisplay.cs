using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 열거형 정의
public enum BuffType
{
    Burn,
    Bleeding,
    Curse,
    Blind,
    Holy
}

public class BuffDisplay : MonoBehaviour, IPointerEnterHandler
{
    public Image iconImage;     // 디버프 아이콘
    public Text countText;      // 디버프 수치

    [Header("이 버프 아이콘의 종류")]
    public BuffType buffType;   // 열거형으로 지정

    private int currentAmount = 0; // <스택> 표시용 내부 값

    public void SetBuff(int amount)
    {
        currentAmount = amount;
        countText.text = amount.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Text nameText = GameObject.Find("NameTxt")?.GetComponent<Text>();
        Text infoText = GameObject.Find("infoText")?.GetComponent<Text>();

        if (nameText == null || infoText == null) return;

        switch (buffType)
        {
            case BuffType.Burn:
                nameText.text = "화염";
                infoText.text = $"매턴 {currentAmount} 만큼의 데미지를 주고, 스택이 하나씩 줄어듭니다.";
                break;
            case BuffType.Bleeding:
                nameText.text = "출혈";
                infoText.text = $"매턴 {currentAmount} 만큼의 데미지를 주고, 스택이 하나씩 줄어듭니다.";
                break;
            case BuffType.Curse:
                nameText.text = "저주";
                infoText.text = $"매턴 {currentAmount} 만큼의 데미지를 주고, 스택이 하나씩 줄어듭니다.";
                break;
            case BuffType.Blind:
                nameText.text = "실명";
                infoText.text = $"{currentAmount}% 만큼 적의 명중률이 낮아집니다.";
                break;
            case BuffType.Holy:
                nameText.text = "신성";
                infoText.text = $"{currentAmount} 만큼 플레이어를 회복시키며, 다른 디버프를 최대{currentAmount} 만큼 정화하고 사라집니다." +
                    $"남아있을시, 턴당 스택이 하나씩 줄어듭니다.";
                break;
        }
    }
}
