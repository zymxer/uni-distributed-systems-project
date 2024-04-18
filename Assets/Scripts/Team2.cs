using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team2 : MonoBehaviour
{
    private static int teamsAmount = 0;
    [SerializeField] private int teamNumber;
    [SerializeField] private Color teamColor;
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private int tanksAmount;
    [SerializeField] private int defendersAmount;
    [SerializeField] private int attackersAmount;
    [SerializeField] private Vector3[] spawns;
    [SerializeField] private GameObject baseObject;

    private List<Tank> _tanks = new List<Tank>();


    private void Start()
    {
        teamNumber = teamsAmount++;
        baseObject.GetComponent<SpriteRenderer>().color = teamColor;
        CreateTanks();
    }

    public void CreateTanks()
    {
        for (int i = 0; i < tanksAmount; i++)
        {
            GameObject tank = Instantiate(tankPrefab, spawns[i], Quaternion.identity);
            tank.GetComponent<Tank>().SetTeam(this, i);
        }
    }

    public Color Color => teamColor;
    public int TeamNumber => teamNumber;
}
