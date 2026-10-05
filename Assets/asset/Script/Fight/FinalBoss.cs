using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//
using System.Collections;
//

public class FinalBoss : MonoBehaviour,IPointerEnterHandler
{
    public int StageLevel = 1;
    public float Health = 100f;
    public float MaxHP;
    public float AttackSpeed = 1f;
    public float DefaultDamage = 10f;
    public float FinalDamage;
    public int Accuracy;
    public int Evasion;
    public bool IsDead { get; private set; }

    public BuffUIHandler buffUI;
    public Slider HPslider;

    public int bleeding = 0;
    public int curse = 0;
    public int burn = 0;
    public int blind = 0;
    public int holy = 0;

    private GameObject player;
    private int powerStage = 1;

    //
    private Animator animator;
    //

    //
    //효과음
    public AudioSource audioSource;
    public AudioClip gunshotClip;
    public AudioClip hurtClip;
    public AudioClip deadClip;
    //

    [Header("데미지 UI")]
    public Text damageLogText;
    [Header("자가 힐 UI")]
    public Text HealLogText;
    void Start()
    {
        damageLogText.text = "";
        HealLogText.text = "";
        float SM = 1 + ((float)StageLevel / 10);

        MaxHP = Health;
        HPslider.maxValue = MaxHP;
        HPslider.value = MaxHP;

        FinalDamage = DefaultDamage;

        player = GameObject.FindWithTag("Player");

        //
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
                Debug.LogWarning("[FinalBoss] Animator를 자식에서 찾았습니다.");
            else
                Debug.LogError("[FinalBoss] Animator 못 찾음!!");
        }

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                Debug.Log("Animator 파라미터 이름: " + param.name);
            }
        }
        //
    }

    void Update()
    {
        if (Health <= 0 && !IsDead)
        {
            Death();
        }

        //
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform))
            {
                ShowTooltip();
            }
        }
        //
    }

    public void AttackPlayer()
    {
        if (player == null || IsDead) return;

        // blind 확률로 공격 스킵
        if (blind > 0)
        {
            int chance = Random.Range(1, 101); // 1~100
            if (chance <= blind)
            {
                blind = Mathf.Max(0, blind - 1);
                Debug.Log($"[Blind] 보스가 실명 상태로 공격을 건너뜀. (남은 blind: {blind})");
                return; // 공격 및 powerStage 증가 모두 스킵
            }
        }

        //
        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            StartCoroutine(ResetAttackFlag());
        }
        // 공격 딜레이 처리 코루틴
        StartCoroutine(DelayedBossAttack());
        //

        //

        //PlayerState ps = player.GetComponent<PlayerState>();
        //if (ps == null) return;

        //int[] curseTable = { 0, 2, 4, 8, 16, 32, 64, 128 };
        //int curseAmount = curseTable[powerStage];

        //if (powerStage < 7)
        //{
        //    ps.ApplyDebuff("curse", curseAmount);
        //    Debug.Log($"[보스 공격] powerStage: {powerStage}, 데미지: {curseAmount}, 저주 {curseAmount}");
        //}
        //else
        //{
        //    ApplyDebuff("curse", curseAmount);
        //    Debug.Log($"[보스 자해] powerStage: 7, 자기 자신에게 저주 {curseAmount}, 플레이어 피해 없음");
        //}

        //powerStage = Mathf.Clamp(powerStage + 1, 1, 7);
    }


    public void TakeDamage(float damage)
    {
        Health -= damage;
        HPslider.value = Mathf.Max(0, Health);
        //Debug.Log($"[보스 피격] 데미지 {damage}, 현재 체력: {Health}");
        string log = $"[보스 피격] 데미지 {damage}, 현재 체력: {Health}";
        Debug.Log(log);
        DamageText(damage);
        //
        if (animator != null)
        {
            animator.SetTrigger("isHurt");
        }
        //

        if (Health <= 0)
        {
            Death();
        }
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

        UpdateBuffUI();
    }

    public void ProcessDebuffsPerTurn()
    {
        float debuffDamage = burn + bleeding * 2 + curse;
        float healAmount = holy * 2;

        if (debuffDamage > 0)
        {
            Health -= debuffDamage;
            Debug.Log($"[보스 디버프 피해] {debuffDamage}, 남은 체력: {Health}");
            DamageText(debuffDamage);
        }

        if (healAmount > 0)
        {
            Health += healAmount;
            Debug.Log($"[보스 holy 회복] {healAmount}, 현재 체력: {Health}");
            HealText(healAmount);
        }

        if (burn > 0) burn--;
        if (bleeding > 0) bleeding--;
        if (curse > 0) curse--;
        if (blind > 0) blind--;
        if (holy > 0) holy--;

        Health = Mathf.Clamp(Health, 0, MaxHP);
        HPslider.value = Health;

        UpdateBuffUI();

        if (Health <= 0 && !IsDead)
        {
            Death();
        }
    }

    void UpdateBuffUI()
    {
        if (buffUI != null)
        {
            buffUI.UpdateBuff("burn", burn);
            buffUI.UpdateBuff("bleeding", bleeding);
            buffUI.UpdateBuff("curse", curse);
            buffUI.UpdateBuff("blind", blind);
            buffUI.UpdateBuff("holy", holy);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Text nameText = GameObject.Find("NameTxt")?.GetComponent<Text>();
        Text infoText = GameObject.Find("infoText")?.GetComponent<Text>();
        nameText.text = "보스";
        infoText.text = "보스의 스텟정보 들어갈 장소";
    }
    void Death()
    {
        if (IsDead) return;

        IsDead = true;
        Debug.Log("보스 사망");

        //TurnManager.Instance?.OnEnemyDied(this);
        //Destroy(gameObject);

        //
        if (animator != null)
        {
            animator.SetBool("isDead", true); // 사망 애니메이션 시작
            StartCoroutine(DeathAfterAnimation()); // 애니메이션 끝날 때까지 대기 후 처리
        }
        else
        {
            TurnManager.Instance?.OnEnemyDied(this);
            Destroy(gameObject);
        }
        //
    }

    //
    IEnumerator ResetAttackFlag()
    {
        yield return new WaitForSeconds(1f);
        if (animator != null)
            animator.SetBool("isAttacking", false);
    }
    //

    //
    IEnumerator DeathAfterAnimation()
    {
        // "Dead" 상태가 시작될 때까지 대기
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
        {
            yield return null;
        }

        // 해당 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        TurnManager.Instance?.OnEnemyDied(this);
        Destroy(gameObject);
    }
    //

    //
    IEnumerator DelayedBossAttack()
    {
        yield return new WaitForSeconds(1.5f); // 보스 공격 애니메이션 타이밍과 맞추기

        PlayerState ps = player.GetComponent<PlayerState>();
        if (ps == null) yield break;

        int[] curseTable = { 0, 2, 4, 8, 16, 32, 64, 128 };
        int curseAmount = curseTable[powerStage];

        if (powerStage < 7)
        {
            ps.ApplyDebuff("curse", curseAmount);
            Debug.Log($"[보스 공격] powerStage: {powerStage}, 데미지: {curseAmount}, 저주 {curseAmount}");
        }
        else
        {
            ApplyDebuff("curse", curseAmount);
            Debug.Log($"[보스 자해] powerStage: 7, 자기 자신에게 저주 {curseAmount}, 플레이어 피해 없음");
        }

        powerStage = Mathf.Clamp(powerStage + 1, 1, 7);
    }
    //

    void ShowTooltip()
    {
        Text nameText = GameObject.Find("NameTxt")?.GetComponent<Text>();
        Text infoText = GameObject.Find("infoText")?.GetComponent<Text>();
        if (nameText != null) nameText.text = "마탄의 사수";
        if (infoText != null) infoText.text = "악마와 계약을 맺은 마지막 보스입니다.\n" +
                "악마의 저주에 의해 매턴 두배로 강한 탄을 쏘아대지만.\n " +
                "7번째 탄환은 자기 자신에게 돌아오는 계약을 맺었습니다.";
    }

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
    //
    void DamageText(float damage)
    {
        if (damageLogText != null)
        {
            damageLogText.text = damage.ToString("F1"); //소수점 첫째자리 까지만
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
    //적 데미지 텍스트 안보이게
    IEnumerator HideDamageTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (damageLogText != null)
            damageLogText.text = "";
    }

    IEnumerator HideHealTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (HealLogText != null)
            HealLogText.text = "";
    }

    //
}
