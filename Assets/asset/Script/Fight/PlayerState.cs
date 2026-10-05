using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public enum BulletType
{
    Normal,
    Burn,
    Bleeding,
    Curse,
    Blind,
    Holy
}

public class PlayerState : MonoBehaviour, IPointerEnterHandler
{
    public int StageLevel = 1;
    public float AttackSpeed = 1f;
    public float DefaultDamage = 1f;
    public float ItemDamage = 0f;
    public float FinalDamage;
    public float Health = 100f;
    public float MaxHP;
    public int bang = 2;
    public int bullet = 2;

    public bool IsDead { get; private set; } = false;
    public GameObject enemy;

    public BuffUIHandler buffUI;
    public Slider HPslider;

    public int bleeding = 0;
    public int curse = 0;
    public int burn = 0;
    public int blind = 0;
    public int holy = 0;

    private Animator animator;

    //효과음
    public AudioSource audioSource;
    public AudioClip gunshotClip;
    public AudioClip hurtClip;
    public AudioClip deadClip;

    [Header(" 플레이어 데미지 UI")]
    public Text PlayerdamageLogText;
    [Header(" 플레이어 힐 UI")]
    public Text HealLogText;
    // 권총 및 장총 애니메이션 변경 관련
    public enum WeaponType
    {
        Pistol = 0,
        Rifle = 1
    }

    public WeaponType testWeaponType = WeaponType.Pistol; // 인스펙터에서 바꿀 수 있음

    //권총 및 장총 애니메이션 변경 관련

    void Start()
    {
        Debug.Log("[PlayerState] Start() 호출됨");

        PlayerdamageLogText.text = "";
        HealLogText.text = "";
        MaxHP = Health;
        HPslider.maxValue = MaxHP;
        HPslider.value = MaxHP;

        FinalDamage = (StageLevel * DefaultDamage); // ItemDamage는 공격 시 갱신

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
                Debug.LogWarning("[PlayerState] Animator를 자식에서 찾았습니다.");
            else
                Debug.LogError("[PlayerState] Animator 못 찾음!!");
        }

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                Debug.Log("Animator 파라미터 이름: " + param.name);
            }

            //
            DetectEquippedWeaponType(); // 시작 시 아이템 무기 애니메이션 설정
            //
        }
        //  아이템 스탯 적용
        var stats = GetItemDebuffStats();
        bang = Mathf.Clamp(stats["bang"], 1, 12);     // 최소 1, 최대 12로 제한
        bullet = Mathf.Clamp(stats["bullet"], 1, 12); // UI에서 보여줄 최대 총알 수

        float bonusHP = stats["hp"]; //  아이템 HP 보너스
        Health += bonusHP;
        MaxHP = Health;


        //권총 및 장총 애니메이션 변경 관련
        //animator.SetInteger("weaponType", (int)testWeaponType);
    }

    void Update()
    {
        if (Health <= 0 && !IsDead)
        {
            Death();
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform))
            {
                ShowTooltip();
            }
        }
    }

    Dictionary<string, int> GetItemDebuffStats()
    {
        var result = new Dictionary<string, int>
    {
        { "burn", 0 },
        { "bleeding", 0 },
        { "curse", 0 },
        { "blind", 0 },
        { "holy", 0 },
        { "bang", 2 }, // 여기 추가
        { "bullet", 2 },
        { "hp", 0 }

    };

        var itemList = InventoryManager.InventoryInstance.GetEquippedItems();
        ItemDamage = 0;

        foreach (var item in itemList)
        {
            //ItemDamage += item.damage;
            result["burn"] += item.burn;
            result["bleeding"] += item.bleeding;
            result["curse"] += item.curse;
            result["blind"] += item.blind;
            result["holy"] += item.holy;
            result["bang"] += item.bang;  //  아이템에서 bang 값 누적
            result["bullet"] += item.bullet;
            result["hp"] += item.HP;
        }

        return result;
    }


    public void Attack(GameObject bulletObject, GameObject enemy, BulletType bulletType)
    {
        if (IsDead) return;

        //var debuffs = GetItemDebuffStats();
        FinalDamage = (StageLevel * DefaultDamage) + ItemDamage;

        //
        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            StartCoroutine(ResetAttackFlag());
        }
        //

        // 데미지 딜레이 
        StartCoroutine(DelayedDamage(enemy, bulletType));

        //float minDamage = FinalDamage * 0.67f;
        //float maxDamage = FinalDamage * 1.33f;
        //float actualDamage = Random.Range(minDamage, maxDamage);
        //Debug.Log($"공격 데미지: {actualDamage:F2} (기본: {FinalDamage})");

        //if (bulletType == BulletType.Normal)
        //{
        //    Debug.Log("일반탄 - 디버프 적용 없음");
        //}
        //else if (bulletType == BulletType.Holy)
        //{
        //    int value = 2 + debuffs["holy"];
        //    ApplyDebuff("holy", value);
        //}
        //else if (enemy != null)
        //{
        //    string key = bulletType.ToString().ToLower();
        //    int value = 2 + debuffs[key];

        //    if (enemy.TryGetComponent<Enemy>(out var enemyScript))
        //    {
        //        enemyScript.TakeDamage(actualDamage);
        //        enemyScript.ApplyDebuff(key, value);
        //    }
        //    else if (enemy.TryGetComponent<FinalBoss>(out var bossScript))
        //    {
        //        bossScript.TakeDamage(actualDamage);
        //        bossScript.ApplyDebuff(key, value);
        //    }
        //}
    }

    public void AttackGrouped(GameObject enemy, BulletType bulletType, int count)
    {
        if (IsDead) return;

        var debuffs = GetItemDebuffStats();
        FinalDamage = (StageLevel * DefaultDamage) + ItemDamage;

        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            StartCoroutine(ResetAttackFlag());
        }

        //
        StartCoroutine(DelayedGroupedDamage(enemy, bulletType, count));
        //

        //float totalDamage = 0f;
        //for (int i = 0; i < count; i++)
        //{
        //    float min = FinalDamage * 0.67f;
        //    float max = FinalDamage * 1.33f;
        //    totalDamage += Random.Range(min, max);
        //}

        //if (bulletType == BulletType.Normal)
        //{
        //    Debug.Log("일반탄 - 디버프 적용 없음");
        //}
        //else if (bulletType == BulletType.Holy)
        //{
        //    int buffAmount = count * 2 + debuffs["holy"];
        //    ApplyDebuff("holy", buffAmount);
        //}
        //else if (enemy != null)
        //{
        //    string debuffKey = bulletType.ToString().ToLower();
        //    int debuffAmount = count * (count + 1) / 2 + debuffs[debuffKey];

        //    if (enemy.TryGetComponent<Enemy>(out var enemyScript))
        //    {
        //        enemyScript.TakeDamage(totalDamage);
        //        enemyScript.ApplyDebuff(debuffKey, debuffAmount);
        //    }
        //    else if (enemy.TryGetComponent<FinalBoss>(out var bossScript))
        //    {
        //        bossScript.TakeDamage(totalDamage);
        //        bossScript.ApplyDebuff(debuffKey, debuffAmount);
        //    }
        //}
    }

    public void ApplyDebuff(string type, int amount)
    {
        switch (type)
        {
            case "burn": burn += amount; break;
            case "bleeding": bleeding += amount; break;
            case "curse": curse += amount; break;
            case "blind": blind += amount; break;
            case "holy": holy += amount; break;
        }

        if (buffUI != null)
        {
            int value = type switch
            {
                "burn" => burn,
                "bleeding" => bleeding,
                "curse" => curse,
                "blind" => blind,
                "holy" => holy,
                _ => 0
            };

            buffUI.UpdateBuff(type, value);
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        Health -= damage;
        Debug.Log($"[PlayerState] {damage} 피해를 입음. 현재 체력: {Health}");
        DamageText(damage);
        if (PlayerdamageLogText != null)
        {
            PlayerdamageLogText.text = damage.ToString("F1"); //소수점 첫째자리 까지만
            StartCoroutine(HideDamageTextAfterDelay(0.5f));
        }
        if (animator != null)
            animator.SetTrigger("isHurt");

        HPslider.value = Mathf.Clamp(Health, 0, MaxHP);

        if (Health <= 0)
        {
            Death();
        }
    }

    public void ProcessDebuffsPerTurn()
    {
        // 1. 디버프 피해 및 회복
        float debuffDamage = burn + bleeding * 2 + curse;
        float holyHeal = holy * 2f;

        if (debuffDamage > 0)
        {
            Health -= debuffDamage;
            Debug.Log($"[디버프 피해] {debuffDamage}, 현재 체력: {Health}");

            if (animator != null)
                animator.SetTrigger("isHurt");
            if (PlayerdamageLogText != null)
            {
                PlayerdamageLogText.text = debuffDamage.ToString("F1"); //소수점 첫째자리 까지만
                StartCoroutine(HideDamageTextAfterDelay(0.5f));
            }
        }

        if (holyHeal > 0)
        {
            Health += holyHeal;
            Debug.Log($"[holy 회복] {holyHeal}, 현재 체력: {Health}");
            HealText(holyHeal);
        }

        // 2. Holy 기본 소모
        holy = Mathf.Max(0, holy - 1);

        // 3. Burn 정화
        int originalBurn = burn;
        burn = Mathf.Max(0, burn - holy);
        int burnCleansed = originalBurn - burn;
        holy = Mathf.Max(0, holy - burnCleansed);

        // 4. Bleeding 정화
        int originalBleeding = bleeding;
        bleeding = Mathf.Max(0, bleeding - holy);
        int bleedingCleansed = originalBleeding - bleeding;
        holy = Mathf.Max(0, holy - bleedingCleansed);

        // 5. Blind 정화
        int originalBlind = blind;
        blind = Mathf.Max(0, blind - holy);
        int blindCleansed = originalBlind - blind;
        holy = Mathf.Max(0, holy - blindCleansed);

        // 6. Curse 정화
        int originalCurse = curse;
        curse = Mathf.Max(0, curse - holy);
        int curseCleansed = originalCurse - curse;
        holy = Mathf.Max(0, holy - curseCleansed);

        // 7. 체력 클램핑 및 슬라이더 반영
        HPslider.value = Mathf.Clamp(Health, 0, MaxHP);

        // 8. 버프 UI 갱신
        if (buffUI != null)
        {
            buffUI.UpdateBuff("burn", burn);
            buffUI.UpdateBuff("bleeding", bleeding);
            buffUI.UpdateBuff("curse", curse);
            buffUI.UpdateBuff("blind", blind);
            buffUI.UpdateBuff("holy", holy);
        }

        // 9. 사망 처리
        if (Health <= 0 && !IsDead)
        {
            Death();
        }
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        Text nameText = GameObject.Find("NameTxt")?.GetComponent<Text>();
        Text infoText = GameObject.Find("infoText")?.GetComponent<Text>();
        nameText.text = "플레이어";
        //infoText.text = "플레이어의 스텟정보 들어갈 장소";
        var s = InventoryManager.InventoryInstance.GetTotalStats();
        infoText.text =
            $"신성: {s.holy}     화염: {s.burn}     출혈: {s.bleeding}\n" +
            $"실명: {s.blind}     저주: {s.curse}\n" +
            $"턴당 발사 가능 발수: {s.bang}\n" +
            $"턴당 생성 총알 수: {s.bullet}";
    }

    void Death()
    {
        if (IsDead) return;

        IsDead = true;
        Debug.Log("플레이어 사망!");

        if (animator != null)
            animator.SetBool("isDead", true);

        StartCoroutine(DeathAfterAnimation());
    }

    IEnumerator ResetAttackFlag()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("isAttacking", false);
    }

    IEnumerator DeathAfterAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
        {
            yield return null;
        }

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        TurnManager.Instance.GameOver();
        Destroy(gameObject);
    }

    public void OnDeathAnimationEnd()
    {
        TurnManager.Instance.GameOver();
        Destroy(gameObject);
    }

    void ShowTooltip()
    {
        Text nameText = GameObject.Find("NameTxt")?.GetComponent<Text>();
        Text infoText = GameObject.Find("infoText")?.GetComponent<Text>();
        nameText.text = "플레이어";
        //infoText.text = "플레이어의 스텟정보 들어갈 장소";
        var s = InventoryManager.InventoryInstance.GetTotalStats();
        infoText.text =
            $"신성: {s.holy}     화염: {s.burn}     출혈: {s.bleeding}\n" +
            $"실명: {s.blind}     저주: {s.curse}\n" +
            $"턴당 발사 가능 발수: {s.bang}\n" +
            $"턴당 생성 총알 수: {s.bullet}";
    }

    //
    IEnumerator DelayedDamage(GameObject enemy, BulletType bulletType)
    {
        yield return new WaitForSeconds(1.5f); // 공격 애니메이션 타이밍

        var debuffs = GetItemDebuffStats(); // 이제 여기서 디버프 계산!

        float minDamage = FinalDamage * 0.67f;
        float maxDamage = FinalDamage * 1.33f;
        float actualDamage = Random.Range(minDamage, maxDamage);
        Debug.Log($"공격 데미지: {actualDamage:F2} (기본: {FinalDamage})");

        if (bulletType == BulletType.Normal)
        {
            Debug.Log("일반탄 - 디버프 적용 없음");
        }
        else if (bulletType == BulletType.Holy)
        {
            int value = 2 + debuffs["holy"];
            ApplyDebuff("holy", value);
        }
        else if (enemy != null)
        {
            string key = bulletType.ToString().ToLower();
            int value = 2 + debuffs[key];

            if (enemy.TryGetComponent<Enemy>(out var enemyScript))
            {
                enemyScript.TakeDamage(actualDamage);
                enemyScript.ApplyDebuff(key, value);
            }
            else if (enemy.TryGetComponent<FinalBoss>(out var bossScript))
            {
                bossScript.TakeDamage(actualDamage);
                bossScript.ApplyDebuff(key, value);
            }
        }
    }
    //

    //
    IEnumerator DelayedGroupedDamage(GameObject enemy, BulletType bulletType, int count)
    {
        yield return new WaitForSeconds(1.5f); // 애니메이션과 맞춰서 타이밍 조절

        var debuffs = GetItemDebuffStats(); // 디버프도 여기서 계산

        float totalDamage = 0f;
        for (int i = 0; i < count; i++)
        {
            float min = FinalDamage * 0.67f;
            float max = FinalDamage * 1.33f;
            totalDamage += Random.Range(min, max);
        }

        if (bulletType == BulletType.Normal)
        {
            Debug.Log("일반탄 - 디버프 적용 없음");
        }
        else if (bulletType == BulletType.Holy)
        {
            int buffAmount = count * 2 + debuffs["holy"];
            ApplyDebuff("holy", buffAmount);
        }
        else if (enemy != null)
        {
            string debuffKey = bulletType.ToString().ToLower();
            int debuffAmount = count * (count + 1) / 2 + debuffs[debuffKey];

            if (enemy.TryGetComponent<Enemy>(out var enemyScript))
            {
                enemyScript.TakeDamage(totalDamage);
                enemyScript.ApplyDebuff(debuffKey, debuffAmount);
            }
            else if (enemy.TryGetComponent<FinalBoss>(out var bossScript))
            {
                bossScript.TakeDamage(totalDamage);
                bossScript.ApplyDebuff(debuffKey, debuffAmount);
            }
        }
    }
    void DamageText(float damage)
    {
        if (PlayerdamageLogText != null)
        {
            PlayerdamageLogText.text = damage.ToString("F1"); //소수점 첫째자리 까지만
            StartCoroutine(HideDamageTextAfterDelay(0.5f));
            
        }
    }

    void HealText(float heal)
    {
        if (HealLogText != null)
        {
            HealLogText.text = heal.ToString("F1"); //소수점 첫째자리 까지만
            StartCoroutine(HideHealTextAfterDelay(0.5f));
        }
    }
    //

    // 총 발사음
    public void PlayGunshotSound()
    {
        if (audioSource != null && gunshotClip != null)
        {
            audioSource.PlayOneShot(gunshotClip);
        }
    }
    //

    // 총 맞았을때 으윽 사운드
    public void PlayHurtSound()
    {
        if (audioSource != null && hurtClip != null)
        {
            audioSource.PlayOneShot(hurtClip);
        }
    }
    //

    // 죽었을때 사운드
    public void PlayDeadSound()
    {
        if (audioSource != null && deadClip != null)
        {
            audioSource.PlayOneShot(deadClip);
        }
    }
    //

    //아이템 무기id별 애니메이션 적용
    void DetectEquippedWeaponType()
    {
        var itemList = InventoryManager.InventoryInstance.GetEquippedItems();
        testWeaponType = WeaponType.Pistol; // 기본값

        foreach (var item in itemList)
        {
            if (item.id == 6) // Pistol
            {
                testWeaponType = WeaponType.Pistol;
                break;
            }
            else if (item.id == 18) // Rifle
            {
                testWeaponType = WeaponType.Rifle;
                break;
            }
        }

        animator.SetInteger("weaponType", (int)testWeaponType);
    }
    //
    //그냥 오류테스트

    IEnumerator HideDamageTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (PlayerdamageLogText != null)
            PlayerdamageLogText.text = "";
    }
    IEnumerator HideHealTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (HealLogText != null)
            HealLogText.text = "";
    }
    //
}
