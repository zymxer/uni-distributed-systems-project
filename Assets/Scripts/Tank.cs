using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine.Serialization;

[Serializable]
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
    private float _deltaTime;
    private Seeker _seeker;
    
    
    private Thread _thread;
    private bool _threadStopped = false;
    private AutoResetEvent _resetEvent;
    private readonly object _lockObject = new object();
    
    //VARIABLES
    private Vector3 _position;
    private Quaternion _rotation;
    
    private bool _pathFinished = true;

    private int capturedTeamNumber;
    
    private Path _path;
    private Vector3 _target;
    private int _currentPoint;
    private Vector3 _direction;
    private float _distance;
    private float _angle;

    private bool _canMove = true;
    private bool _triggered;
    private bool _capturing;
    private bool _isDefender;
    private bool _shoots;
    

    
    private void Start()
    {
        _position = transform.position;
        
        _seeker = GetComponent<Seeker>();

        _shotTimer = gameObject.AddComponent<Timer>();
        _shotTimer.SetTimer(shotCooldown);

        _resetEvent = new AutoResetEvent(false);
        _thread = new Thread(Run);
        _thread.Start();

    }
    
    private void Update()
    {
        _resetEvent.Set(); // allows thread function to continue executing
        
        if (_pathFinished && !_triggered)
        {
            if (_capturing)
            {
                UpdateTarget(Team2.Teams[capturedTeamNumber].GetRandomPoint(false));
            }
            else if (_isDefender)
            {
                UpdateTarget(_team.GetRandomPoint(true));
            }
            else if(!_isDefender)
            {
                UpdateTarget(Field.Instance.GetRandomPoint());   
            }
            _pathFinished = false;
        }
        
        if (_path == null)
            return;
        
        _deltaTime = Time.deltaTime;
        
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

    public void SetTeam(Team2 team, int tankNumber, bool isDefender)
    {
        _team = team;
        this.tankNumber = tankNumber;
        teamNumber = team.TeamNumber;
        _isDefender = isDefender;
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
            if (other.isTrigger) 
                return;
            if (other.GetComponent<Tank>().TeamNumber == teamNumber) 
                return;
            _triggered = true;
            if (_capturing)
            {
                UpdateTarget(other.transform.position);   
            }

        }
        else if(other.CompareTag("Base"))
        {
            if (other.GetComponent<Team2>().TeamNumber != teamNumber)
            {
                _capturing = true;
                capturedTeamNumber = other.GetComponent<Team2>().TeamNumber;
                UpdateTarget(other.GetComponent<Team2>().GetRandomPoint(false));
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
                return;
            _canMove = true;
            _triggered = false;
            _pathFinished = true;
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
