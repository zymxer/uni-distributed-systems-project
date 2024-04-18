using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class Tank : MonoBehaviour
{
    //CONST
    [Header("Tank Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationThreshold;
    [SerializeField] private GameObject missile;
    [SerializeField] private float shotCooldown;
    [SerializeField] private float attackRange;
    
    [Space] [Header("Navigation")] 
    [SerializeField] private float nextPointDistance = 0.1f;

    [Space] [Header("Team")] 
    private Team2 _team;
    private int teamNumber;
    private int tankNumber;
    [SerializeField] private GameObject flag;
    
    private Timer _shotTimer;
    private Seeker _seeker;
    
    
    private Thread _thread;
    private bool _threadStopped = false;
    private AutoResetEvent _resetEvent;
    private readonly object _lockObject = new object();
    
    //VARIABLES
    private Vector3 _position;
    private Quaternion _rotation;
    
    private bool _pathFinished = false;
    private bool _shoots = false;
    
    private Path _path;
    
    private Vector3 _target;
    private int _currentPoint;
    private Vector3 _direction;
    private float _distance;
    private float _angle;

    private bool _canMove = true;
    private bool _triggered = false;
    
    //CHANGED IN MAIN THREAD
    private float _deltaTime;

    
    private void Start()
    {
        _position = transform.position;
        
        _seeker = GetComponent<Seeker>();
        _target = Field.Instance.GetRandomPoint();
        
        
        _seeker.StartPath(_position, _target, OnPathGenerationComplete);

        _shotTimer = gameObject.AddComponent<Timer>();
        _shotTimer.SetTimer(shotCooldown);

        _resetEvent = new AutoResetEvent(false);
        _thread = new Thread(Run);
        _thread.Start();

    }
    
    private void Update()
    {
        _resetEvent.Set(); // allows thread function to continue executing
        if (_path == null)
            return;
        
        _deltaTime = Time.deltaTime;
        
        if (_pathFinished)
        {
            UpdateTarget(Field.Instance.GetRandomPoint());
            _pathFinished = false;
        }

        if (_shoots)
        {
            Shoot();
        }
        
        transform.rotation = _rotation;
        transform.position = _position;
    }
    
    private void ThreadFunction()
    {
        _resetEvent.WaitOne();  // waits for signal from Update()
        if (_pathFinished)
            return;
        UpdatePath();
        if (_pathFinished)
            return;
        if (_path == null) 
            return;
        
        UpdateDirection();
        UpdateRotation();
        if (_canMove)
        {
            UpdatePosition();
        }

        if (_triggered)
        {
            _canMove = false;
            _shoots = true;
        }
    }

    public void SetTeam(Team2 team, int tankNumber)
    {
        _team = team;
        this.tankNumber = tankNumber;
        teamNumber = team.TeamNumber;
        flag.GetComponent<SpriteRenderer>().color = _team.Color;
    }

    public int TeamNumber => teamNumber;

    private void OnPathGenerationComplete(Path path)
    {
        if (!path.error)
        {
            _path = path;
            _currentPoint = 0;
        }
    }
    
    private void UpdateDirection()
    {
        _direction = (_path.vectorPath[_currentPoint] - _position).normalized;
        _angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
    }
    
    private void UpdatePath()
    {
        if(_path == null)
            return;
        _distance = Vector2.Distance(_position, _path.vectorPath[_currentPoint]);
        if (_distance < nextPointDistance)
        {
            _currentPoint++;
        }

        ReachedEndCheck();
    }

    private void Shoot()
    {
        if (!_shotTimer.IsActive())
        {
            Instantiate(missile, _position, _rotation);
            _shotTimer.Activate();
            _shoots = false;
        }
    }

    private void UpdateRotation()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, _angle - 90f);
        _rotation = Quaternion.Lerp(_rotation, targetRotation, rotationSpeed * _deltaTime);
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
        _position += _direction * (speed * _deltaTime);
    }

    private void UpdateTarget(Vector3 value)
    {
        _target = value;
        _seeker.StartPath(_position, _target, OnPathGenerationComplete);
        _currentPoint = 0;
    }
    
    private void ReachedEndCheck()
    {
        if(_currentPoint >= _path.vectorPath.Count)
        {
            _pathFinished = true;
        }
    }
    
    private void Run()
    {
        while (!_threadStopped)  
        {  
            ThreadFunction();  
        } 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            if (other.GetComponent<Tank>().TeamNumber != teamNumber)
            {
                //UpdateTarget(other.transform.position);
                _triggered = true;   
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tank"))
        {
            if (other.GetComponent<Tank>().TeamNumber != teamNumber)
            {
                _canMove = true;
                _triggered = false;
            }
        }
    }

    private void OnDestroy()
    {
        _thread.Abort();
        _threadStopped = true;
    }

    private void OnApplicationQuit()
    {
        _thread.Abort();
        _threadStopped = true;
    }
}
