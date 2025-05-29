using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement; // ✅ TextMeshPro 사용

public class InGameResultController : MonoBehaviour
{
    public GameObject resultUI;
    public GameObject button;

    public RectTransform[] playerCrownResult;
    public RectTransform[] enemyCrownResult;

    [SerializeField] private GameObject PlayerCushionPrefab;
    [SerializeField] private GameObject EnemyCushionPrefab;

    [SerializeField] private GameObject crownFlyPlayerPrefab;
    [SerializeField] private GameObject crownFlyEnemyPrefab;

    [SerializeField] private TextMeshProUGUI resultText; // ✅ 결과 표시 텍스트



    
    private bool hasPlayed = false;
    private bool delayPassed = false;
    private float delayTimer = 0f;

    private float spawnInterval = 0.2f;
    private float spawnTimer = 0f;
    private int spawnIndex = 0;
    private float size;

    private int myCrowns;
    private int enemyCrowns;
    private bool spawningDone = false;
    private bool resultShown = false; // ✅ 결과 출력 여부

    [SerializeField] private bool isHost;
    
    private void Start()
    {
        isHost = (UserManager.Instance.FusionPlayerRef.RawEncoded -1  == 1);
        button.transform.localScale = Vector3.zero;
        if (resultText != null)
            resultText.text = ""; // ✅ 텍스트 초기화
    }

    private void Update()
    {

        if (GameManager.Instance.ended.Equals(false)) return;

        if (!hasPlayed)
        {
            hasPlayed = true;
            resultUI.SetActive(true);

            myCrowns = GameManager.Instance.MyCrowns;
            enemyCrowns = GameManager.Instance.EnemyCrowns;
            size = 2.1f;

            for (int i = 0; i < 3; i++)
            {
                if (isHost)
                {
                    SpawnCrownWithEffect(PlayerCushionPrefab,
                        playerCrownResult[i].position + new Vector3(0f, -40f, 0f));
                    SpawnCrownWithEffect(EnemyCushionPrefab, enemyCrownResult[i].position + new Vector3(0f, -40f, 0f));
                }
                else
                {
                    SpawnCrownWithEffect(EnemyCushionPrefab,
                        playerCrownResult[i].position + new Vector3(0f, -40f, 0f));
                    SpawnCrownWithEffect(PlayerCushionPrefab, enemyCrownResult[i].position + new Vector3(0f, -40f, 0f));
                }
            }
        }

        if (!delayPassed)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= 0.5f)
            {
                delayPassed = true;
                size = 2.5f;
                button.transform.localScale = Vector3.zero;
            }
        }

        if (delayPassed && !spawningDone)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                bool spawned = false;
                if (isHost)
                {
                    if (spawnIndex < myCrowns)
                    {
                        SpawnCrownWithEffect(crownFlyPlayerPrefab, playerCrownResult[spawnIndex].position);
                        spawned = true;
                    }

                    if (spawnIndex < enemyCrowns)
                    {
                        SpawnCrownWithEffect(crownFlyEnemyPrefab, enemyCrownResult[spawnIndex].position);
                        spawned = true;
                    }
                }
                else
                {
                    if (spawnIndex < myCrowns)
                    {
                        SpawnCrownWithEffect(crownFlyPlayerPrefab, enemyCrownResult[spawnIndex].position);
                        spawned = true;
                    }

                    if (spawnIndex < enemyCrowns)
                    {
                        SpawnCrownWithEffect(crownFlyEnemyPrefab, playerCrownResult[spawnIndex].position);
                        spawned = true;
                    }
                }

                spawnIndex++;

                if (spawnIndex >= Mathf.Max(myCrowns, enemyCrowns) && spawned)
                {
                    spawningDone = true;
                    button.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                }
            }
        }
        
        if (spawningDone && !resultShown)
        {
            resultShown = true;
            if (isHost)
            {
                if (resultText != null)
                {
                    if (myCrowns > enemyCrowns)
                        resultText.text = "승 리";
                    else if (myCrowns < enemyCrowns)
                        resultText.text = "패 배";
                    else
                        resultText.text = "동 점";
                }
            }
            else
            {
                if (resultText != null)
                {
                    if (myCrowns > enemyCrowns)
                        resultText.text = "패 배";
                    else if (myCrowns < enemyCrowns)
                        resultText.text = "승 리";
                    else
                        resultText.text = "동 점";
                }
            }

            resultText.gameObject.transform.DOScale(1f, 0.4f);
        }
        
    }

    private void SpawnCrownWithEffect(GameObject prefab, Vector3 spawnPosition)
    {
        GameObject crown = Instantiate(prefab, spawnPosition, Quaternion.identity, resultUI.transform);
        crown.transform.localScale = Vector3.zero;
        crown.transform.DOScaleX(2f, 0.5f).SetEase(Ease.OutBack);
        crown.transform.DOScaleY(2.5f, 0.5f).SetEase(Ease.OutBack);
    }

    public void GoToLobby()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
