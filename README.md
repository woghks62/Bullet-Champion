# BULLET CHAMPION

<img width="450" height="253" alt="image" src="https://github.com/user-attachments/assets/9d627a9a-9a4e-4b86-a7ab-b88ef2de548e" />


Unity로 개발한 2D 턴제 슈팅 전략 게임입니다.  
상점에서 장비를 구매해 캐릭터를 강화하고, 다양한 탄환을 선택하여 적과 전투하는 반복형 구조로 제작했습니다.

## 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 개발 기간 | 2025.03 ~ 2025.06 |
| 개발 인원 | 3명 |
| 장르 | 2D 턴제 슈팅 전략 게임 |
| 개발 엔진 | Unity |
| 개발 언어 | C# |
| 형상 관리 | Unity Version Control (Plastic SCM) |
| 담당 | 상점 시스템 및 UI 구현 |

## 게임 플레이

**상점 → 장비 선택 → 전투 → 보상**

1. 상점에서 무작위로 등장하는 장비를 구매합니다.
2. 구매한 장비를 인벤토리에서 장착하여 캐릭터의 능력치를 강화합니다.
3. 전투에서 탄환을 선택해 적을 공격합니다.
4. 전투 보상을 획득한 뒤 다시 상점으로 이동하여 캐릭터를 강화합니다.

<p align="center">
<img width="300" height="167" alt="image" src="https://github.com/user-attachments/assets/ec22e0e5-8e4b-449a-ac19-3570e831070b" width="40%" / >
<img width="299" height="167" alt="image" src="https://github.com/user-attachments/assets/970fc93d-213f-4920-ac0f-5aeead6b3ccc" width="40%"/>

</p>
<p align="center">
  상점 및 장비 선택
</p>


<p align="center">
  <img width="450" height="253" alt="image" src="https://github.com/user-attachments/assets/9d627a9a-9a4e-4b86-a7ab-b88ef2de548e" width="60%"/>
</p>
<p align="center">
  탄환 선택 및 전투
</p>


## 담당 역할 및 구현

### 1. 상점 시스템 구현

상점 시스템 전반을 설계하고 구현했습니다.

- 구매 가능한 아이템 중 무작위로 최대 6개의 상품을 상점에 표시
- 플레이어의 보유 골드를 확인하여 구매 가능 여부 판정
- 구매 시 골드를 차감하고 해당 아이템을 인벤토리에 자동 추가
- 구매한 아이템이 다시 등장하지 않도록 상태 관리
- 리롤을 통해 상점의 상품 목록을 다시 생성
- 리롤할 때마다 비용이 `30 → 300 → 3000 → 30000`으로 증가하도록 구현

<img width="344" height="194" alt="image" src="https://github.com/user-attachments/assets/44e83b34-de1d-42ad-a640-e7b06e831c3f" />

```csharp
List<ShopItem> availableItems =
    allItems.FindAll(item => item.IsAvailableInShop());

// 판매 가능한 아이템 순서를 랜덤하게 섞음
for (int i = 0; i < availableItems.Count; i++)
{
    ShopItem temp = availableItems[i];
    int randomIndex = Random.Range(i, availableItems.Count);

    availableItems[i] = availableItems[randomIndex];
    availableItems[randomIndex] = temp;
}

// 최대 6개의 상품을 상점에 표시
int itemCount = Mathf.Min(6, availableItems.Count);

for (int i = 0; i < itemCount; i++)
{
    ShopItem item = availableItems[i];
    currentItems.Add(item);

    GameObject newItem = Instantiate(itemPrefab, itemParent);
}
```


### 2. 구매 및 인벤토리 연동

상점에서 구매한 아이템이 인벤토리에 바로 반영되도록 구매 로직과 인벤토리 시스템을 연동했습니다.

**구매 처리 흐름**

`구매 요청 → 구매 가능 여부 확인 → 골드 차감 → 구매 상태 변경 → 인벤토리 추가 → UI 갱신`

- 인벤토리 공간이 부족하면 구매를 중단하고 안내 UI 표시
- 플레이어의 보유 골드가 아이템 가격보다 적으면 구매 중단
- 구매 완료 시 아이템을 구매 상태로 변경하여 상점에 다시 등장하지 않도록 처리
- 구매한 아이템을 인벤토리에 추가하고 UI를 갱신

```csharp
public void OnBuyButtonClicked(ShopItem item, GameObject itemObject)
{
    // 인벤토리 공간 확인
    if (InventoryManager.InventoryInstance.IsInventoryFull())
        return;

    // 보유 골드 확인
    if (!CanAffordItem(item.price))
        return;

    // 구매 처리
    GameManager.instance.playerMoney -= item.price;
    item.MarkAsBought();

    // 인벤토리 연동
    InventoryManager.InventoryInstance.AddItemToInventory(item);
    UpdatePlayerMoneyUI();

    Destroy(itemObject);
}
```
구매가 완료된 `ShopItem`을 `InventoryManager`에 전달하는 방식으로 상점 시스템과 인벤토리 시스템을 연동했습니다.



### 3. 장비 스탯 UI 구현

장착한 아이템에 따라 변화하는 캐릭터의 능력치를 확인할 수 있도록
인벤토리와 연동되는 스탯 UI를 구현했습니다.

- 장비 변경 시 현재 장착된 아이템의 합산 스탯을 받아 UI 갱신
- 신성, 화염, 출혈, 실명, 저주 등 상태 효과 수치 표시
- 추가 체력 수치 표시
- 턴당 발사 가능한 탄환 수와 생성되는 탄환 수 표시

  <img width="344" height="194" alt="image" src="https://github.com/user-attachments/assets/6fb4d505-a7ad-4533-b9a9-127e8d450321" />


```csharp
public void UpdateStatUI()
{
    if (InventoryManager.InventoryInstance == null)
        return;

    var stats = InventoryManager.InventoryInstance.GetTotalStats();

    statText.text =
        $"신성: {stats.holy}     화염: {stats.burn}     출혈: {stats.bleeding}\n" +
        $"실명: {stats.blind}     저주: {stats.curse}     체력: 200 + {stats.hp}\n" +
        $"턴당 발사 가능 발수: {stats.bang}\n" +
        $"턴당 생성 총알 수: {stats.bullet}";
}
```
스탯 계산은 `InventoryManager`에서 담당하고,
`StatUIHandler`에서는 계산된 결과를 받아 화면에 표시하도록 구성했습니다.

### 4. 기타 기여

- 게임 진행에 필요한 씬 전환 구현
- 캐릭터 및 전투 연출을 위한 간단한 애니메이션 제작 및 적용
- 전투 및 UI 상황에 맞는 효과음 적용


## 문제 해결

### 랜덤 상점에서 구매한 아이템이 다시 등장하는 문제

**문제**

상점의 상품을 무작위로 다시 생성하는 과정에서
이미 구매한 아이템이 다시 등장할 수 있었습니다.

**원인**

전체 아이템 목록을 기준으로 랜덤 상품을 생성했기 때문에
각 아이템의 구매 여부가 상품 생성 조건에 반영되지 않았습니다.

**해결**

`ShopItem`에 상점 등장 가능 상태를 두고, 구매 완료 시 `MarkAsBought()`를 호출하여 해당 아이템을 구매 상태로 변경했습니다.

상품을 다시 생성할 때는 `IsAvailableInShop()`을 통해 구매 가능한 아이템만 필터링한 뒤 랜덤으로 상품을 구성하도록 변경했습니다.

```csharp
public bool IsAvailableInShop()
{
    return count > 0;
}

public void MarkAsBought()
{
    count = 0;
}
```

**결과**

구매 완료된 아이템을 랜덤 생성 후보에서 제외하여 리롤 이후에도 동일한 아이템이 다시 등장하지 않도록 개선했습니다.


## 회고 및 개선점

프로젝트 당시에는 기능 구현과 완성을 우선하여 상점 시스템을 개발했습니다.
현재 코드를 다시 살펴보면서 기능의 동작뿐만 아니라 클래스의 책임과
데이터의 의미를 명확하게 표현하는 설계도 중요하다는 점을 확인했습니다.

### ShopManager의 책임 분리

현재 `ShopManager`는 상품 생성과 구매뿐만 아니라
상점 UI, 인벤토리 UI 생성, 저장, 씬 전환 등 여러 기능을 함께 담당하고 있습니다.

당시에는 빠르게 기능을 연결하는 데 집중했지만,
현재 다시 설계한다면 각 기능의 책임을 분리하여
상점 로직의 변경이 다른 시스템에 미치는 영향을 줄이고자 합니다.

예시:
- `ShopManager` : 상품 생성, 구매, 리롤
- `ShopUI` : 상점 UI 표시 및 갱신
- `InventoryUI` : 인벤토리 UI 표시
- 게임 진행/씬 전환 : 별도의 진행 관리 클래스

### 아이템 상태 표현 개선

구매 여부를 `count`라는 정수 값의 `0`, `1`로 관리했기 때문에
변수 이름만으로는 해당 값의 의미를 파악하기 어렵습니다.

현재 다시 구현한다면 구매 여부를 명확하게 표현할 수 있는
`bool` 또는 상태 값으로 변경하여 코드의 의도를 쉽게 파악할 수 있도록 개선하고자 합니다.

### 개발 리소스 활용에 대한 아쉬움

프로젝트 당시에는 필요한 이미지와 간단한 애니메이션을 직접 제작하고,
일부 이미지는 생성형 AI를 활용하여 제작했습니다.

직접 제작하는 과정에서 예상보다 많은 시간이 소요되었고,
그만큼 게임의 핵심 기능과 완성도를 높이는 데 사용할 수 있는 개발 시간이 줄어들었습니다.

프로젝트에는 에셋 구매 등에 활용할 수 있는 지원금이 있었지만,
당시에는 외부 에셋을 적극적으로 탐색하고 활용하는 방법까지 충분히 고려하지 못했습니다.

이 경험을 통해 모든 리소스를 직접 제작하는 것보다
프로젝트의 기간과 목표를 고려하여 직접 제작할 부분과 기존 에셋을 활용할 부분을
구분하는 것도 개발 과정에서 중요한 판단이라는 점을 배웠습니다.


## 참고 사항

이 저장소는 포트폴리오 공개를 위해 기존 프로젝트를 정리하여 업로드한 버전입니다.

- 원본 프로젝트는 팀원들과 **Unity Version Control (Plastic SCM)**을 사용하여 개발했습니다.
- GitHub 저장소는 프로젝트 종료 후 포트폴리오 공개를 위해 별도로 구성했기 때문에 실제 개발 당시의 커밋 기록은 포함되어 있지 않습니다.
- 외부에서 제공받거나 다운로드한 일부 에셋 및 사운드 파일은 재배포 문제를 고려하여 저장소에서 제외했습니다.
- 따라서 저장소를 새로 Clone할 경우 일부 이미지, 사운드 등의 참조가 누락될 수 있습니다.
