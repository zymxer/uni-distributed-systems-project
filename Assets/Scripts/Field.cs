using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Field : MonoBehaviour
{
    private Vector2 _size;

    private static Field _instance = null;

    public static Field Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public Vector3 GetRandomPoint()
    {
        return new Vector3(Random.Range(-_size.x/2, _size.x/2), Random.Range(-_size.y/2, _size.y/2));
    }
    void Start()
    {
        _size = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
