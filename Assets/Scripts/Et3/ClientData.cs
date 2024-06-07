using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClientData
{
    [SerializeField] public Color _teamColor;
    [SerializeField] public int _teamNumber;
    [SerializeField] public List<ObjectData> _tanks;
    [SerializeField] public List<ObjectData> _missiles;

    public ClientData()
    {
        _teamColor = new Color();
        _teamNumber = 0;
        _tanks = new List<ObjectData>();
        _missiles = new List<ObjectData>();
    }
    public List<ObjectData> Tanks => _tanks;
    public List<ObjectData> Missiles => _missiles;
    public Color TeamColor => _teamColor;
    public int TeamNumber => _teamNumber;

    public void OutputData()
    {
        Debug.Log("Tanks: " + _tanks.Count);
        Debug.Log("Missiles: " + _missiles.Count);
    }
    public void Set(Color teamColor, int teamNumber, List<ObjectData> tanks, List<ObjectData> missiles)
    {
        _teamColor = teamColor;
        _teamNumber = teamNumber;
        _tanks = tanks;
        _missiles = missiles;
    }

}
