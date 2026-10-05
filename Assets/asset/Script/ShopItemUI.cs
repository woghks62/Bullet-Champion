using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItem itemData; // 아이템 정보
    private GameObject tooltipPanel;
    private Text tooltipText;
    //
    private Image tooltipImage;
    //
    private Text tooltipName;
    //
    private RectTransform tooltipRect;
    private RectTransform canvasRect;
    //
    private Text tooltipAttackAt;
    //
    public GameObject Inventory; //인벤 창

    //
    private Transform tooltipPart;
    private Dictionary<EquipType, string> equipTypeToTextName;
    //
    private bool isPointerOverTooltip = false;
    private bool isPointerOverItem = false;
    //


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //  ShopManager에서 tooltipPanel 가져오기 (오류 방지)
        if (ShopManager.instance == null)
        {
            Debug.LogError(" ShopManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        tooltipPanel = ShopManager.instance.tooltipPanel;

        if (tooltipPanel == null)
        {
            Debug.LogError(" tooltipPanel을 찾을 수 없습니다! ShopManager에서 설정되었는지 확인하세요.");
            return;
        }

        tooltipText = tooltipPanel.transform.Find("TooltipText")?.GetComponent<Text>();
        //
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        //
        if (tooltipText == null)
        {
            Debug.LogError(" TooltipText를 찾을 수 없습니다! 오브젝트 계층 구조를 확인하세요.");
            return;
        }
        //
        tooltipImage = tooltipPanel.transform.Find("TooltipImage")?.GetComponent<Image>();

        if (tooltipImage == null)
        {
            Debug.LogError(" tooltipImage 찾을 수 없습니다! 오브젝트 계층 구조를 확인하세요.");
            return;
        }
        //

        ////    아이템 이름 관련
        tooltipName = tooltipPanel.transform.Find("TooltipName")?.GetComponent<Text>();

        if (tooltipName == null)
        {
            Debug.LogError(" tooltipName 찾을 수 없습니다! 오브젝트 계층 구조를 확인하세요.");
            return;
        }
        ///

        ////    아이템 속성 공격력 관련
        tooltipAttackAt = tooltipPanel.transform.Find("TooltipAttackAt")?.GetComponent<Text>();

        if (tooltipAttackAt == null)
        {
            Debug.LogError(" tooltipAttackAt 찾을 수 없습니다! 오브젝트 계층 구조를 확인하세요.");
            return;
        }
        ///

        ///     아이템 장비 부위 관련
        tooltipPart = tooltipPanel.transform.Find("TooltipPart");
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


        //
        // Canvas의 RectTransform 찾기
        canvasRect = tooltipPanel.transform.parent.GetComponent<RectTransform>();
        //

        // 시작할 때 툴팁 숨기기
        tooltipPanel.SetActive(false);



        //
        //AddTooltipEventTriggers();
        //

    }

    // 마우스를 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        //
        isPointerOverItem = true;
        //



        if (tooltipPanel == null || tooltipText == null || tooltipImage == null || tooltipName == null) return; // 예외 방지

        tooltipPanel.SetActive(true); // 툴팁 보이기
        tooltipText.text = $"{GetItemDescription()}";
        tooltipName.text = $"{itemData.itemName}";
        tooltipAttackAt.text = $"Bleeding: {itemData.bleeding}   Curse: {itemData.curse}   Burn: {itemData.burn}\n" +
                                $"Blind: {itemData.blind}   Holy: {itemData.holy}";
        


        tooltipImage.sprite = itemData.itemIcon;
        tooltipPanel.transform.SetAsLastSibling();
        // 장비 ui색깔 변경 함수 호출
        HighlightEquipPart(itemData.equipType);
        UpdateTooltipPosition(eventData);
    }

    // 마우스를 뗐을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverItem = false;
    }

    private void UpdateTooltipPosition(PointerEventData eventData)
    {
        if (tooltipPanel == null || canvasRect == null) return;

        // 마우스 포인터 위치를 캔버스 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, null, out localPoint);

        // 툴팁 위치 설정
        tooltipRect.anchoredPosition = localPoint + new Vector2(10, -10); // 약간의 오프셋 추가

        // 툴팁이 화면 밖으로 나가지 않도록 조정
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);

        //툴팀의 4개 모서리 좌표
        float minX = corners[0].x;  //왼쪽 아래 x
        float maxX = corners[2].x;  //오른쪽 위 x
        float minY = corners[0].y;  //왼쪽 아래 y
        float maxY = corners[2].y;  //오른쪽 위 y

        //화면 크기 가져오기
        float screenWidth = Screen.width;   //화면 가로 길이
        float screenHeight = Screen.height; // 화면 세로 길이

        Vector3 position = tooltipRect.position;    //현재 툴팀의 위치 저장

        if (maxX > screenWidth)
            position.x -= maxX - screenWidth;   // maxX가 화면너비보다 크면 maxX - screenWidth만큼 왼쪽 이동 (오른쪽으로 벗어나면 왼쪽 이동)
        if (minX < 0)
            position.x -= minX;                 // 위와 같음 왼쪽으로 벗어나면 오른쪽으로 이동 

        if (minY < 0)
            position.y -= minY;                 // 아래 벗어나면 위로 이동
        if (maxY > screenHeight)
            position.y -= maxY - screenHeight;  // 위 벗어나면 아래로 이동

        tooltipRect.position = position;        // 새로운 위치 적용
    }


    // 아이템 설명 반환 (설명이 따로 없다면 기본값 설정)
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

    public void OnInventoryButtonClicked()
    {
        Inventory.SetActive(!Inventory.activeSelf);
        

    }


    // 장비 부위 ui색깔 변화
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




    // Update is called once per frame
    void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)        // 툴팁이 존재하고 보이는 상태일 때 작동
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition) &&               //RectTransformUtility.RectangleContainsScreenPoint : 마우스가 해당 영역 UI에 있는지
                !RectTransformUtility.RectangleContainsScreenPoint(tooltipRect, Input.mousePosition))                                   // 즉 첫번째 줄은 마우스 올린 상점아이템 자체, 두번째는 툴팁 UI영역
            {
                tooltipPanel.SetActive(false);
            }
        }
    }
}
