using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClientData
{
    [SerializeField] private List<ObjectData> _tanks;
    [SerializeField] private List<ObjectData> _missiles;

    public List<ObjectData> Tanks => _tanks;
    public List<ObjectData> Missiles => _missiles;

}
