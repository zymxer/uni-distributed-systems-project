using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData
{
    private List<ObjectData> _tanks;
    private List<ObjectData> _missiles;

    public List<ObjectData> Tanks => _tanks;
    public List<ObjectData> Missiles => _missiles;

}
