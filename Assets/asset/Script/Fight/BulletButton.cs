using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BulletButton : MonoBehaviour, IPointerEnterHandler
{
    public BulletType bulletType;

    private bool isSelected = false;
    private PlayerState playerState;
    private GameObject targetEnemy;

    [Header("Optional Visuals")]
    public Image backgroundImage;
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.cyan;

    void Start()
    {
        playerState = GameObject.FindWithTag("Player")?.GetComponent<PlayerState>();
        targetEnemy = GameObject.FindWithTag("Enemymob");

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        UpdateVisual();
    }

    public void OnButtonClick()
    {
        if (TurnManager.Instance.CurrentPhase != TurnPhase.Preparation)
        {
            Debug.Log("선택은 준비 단계에서만 가능합니다.");
            return;
        }

        if (!isSelected && GetCurrentSelectedBulletCount() >= playerState.bang)
        {
            Debug.LogWarning("선택 제한 초과! 더 이상 선택할 수 없습니다.");
            return;
        }

        isSelected = !isSelected;
        UpdateVisual();
    }

    private int GetCurrentSelectedBulletCount()
    {
        int count = 0;
        foreach (Transform bullet in transform.parent)
        {
            BulletButton bb = bullet.GetComponent<BulletButton>();
            if (bb != null && bb.isSelected)
                count++;
        }
        return count;
    }

    private void UpdateVisual()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : defaultColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Text nameText = GameObject.Find("NameTxt")?.GetComponent<Text>();
        Text infoText = GameObject.Find("infoText")?.GetComponent<Text>();

        if (nameText == null || infoText == null)
        {
            Debug.LogWarning("NameTxt 또는 infoText를 찾을 수 없습니다.");
            return;
        }

        switch (bulletType)
        {
            case BulletType.Normal:
                nameText.text = "일반 탄환";
                infoText.text = "기본 데미지를 가하는 표준 탄환입니다.";
                break;
            case BulletType.Burn:
                nameText.text = "화염 탄환";
                infoText.text = "적에게 불 데미지를 입히고 매 턴 데미지를 입힙니다.";
                break;
            case BulletType.Bleeding:
                nameText.text = "출혈 탄환";
                infoText.text = "적에게 출혈 효과를 부여해 매 턴 데미지를 입힙니다.";
                break;
            case BulletType.Curse:
                nameText.text = "저주 탄환";
                infoText.text = "적에게 저주 효과를 부여해 매턴 데미지를 입힙니다.";
                break;
            case BulletType.Blind:
                nameText.text = "실명 탄환";
                infoText.text = "적의 명중률을 낮추는 실명 효과를 부여합니다.";
                break;
            case BulletType.Holy:
                nameText.text = "신성 탄환";
                infoText.text = "플레이어를 회복시켜주는 신성한 탄환입니다.";
                break;
        }
    }


    public bool IsSelected()
    {
        return isSelected;
    }

    public BulletType GetBulletType()
    {
        return bulletType;
    }

    public GameObject GetTarget()
    {
        return targetEnemy;
    }
}

