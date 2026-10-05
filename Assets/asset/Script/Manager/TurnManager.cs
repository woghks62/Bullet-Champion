using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    GameOver
}

public enum TurnPhase
{
    Preparation,
    Execution
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public TurnState CurrentTurn = TurnState.PlayerTurn;
    public TurnPhase CurrentPhase = TurnPhase.Preparation;

    public PlayerState player;
    public List<Enemy> enemies = new List<Enemy>();
    public BulletController bulletController;

    public GameObject executeButton;

    private bool isTurnInProgress = false;
    private bool playerPressedExecute = false;

    private List<ActionData> actionQueue = new List<ActionData>();

    // 게임 오버 관련
    public GameObject gameOverPopup;
    public Text moneyText;
    public Text stageText;
    //

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (CurrentTurn != TurnState.GameOver)
        {
            yield return StartCoroutine(HandleTurn());
        }
    }

    IEnumerator HandleTurn()
    {
        isTurnInProgress = true;

        Debug.Log($"[턴 시작] {CurrentTurn}");

        switch (CurrentTurn)
        {
            case TurnState.PlayerTurn:
                CurrentPhase = TurnPhase.Preparation;

                bulletController.SpawnBullets(player.bullet);
                bulletController.SetBulletButtonsInteractable(true);

                executeButton.SetActive(true);
                playerPressedExecute = false;

                yield return StartCoroutine(WaitForPlayerExecute());

                executeButton.SetActive(false);
                bulletController.SetBulletButtonsInteractable(false);

                CurrentPhase = TurnPhase.Execution;

                //  디버프 항상 처리 - 플레이어
                player.ProcessDebuffsPerTurn();

                yield return StartCoroutine(ExecuteActions());

                CurrentTurn = TurnState.EnemyTurn;
                break;

            case TurnState.EnemyTurn:
                CurrentPhase = TurnPhase.Preparation;

                yield return StartCoroutine(EnemyActions());

                CurrentPhase = TurnPhase.Execution;

                //  디버프 항상 처리 - 모든 적
                foreach (Enemy enemy in enemies)
                {
                    if (enemy != null && !enemy.IsDead)
                    {
                        enemy.ProcessDebuffsPerTurn();
                    }
                }

                //  디버프 항상 처리 - 보스
                FinalBoss boss = FindObjectOfType<FinalBoss>();
                if (boss != null && !boss.IsDead)
                {
                    boss.ProcessDebuffsPerTurn();
                }

                yield return StartCoroutine(ExecuteActions());

                CurrentTurn = TurnState.PlayerTurn;
                break;
        }

        isTurnInProgress = false;
    }

    IEnumerator WaitForPlayerExecute()
    {
        while (!playerPressedExecute)
        {
            yield return null;
        }
    }

    public void OnExecuteButtonPressed()
    {
        if (CurrentTurn == TurnState.PlayerTurn && CurrentPhase == TurnPhase.Preparation)
        {
            bulletController.RegisterSelectedBullets();
            playerPressedExecute = true;
        }
    }

    public void SubmitPlayerAction(GameObject target, BulletType type)
    {
        var action = new ActionData(player.gameObject, target, player.AttackSpeed, type);
        actionQueue.Add(action);
    }

    public void SubmitGroupedPlayerAction(GameObject target, BulletType type, int count)
    {
        var action = new GroupedActionData(player.gameObject, target, player.AttackSpeed, type, count);
        actionQueue.Add(action);
    }

    IEnumerator EnemyActions()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                Debug.Log($"[등록] Enemy: {enemy.name}");
                actionQueue.Add(new ActionData(enemy.gameObject, player.gameObject, enemy.AttackSpeed, BulletType.Curse));
            }
        }

        FinalBoss boss = FindObjectOfType<FinalBoss>();
        if (boss != null && !boss.IsDead)
        {
            if (!actionQueue.Exists(a => a.caster == boss.gameObject))
            {
                Debug.Log($"[등록] FinalBoss: {boss.name}");
                actionQueue.Add(new ActionData(boss.gameObject, player.gameObject, boss.AttackSpeed, BulletType.Curse));
            }
            else
            {
                Debug.LogWarning("[중복 차단] 보스 이미 등록됨!");
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ExecuteActions()
    {
        actionQueue.Sort((a, b) => b.attackSpeed.CompareTo(a.attackSpeed));

        foreach (var action in actionQueue)
        {
            if (action.caster == null || action.target == null)
                continue;

            if (action is GroupedActionData groupedAction)
            {
                groupedAction.Execute();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                action.Execute();
                yield return new WaitForSeconds(1f);
            }

            //  디버프 처리 제거됨 (중복 방지 및 역할 정리)
        }

        actionQueue.Clear();
        yield return new WaitForSeconds(0.3f);
    }

    public void OnEnemyDied(Enemy deadEnemy)
    {
        if (enemies.Contains(deadEnemy))
            enemies.Remove(deadEnemy);

        Debug.Log($"남은 적 수: {enemies.Count}");

        if (enemies.Count == 0)
        {
            GameOver();
        }
    }

    public void OnEnemyDied(FinalBoss boss)
    {
        Debug.Log("보스 사망 처리됨");
        ClearScene();
    }

    public void GameOver()
    {
        if (CurrentTurn == TurnState.GameOver) return;

        CurrentTurn = TurnState.GameOver;
        Debug.Log("게임 종료 - 팝업 표시");

        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(true);

            // GameManager에서 정보 가져와서 표시
            moneyText.text = $"소유 금액: {GameManager.instance.playerMoney} G";
            stageText.text = $"스테이지: {GameManager.instance.StageLevel}";
        }

        //if (!Application.CanStreamedLevelBeLoaded("Store"))
        //{
        //    Debug.LogError("Store 씬을 로드할 수 없습니다. Build Settings에 등록되었는지 확인하세요.");
        //    return;
        //}

        //SceneManager.LoadScene("Store");
    }

    //
    public void StoreScene()
    {
        if (CurrentTurn == TurnState.GameOver) return;

        CurrentTurn = TurnState.GameOver;
        Debug.Log("게임 종료 - 상점 으로 이동");

        if (!Application.CanStreamedLevelBeLoaded("Store"))
        {
            Debug.LogError("Store 씬을 로드할 수 없습니다. Build Settings에 등록되었는지 확인하세요.");
            return;
        }

        SceneManager.LoadScene("Store");
    }
    //

    public void ClearScene()
    {
        if (CurrentTurn == TurnState.GameOver) return;

        CurrentTurn = TurnState.GameOver;
        Debug.Log("게임 종료 - 상점 으로 이동");

        if (!Application.CanStreamedLevelBeLoaded("Clear"))
        {
            Debug.LogError("Clear 씬을 로드할 수 없습니다. Build Settings에 등록되었는지 확인하세요.");
            return;
        }

        SceneManager.LoadScene("Clear");
    }

    public void Titlescene()
    {
        GameManager.isReturningFromGame = true;
        SceneManager.LoadScene("Title");

    }

    // ----------------------------
    // 내부 클래스: 기본 공격 데이터
    // ----------------------------
    private class ActionData
    {
        public GameObject caster;
        public GameObject target;
        public float attackSpeed;
        public BulletType bulletType;

        public ActionData(GameObject caster, GameObject target, float speed, BulletType bullet)
        {
            this.caster = caster;
            this.target = target;
            this.attackSpeed = speed;
            this.bulletType = bullet;
        }

        public virtual void Execute()
        {
            if (caster.TryGetComponent<PlayerState>(out var player))
            {
                player.Attack(null, target, bulletType);
            }
            else if (caster.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.AttackPlayer(target, bulletType);
            }
            else if (caster.TryGetComponent<FinalBoss>(out var boss))
            {
                boss.AttackPlayer();
            }
        }
    }

    // ----------------------------
    // 내부 클래스: 누적 총알 처리용
    // ----------------------------
    private class GroupedActionData : ActionData
    {
        private int count;

        public GroupedActionData(GameObject caster, GameObject target, float speed, BulletType bullet, int count)
            : base(caster, target, speed, bullet)
        {
            this.count = count;
        }

        public override void Execute()
        {
            Debug.Log($"[GroupedActionData] Execute 호출됨: {bulletType} x{count}");

            if (caster.TryGetComponent<PlayerState>(out var player))
            {
                player.AttackGrouped(target, bulletType, count);
            }
        }
    }
}
