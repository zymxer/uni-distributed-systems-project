using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Runtime.Serialization.Formatters.Binary;

public class Team2 : MonoBehaviour
{
    [SerializeField] private bool active = true;
    public static int teamsAmount = 0;
    public static List<Team2> teams = new List<Team2>();

    [SerializeField] private float captureTime = 5f;
    [SerializeField] private int teamNumber;
    [SerializeField] private Color teamColor;
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private int tanksAmount;
    [SerializeField] private int startDefendersAmount;
    private int defendersAmount;
    [SerializeField] private Vector3[] spawns;

    private Timer captureTimer;
    [SerializeField] private Slider captureSlider;
    private int attackerTeamNumber;
    private int alliesInBase = 0;

    [SerializeField] private float baseMargin;
    private Vector2 _baseSize;

    private List<Tank> _tanks;

    private List<GameObject> _tanksObjects;
    private List<GameObject> _missilesObjects;

    private void Start()
    {
        _tanks = new List<Tank>();
        _tanksObjects = new List<GameObject>();
        _missilesObjects = new List<GameObject>();
        
        _baseSize = gameObject.transform.localScale;
        spawns = new Vector3[9];
        int index = 0;
        Vector3 start = transform.position;
        for (int i = 1; i >= -1; i--)
        {
            for (int j = -1; j <= 1; j++)
            {
                spawns[index++] = new Vector3(start.x + _baseSize.x / 2 * j, start.y + _baseSize.y / 2 * i);
            }
        }
        teamNumber = teamsAmount++;
        teams.Add(this);
        gameObject.GetComponent<SpriteRenderer>().color = teamColor;
        _baseSize = gameObject.transform.localScale;

        captureSlider.maxValue = captureTime;
        captureSlider.value = captureTime;

        captureTimer = gameObject.AddComponent<Timer>();
        captureTimer.SetTimer(captureTime);
        captureTimer.OnEnd().AddListener(OnCaptureTimerEnd);
        captureTimer.OnValueChanged().AddListener(OnCaptureTimerChange);
        if(!active)
            return;
        
        CreateTanks();
    }

    public void CreateTanks()
    {
        for (int i = 0; i < tanksAmount; i++)
        {
            GameObject tank = Instantiate(tankPrefab, spawns[i], Quaternion.identity);
            _tanksObjects.Add(tank);
            bool isDefender = i < startDefendersAmount;
            tank.GetComponent<Tank>().SetTeam(this, i, isDefender);
            _tanks.Add(tank.GetComponent<Tank>());
        }
    }

    public Vector3 GetRandomPoint(bool withMargin)
    {
        Vector3 point = new Vector3(Random.Range(-_baseSize.x / 2, _baseSize.x / 2), Random.Range(-_baseSize.y / 2, _baseSize.y / 2));
        if (withMargin)
        {
            point *= baseMargin;
        }
        point += transform.position;
        return point;
    }

    public void Activate()
    {
        active = true;
        CreateTanks();
    }

    public Color Color => teamColor;
    public int TeamNumber => teamNumber;
    public static List<Team2> Teams => teams;
    public List<GameObject> MissilesObjects => _missilesObjects;
    public List<GameObject> TanksObjects => _tanksObjects;
    public bool Active {
        get
        {
            return active;
        }
        set
        {
            active = value;
        }
    }

    private void OnCaptureTimerEnd()
    {
        UI.Instance.GameOver(attackerTeamNumber);
        Time.timeScale = 0;
    }

    private void OnCaptureTimerChange()
    {
        captureSlider.value = captureTimer.GetValue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            if (other.isTrigger)
                return;
            if (other.GetComponent<Tank>().TeamNumber == teamNumber)
            {
                alliesInBase++;
                if (captureTimer.IsActive())
                    captureTimer.Stop();
            }
            else
            {
                attackerTeamNumber = other.GetComponent<Tank>().TeamNumber;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            if (other.isTrigger)
                return;
            if (other.GetComponent<Tank>().TeamNumber == teamNumber)
                return;
            if (alliesInBase == 0)
            {
                if (!captureTimer.IsActive())
                    captureTimer.Activate();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            if (other.isTrigger)
                return;
            if (other.GetComponent<Tank>().TeamNumber == teamNumber)
            {
                alliesInBase--;
            }
            else
            {
                captureTimer.Stop();
            }
        }
    }

    public void ReportEnemySpotted(Tank enemyTank)
    {
        Debug.Log($"Team {teamNumber} spotted an enemy tank from Team {enemyTank.TeamNumber} at position {enemyTank.transform.position}");
        foreach (Tank tank in _tanks) // Notifying the whole team about the opponent
        {
            if(!tank.IsDefender && !tank.IsTriggered)
                tank.OnEnemySpotted(enemyTank);
        }
    }

    public void ReportEnemyBaseSpotted(Team2 enemyBase)
    {
        Debug.Log($"Team {teamNumber} spotted an enemy base from Team {enemyBase.TeamNumber} at position {enemyBase.transform.position}");
        foreach (Tank tank in _tanks) // Notifying the whole team about the enemy base
        {
            tank.OnEnemyBaseSpotted(enemyBase);
        }
    }
}