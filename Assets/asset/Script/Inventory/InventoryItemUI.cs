using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItem itemData;
    private GameObject tooltipPanel;
    private Text tooltipText;
    private Image tooltipImage;
    private Text tooltipName;
    private RectTransform tooltipRect;
    private RectTransform canvasRect;
    private Text tooltipAttackAt;
    private Transform tooltipPart;
    private Dictionary<EquipType, string> equipTypeToTextName;
    
    void Start()
    {
        tooltipPanel = ShopManager.instance.tooltipPanel;
        tooltipText = tooltipPanel.transform.Find("TooltipText")?.GetComponent<Text>();
        tooltipImage = tooltipPanel.transform.Find("TooltipImage")?.GetComponent<Image>();
        tooltipName = tooltipPanel.transform.Find("TooltipName")?.GetComponent<Text>();
        tooltipAttackAt = tooltipPanel.transform.Find("TooltipAttackAt")?.GetComponent<Text>();
        tooltipPart = tooltipPanel.transform.Find("TooltipPart");
        canvasRect = tooltipPanel.transform.parent.GetComponent<RectTransform>();
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();

        equipTypeToTextName = new Dictionary<EquipType, string>
        {
            { EquipType.Head, "HeadText" },
            { EquipType.Top, "TopText" },
            { EquipType.Bottom, "BottomText" },
            { EquipType.Gloves, "GlovesText" },
            { EquipType.Shoes, "ShoesText" },
            { EquipType.Weapon, "WeaponText" },
            { EquipType.Accessory, "AccessoryText" },
            { EquipType.Special, "SpecialText" }
        };

        tooltipPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData == null) return;
        tooltipPanel.SetActive(true);
        tooltipName.text = $"{itemData.itemName}";
        tooltipText.text = GetItemDescription();
        tooltipAttackAt.text = $"Bleeding: {itemData.bleeding}   Curse: {itemData.curse}   Burn: {itemData.burn}\n" +
                               $"Blind: {itemData.blind}   Holy: {itemData.holy}";
        tooltipImage.sprite = itemData.itemIcon;
        tooltipPanel.transform.SetAsLastSibling();

        HighlightEquipPart(itemData.equipType);
        UpdateTooltipPosition(eventData);
    }
    private string GetItemDescription()
    {
        switch (itemData.itemName)
        {
            case "역병의사 마스크":
                return "흑사병이 돌던 시절, 사람들을 살리려 노력하던 사람들의 흔적입니다.";
            case "역병의사 신발":
                return "흑사병이 돌던 시절, 사람들을 살리려 노력하던 사람들의 흔적입니다.";
            case "역병의사 의상":
                return "흑사병이 돌던 시절, 사람들을 살리려 노력하던 사람들의 흔적입니다.";
            case "역병의사 장갑":
                return "흑사병이 돌던 시절, 사람들을 살리려 노력하던 사람들의 흔적입니다.";
            case "역병의사 벨트":
                return "흑사병이 돌던 시절, 사람들을 살리려 노력하던 사람들의 흔적입니다.";
            case "홀스터":
                return "권총을 집어넣을수 있는 홀스터 입니다.";
            case "벤돌리어":
                return "이름 없는 병사의 탄주머니입니다.";
            case "Dear Boss":
                return "전설적인 살인마, 잭 더 리퍼가 사회에 자신을 소개하던 편지입니다.";
            case "From Hell":
                return "전설적인 살인마, 잭더 리퍼가 사회에 보내는 2번째 편지입니다.";

            //
            case "죽음":
                return "묵시록의 4기사중 죽음을 담당하던 기사가 남겼습니다.";
            case "피의 법복":
                return "누군가의 피에 절여진 법복입니다.";
            case "토끼의 모자":
                return "엘리스의 토끼가 쓰던 모자입니다.";
            case "전쟁":
                return "묵시록의 4기사중 전쟁을 담당하던 기사가 남겼습니다.";
            case "기근":
                return "묵시록의 4기사중 기근을 담당하던 기사가 남겼습니다.";
            case "정복":
                return "묵시록의 4기사중 정복을 담당하던 기사가 남겼습니다.";
            case "토끼의 정장":
                return "엘리스의 토끼가 입던 정장입니다.";
            case "셜록홈즈의 모자":
                return "전설의 탐정 셜록홈즈가 쓰던 모자입니다.";
            case "플린트락 모스킷":
                return " 주인을 잃은 머스킷입니다. 누군가의 흔적이 남아있습니다.";
            case "신성결속의 띠":
                return "수도자들이 차던 허리 띠입니다.";
            case "링어라운드로시":
                return "구전으로 전해져 내려오던, 시에서 나온 신발입니다.";
            case "캐슬링신발":
                return "체스나라에서 나온 신발입니다.";
            case "가이포크스 가면":
                return "지금은 의미가 변질되어 버린 가면입니다.";
            case "깃털 귀걸이":
                return "네버랜드의 새에서 얻은 깃털로 만든 귀걸이입니다.";
            case "런던 대화재의 잔불":
                return "런던 대화재에서 발견된 꺼지지 않는 잔불입니다.";
            case "로지의 고리":
                return "로지가 가지고 있던, 팬던트입니다.";
            case "셜록홈즈의 담배":
                return "전설의 탐정 셜록홈즈가 피던 담배입니다.";
            case "셜록홈즈의 돋보기":
                return "전설의 탐정 셜록홈즈가 쓰던 돋보기입니다.";
            case "역병의사의 허브주머니":
                return "흑사병이 돌던 시절, 사람들을 살리려 노력하던 사람들의 흔적입니다.";
            case "포켓워치":
                return "토끼의 포켓워치입니다.";
            case "풍자의 깃펜":
                return "풍자로 유명하던 기자의 깃펜입니다, 그 주인은 썩 좋지 않은 결말을 맞았습니다.";
            case "후크 선장의 브로치":
                return "피터 팬의 후크선장이 가지고 있던 브로치입니다.";
            case "셜록홈즈의 코트":
                return "전설의 탐정 셜록홈즈가 걸치던 코트입니다.";
            case "앙파상의 손아귀":
                return "체스나라에서 나온 장갑입니다.";
            case "성배의 손":
                return "성배가 형상화 된 건틀릿 입니다.";
            case "증기기관머스킷":
                return "스팀펑크 기술의 정수가 들어간 머스킷입니다, 5발을 연속으로 쏠수 있습니다.";
            case "페퍼박스 피스톨":
                return "4발을 연속으로 쏠수 있는 리볼버의 조상입니다.";
            case "문클립":
                return "리볼버를 빠르게 재장전 할수 있게 해주는 문클립입니다. 재장전되는 탄약이 증가합니다.";
            //
            default:
                return "이 아이템에 대한 설명이 없습니다.";
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }

    private void UpdateTooltipPosition(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, null, out localPoint);
        tooltipRect.anchoredPosition = localPoint + new Vector2(10, -10);
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);

        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 position = tooltipRect.position;

        if (maxX > screenWidth) position.x -= maxX - screenWidth;
        if (minX < 0) position.x -= minX;
        if (minY < 0) position.y -= minY;
        if (maxY > screenHeight) position.y -= maxY - screenHeight;

        tooltipRect.position = position;
    }

    private void HighlightEquipPart(EquipType type)
    {
        if (tooltipPart == null) return;

        foreach (var kvp in equipTypeToTextName)
        {
            Transform partTextTransform = tooltipPart.Find(kvp.Value);
            if (partTextTransform != null)
            {
                Text text = partTextTransform.GetComponent<Text>();
                if (text != null)
                {
                    text.color = (kvp.Key == type) ? Color.white : Color.gray;
                }
            }
        }
    }
}