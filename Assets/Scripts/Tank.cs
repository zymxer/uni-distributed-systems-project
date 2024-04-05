using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;

public class Tank : MonoBehaviour
{
    //const during game
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationThreshold;
    [SerializeField] private GameObject missile;
    [SerializeField] private float shotCooldown;
    [SerializeField] private float attackRange;
    
    private Timer _shotTimer;
    private Seeker _seeker;
    [SerializeField] private float nextPointDistance = 0.1f;
    
    //variable
    private Vector3 _position;
    private Quaternion _rotation;
    
    private Path _path;
    private Vector3 _target;
    private int _currentPoint;
    private Vector3 _direction;
    private float _distance;
    private float _angle;

    private bool _canMove = true;
    private bool _triggered = false;


    public Vector3 GetPosition()
    {
        return _position;
    }

    //synchronize
    public void SetPosition(Vector2 value)
    {
        _position = value;
    }

    private void OnPathGenerationComplete(Path path)
    {
        if (!path.error)
        {
            _path = path;
            _currentPoint = 0;
        }
    }

    //Thread function
    private void Run()
    {
        UpdatePath();
        if (_path != null)
        {
            UpdateDirection();
            UpdateRotation();
            if (_canMove)
            {
                UpdatePosition();
            }

            if (_triggered)
            {
                _canMove = false;
                Shoot();
            }
        }
    }

    private void UpdateDirection()
    {
        _direction = (_path.vectorPath[_currentPoint] - _position).normalized;
        _angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
    }
    
    private void UpdatePath()
    {
        _distance = Vector2.Distance(_position, _path.vectorPath[_currentPoint]);
        if (_distance < nextPointDistance)
        {
            _currentPoint++;
        }
        
        if (ReachedEndCheck())
        {
            UpdateTarget(Field.Instance.GetRandomPoint());
        }
    }

    private void Shoot()
    {
        if (!_shotTimer.IsActive())
        {
            Instantiate(missile, _position, _rotation);
            _shotTimer.Activate();   
        }
    }

    private void UpdateRotation()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, _angle - 90f);
        _rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(_rotation, targetRotation) > rotationThreshold)
        {
            _canMove = false;
        }
        else
        {
            _canMove = true;
        }
    }

    private void UpdatePosition()
    {
        _position += _direction * (speed * Time.deltaTime);
    }

    private void UpdateTarget(Vector3 value)
    {
        _target = value;
        _seeker.StartPath(_position, _target, OnPathGenerationComplete);
        _currentPoint = 0;
    }


    void Start()
    {
        _position = transform.position;
        
        _seeker = GetComponent<Seeker>();
        _target = Field.Instance.GetRandomPoint();
        
        
        _seeker.StartPath(_position, _target, OnPathGenerationComplete);

        _shotTimer = gameObject.AddComponent<Timer>();
        _shotTimer.SetTimer(shotCooldown);

    }

    // Update is called once per frame
    void Update()
    {
        if (_path == null)
            return;
        
        Run();
        
        transform.rotation = _rotation;
        transform.position = _position;
    }

    private bool ReachedEndCheck()
    {
        return _currentPoint >= _path.vectorPath.Count; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            UpdateTarget(other.transform.position);
            _triggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            _canMove = true;
            _triggered = false;
        }
    }
}
