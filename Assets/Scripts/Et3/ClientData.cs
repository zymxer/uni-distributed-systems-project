using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClientData
{
    [SerializeField] private Color _teamColor;
    [SerializeField] private int _teamNumber;
    [SerializeField] private List<ObjectData> _tanks;
    [SerializeField] private List<ObjectData> _missiles;

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

    public void Set(Color teamColor, int teamNumber, List<ObjectData> tanks, List<ObjectData> missiles)
    {
        _teamColor = teamColor;
        _teamNumber = teamNumber;
        _tanks = tanks;
        _missiles = missiles;
    }

}
