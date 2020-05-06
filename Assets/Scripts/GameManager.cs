using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager RT { get; private set; }

    #endregion

    public bool GameRunning { get; private set; }

    public float ScreenExtentsY { get { return _mainCamera.orthographicSize; } }

    public float MovementSpeed { get { return _keepScrolling ? _movementSpeed * _baseMovementSpeed * Time.deltaTime : 0.0f; } }

    public int NumberOfLanes { get { return _lanes.Length; } }

    [Header("Scene Refs")]
    [SerializeField] private Transform _lanesParent;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private RoadSegment _segment0;
    [SerializeField] private RoadSegment _segment1;
    [SerializeField] private Transform[] _bonusLanes;

    [Header("Gameplay Digits")]
    [SerializeField] private float _levelDistance = 1000f;
    [SerializeField] private float _maxFuel = 55f;
    [SerializeField] private AnimationCurve _speedIncreaseCurve;
    [SerializeField] private float _baseMovementSpeed = 18f;
    [SerializeField] private float _speedIncreaseFactor = 0.07f;
    [SerializeField] private float _bonusSpawnChance = 0.5f;
    [SerializeField] private float _bonusDistanceMultiplier = 150f;
    [SerializeField] private float _bonusFuelRatio = 0.1f;

    [Header("Spawning")]
    [SerializeField] private int _objectsPerSegment = 3;

    private bool _keepScrolling;
    private Camera _mainCamera;
    private ObsticleSpawner _spawner;
    private Transform[] _lanes;
    private float _currentDistance;
    private float _movementSpeed;
    private float _currentSpeed;
    private float _currentFuel;

    private void Awake()
    {
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
        Messenger.Register<Missile.HitEvent>(HandleMissileHit);
    }

    public void StartGame()
    {
        GameRunning = true;
        _keepScrolling = true;
        _segment0.ClearContents();
        _segment1.ClearContents();
        _currentDistance = 0;
        _currentSpeed = 0;
        _movementSpeed = 0;
        _currentFuel = _maxFuel;
        SoundManager.RT.PlaySound(1);
    }

    private void Update()
    {
        UpdateSpeed();
        UpdateDistance();
        UpdateFuel();
    }

    private void OnDestroy()
    {
        Messenger.Unregister<PlayerHealth.CollisionEvent>(HandlePlayerCollision);
    }

    #region Way Management

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
        
        _currentSpeed += Time.deltaTime * _speedIncreaseFactor;
        _movementSpeed = _speedIncreaseCurve.Evaluate(_currentSpeed);
        _uiManager.UpdateSpeed(_currentSpeed / 1.0f);
    }

    private void UpdateDistance()
    {
        if (!GameRunning)
        {
            return;
        }
        
        _currentDistance += MovementSpeed;
        _uiManager.UpdateDistance(_currentDistance / _levelDistance);

        if (_currentDistance > _levelDistance)
        {
            // Game won
            WinState();
        }
    }

    private void UpdateFuel()
    {
        if (!GameRunning)
        {
            return;
        }

        _currentFuel -= Time.deltaTime;
        _uiManager.UpdateFuel(_currentFuel / _maxFuel);

        if (_currentFuel < 0)
        {
            LoseState();
        }
    }

    #endregion

    #region Game States

    private void WinState()
    {
        Debug.LogError("<color=green>GAME WON!</color>");
        _keepScrolling = false;
        _uiManager.Trigger("Win");
        GameRunning = false;
    }

    private void LoseState()
    {
        Debug.LogError("<color=red>GAME LOST!</color>");
        _uiManager.Trigger("Lose");
        _keepScrolling = false;
        GameRunning = false;
        SoundManager.RT.PlaySound(5);
    }

    #endregion

    #region Bonuses

    private void HandleMissileHit(Missile.HitEvent arg0)
    {
        if (arg0.Other.CompareTag("Bonus_Distance"))
        {
            GiveDistanceBonus();
            
        }
        
        if (arg0.Other.CompareTag("Bonus_Fuel"))
        {
            GiveFuelBonus();
        }
        
        Destroy(arg0.Other.gameObject);
    }

    private void GiveDistanceBonus()
    {
        _currentDistance += MovementSpeed * _bonusDistanceMultiplier;
        SoundManager.RT.PlaySound(2);
    }

    private void GiveFuelBonus()
    {
        _currentFuel += _maxFuel * _bonusFuelRatio;
        SoundManager.RT.PlaySound(3);
    }

    #endregion

    #region Player Events

    private void HandlePlayerCollision(PlayerHealth.CollisionEvent arg0)
    {
        Debug.LogError("PlayerHit!");
        _currentSpeed = Mathf.Clamp01(_currentSpeed - 0.6f);
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
        SetupBonuses(segment);
    }

    private void SetupObsticles(RoadSegment segment)
    {
        _spawner.SetupObsticles(segment, _objectsPerSegment);
    }

    private void SetupBonuses(RoadSegment segment)
    {
        if (Random.Range(0.0f, 1.0f) > _bonusSpawnChance)
        {
            // Failed
            return;
        }

        var y = Random.Range(0.0f, ScreenExtentsY * 2.0f);
        var x = _bonusLanes[Random.Range(0, _bonusLanes.Length)].position.x;
        
        _spawner.SpawnBonus(new Vector3(x, y, 0.0f), segment.BonusParent);
    }

    #endregion

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GameRunning)
        {
            return;
        }
        
        Debug.Log("Pos: " + eventData.position);
    }
}
