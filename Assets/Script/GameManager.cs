using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerController player;
    public GameObject start;
    public GameObject game;
    public GameObject clear;
    public GameObject over;

    public Text KeyTxt;
    public Text HealthTxt;
    public Text GasTxt;

    void Awake()
    {
        start.SetActive(true);
    }

    public void GameStart()
    {
        start.SetActive(false);
        game.SetActive(true);
    }

    private void LateUpdate()
    {
        KeyTxt.text = player.key + "/" + player.maxHasKey;
        HealthTxt.text = player.health.ToString();
        GasTxt.text = player.gasCount.ToString();
    }

    public void GameClear()
    {
        game.SetActive(false);
        clear.SetActive(true);

        // 모든 EnemyController의 움직임 멈추기
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.StopEnemy();
        }
    }

    public void GameOver()
    {
        game.SetActive(false);
        over.SetActive(true);

        // 모든 EnemyController의 움직임 멈추기
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.StopEnemy();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

}
