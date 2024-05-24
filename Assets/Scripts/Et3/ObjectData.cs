using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectData
{
    [SerializeField] private bool _active;
    [SerializeField] private Vector3 _position;
    [SerializeField] private Quaternion _rotation;

    public ObjectData()
    {
        _active = false;
        _position = new Vector3();
        _rotation = new Quaternion();
    }
    public Vector3 Position => _position;
    public Quaternion Rotation => _rotation;
    public bool Active => _active;

    public void Set(bool active, Vector3 position, Quaternion rotation)
    {
        _active = active;
        _position = position;
        _rotation = rotation;
    }
}
