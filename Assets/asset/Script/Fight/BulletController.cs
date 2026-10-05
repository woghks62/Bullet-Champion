using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BulletController : MonoBehaviour
{
    public List<GameObject> bulletPrefabs = new List<GameObject>();
    public Transform spawnParent;
    public PlayerState player;
    public GameObject enemy;

    private List<Button> activeBulletButtons = new List<Button>();
    private const int maxBulletCount = 12;

    public void SpawnBullets(int count)
    {
        int currentCount = spawnParent.childCount;
        int canSpawn = Mathf.Clamp(maxBulletCount - currentCount, 0, count);

        for (int i = 0; i < canSpawn; i++)
        {
            SpawnSingleBullet();
        }

        Debug.Log($"[BulletController] 현재 총알 수: {currentCount}, 생성된 총알: {canSpawn}");
    }

    void SpawnSingleBullet()
    {
        if (bulletPrefabs.Count == 0 || player == null || enemy == null)
        {
            Debug.LogWarning("총알 프리팹 혹은 플레이어/적이 설정되지 않음");
            return;
        }

        int index = Random.Range(0, bulletPrefabs.Count);
        GameObject bullet = Instantiate(
            bulletPrefabs[index],
            spawnParent.position,
            Quaternion.identity,
            spawnParent
        );

        Button button = bullet.GetComponent<Button>();
        if (button != null)
        {
            activeBulletButtons.Add(button);
        }

        // 버튼 클릭 동작은 BulletButton.cs가 처리
    }

    public void SetBulletButtonsInteractable(bool interactable)
    {
        foreach (var button in activeBulletButtons)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }

    public void RegisterSelectedBullets()
    {
        Dictionary<BulletType, int> bulletCounts = new Dictionary<BulletType, int>();
        Dictionary<BulletType, GameObject> targetByType = new Dictionary<BulletType, GameObject>();

        foreach (Transform bullet in spawnParent)
        {
            BulletButton bulletButton = bullet.GetComponent<BulletButton>();
            if (bulletButton != null && bulletButton.IsSelected())
            {
                BulletType type = bulletButton.GetBulletType();

                if (!bulletCounts.ContainsKey(type))
                    bulletCounts[type] = 0;

                bulletCounts[type]++;
                targetByType[type] = bulletButton.GetTarget();

                Destroy(bullet.gameObject); // 선택된 총알 삭제
            }
        }

        foreach (var entry in bulletCounts)
        {
            BulletType type = entry.Key;
            int count = entry.Value;
            GameObject target = targetByType[type];


            //
            Debug.Log($"[등록 시도] {type} 총알 {count}개, 타겟: {target.name}");
            //

            TurnManager.Instance.SubmitGroupedPlayerAction(target, type, count);
        }
    }


    public void ClearAllBullets()
    {
        foreach (Transform child in spawnParent)
        {
            Destroy(child.gameObject);
        }

        activeBulletButtons.Clear();
    }
}
