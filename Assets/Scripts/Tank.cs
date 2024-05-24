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
    [SerializeField] private bool active = true;
    // Constants and Serialized Fields
    [Header("Tank Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationThreshold;
    [SerializeField] private GameObject missile;
    [SerializeField] private float shotCooldown;
    [SerializeField] private float attackRange;
    [Space]
    [Header("Navigation")]
    [SerializeField] private float nextPointDistance = 0.1f;
    [Space]
    [Header("Team")]
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

    // Variables
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
        if (!active)
            return;
        
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
        if(!active)
            return;
        
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
            else if (!_isDefender)
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
    
    public void SetTeam(int teamNumber, Color teamColor)
    {
        this.teamNumber = teamNumber;
        flag.GetComponent<SpriteRenderer>().color = teamColor;
    }

    public int TeamNumber => teamNumber;
    public bool Active => active;
    public bool IsDefender => _isDefender;
    public bool IsTriggered => _triggered;

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
        if (_path == null)
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
            _team.MissilesObjects.Add(Instantiate(missile, _position, _rotation));
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
        if (_currentPoint >= _path.vectorPath.Count)
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
        if(!active)
            return;
        
        if (other.CompareTag("Tank"))
        {
            if (other.isTrigger)
                return;
            Tank enemyTank = other.GetComponent<Tank>();
            if (enemyTank.TeamNumber == teamNumber)
                return;
            _triggered = true;
            _team.ReportEnemySpotted(enemyTank); // reporting

            if (_capturing)
            {
                UpdateTarget(other.transform.position);
            }
        }
        else if (other.CompareTag("Base"))
        {
            Team2 enemyBase = other.GetComponent<Team2>();
            if (enemyBase.TeamNumber != teamNumber)
            {
                _capturing = true;
                capturedTeamNumber = enemyBase.TeamNumber;
                UpdateTarget(enemyBase.GetRandomPoint(false));
                _team.ReportEnemyBaseSpotted(enemyBase); // reporting enemy base
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!active)
            return;
        
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

    public void OnEnemyBaseSpotted(Team2 enemyBase)
    {
        UpdateTarget(enemyBase.transform.position); // Setting current target to enemy base
        Debug.Log($"Tank {tankNumber} from Team {teamNumber} received enemy base spotted information"); // Notifying the tank
    }

    public void OnEnemySpotted(Tank enemyTank)
    {
        UpdateTarget(enemyTank.transform.position); // Setting current target
        Debug.Log($"Tank {tankNumber} from Team {teamNumber} received enemy spotted information"); // Notifying the team
    }
}