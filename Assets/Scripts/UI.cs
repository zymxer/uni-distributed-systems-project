using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UI : MonoBehaviour
{
    
    private static UI _instance = null;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject startPanel;
    

    public static UI Instance
    {
        get { return _instance; }
    }
    
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }
    public void Restart()
    {
        Time.timeScale = 1.0f;
        Team2.teams = new List<Team2>();
        Team2.teamsAmount = 0;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
    }
    public void GameOver(int teamNumber)
    {
        gameOverText.enabled = true;
        gameOverText.text = "Team " + teamNumber + " wins";
    }
    
}
