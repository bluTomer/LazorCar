﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    
    public static GameManager RT { get; private set; }
    
    #endregion
    public bool GameRunning { get; private set; }
    public float ScreenExtentsY { get { return _mainCamera.orthographicSize; }}
    public float MovementSpeed { get { return GameRunning ? _movementSpeed * _baseMovementSpeed * Time.deltaTime : 0.0f; }}
    public int NumberOfLanes { get { return _lanes.Length; } }

    [Header("Scene Refs")]
    [SerializeField] private Transform _lanesParent;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private RoadSegment _segment0;
    [SerializeField] private RoadSegment _segment1;
    
    [Header("Gameplay Digits")]
    [SerializeField] private float _baseMovementSpeed = 8.0f;
    [SerializeField] private AnimationCurve _speedIncreaseCurve;

    [Header("Spawning")]
    [SerializeField] private int _objectsPerSegment = 2;

    private float _movementSpeed;
    
    private Camera _mainCamera;
    private ObsticleSpawner _spawner;
    private Transform[] _lanes;
    private float _currentSpeed;
    
    private void Awake()
    {
        GameRunning = true;
        RT = this;
        _mainCamera = GetComponent<Camera>();
        _spawner = GetComponent<ObsticleSpawner>();
        
        // Build lane array
        _lanes = new Transform[_lanesParent.childCount];
        for (int i = 0; i < _lanesParent.childCount; i++)
        {
            _lanes[i] = _lanesParent.GetChild(i);
        }
        
        Messenger.Register<PlayerHealth.CollisionEvent>(HandlePlayerCollision);
        Messenger.Register<PlayerHealth.DeathEvent>(HandlePlayerDeath);
    }

    private void Update()
    {
        UpdateSpeed();
    }

    private void OnDestroy()
    {
        Messenger.Unregister<PlayerHealth.CollisionEvent>(HandlePlayerCollision);
        Messenger.Unregister<PlayerHealth.DeathEvent>(HandlePlayerDeath);
    }
    
    #region Speed Management
    
    private void UpdateSpeed()
    {
        if (!GameRunning)
        {
            return;
        }

        if (_currentSpeed >= 1)
        {
            return;
        }
        
        _currentSpeed += Time.deltaTime * 0.1f;
        _movementSpeed = _speedIncreaseCurve.Evaluate(_currentSpeed);
    }
    
    #endregion

    #region Player Events
    
    private void HandlePlayerCollision(PlayerHealth.CollisionEvent arg0)
    {
        Debug.LogError("PlayerHit!");
        _currentSpeed = 0.0f;
        _uiManager.UpdatePlayerHealth(arg0.CurrentHealth/arg0.MaxHealth);
    }

    private void HandlePlayerDeath(PlayerHealth.DeathEvent arg0)
    {
        Debug.LogError("PlayerDeath!");
    }
    
    #endregion
    
    #region Utils

    public float GetLaneX(int index)
    {
        index = Mathf.Clamp(index, 0, _lanes.Length - 1);

        return _lanes[index].position.x;
    }

    public bool IsValidLane(int index)
    {
        return index >= 0 && index < _lanes.Length;
    }

    public RoadSegment GetOtherSegment(RoadSegment segment)
    {
        return segment == _segment0 ? _segment1 : _segment0;
    }
    
    #endregion
    
    #region Road Setup

    public void SetupSegment(RoadSegment segment)
    {
        SetupObsticles(segment);
    }

    private void SetupObsticles(RoadSegment segment)
    {
        _spawner.SetupObsticles(segment, _objectsPerSegment);
    }
    
    #endregion
}
