using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int CurrentLane { get; private set; }
    public bool InAir { get; private set; }

    public Missile MissilePrefab;
    public float TurnLerpIncrease = 1.0f;

    private int _targetLane;
    private bool _isTurning;
    private Animator _animation;

    private void Awake()
    {
        _targetLane = 0;
        _animation = GetComponent<Animator>();
        MasterPooler.InitPool<Missile>(MissilePrefab);
    }

    private void Start()
    {
        PositionPlayerImmidiate(_targetLane);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            TurnRight();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            TurnLeft();
        }

        if (_targetLane != CurrentLane)
        {
            MoveToLane();
        }
    }
    
    private void PositionPlayerImmidiate(int layerIndex)
    {
        var newPos = transform.position;
        newPos.x = GameManager.RT.GetLaneX(layerIndex);
        transform.position = newPos;
        CurrentLane = layerIndex;
    }

    public void TurnRight()
    {
        if (!CanTurn(CurrentLane + 1))
        {
            return;
        }
        
        FireMissile(-1.0f);
        ChangeLane(CurrentLane + 1);
    }

    public void TurnLeft()
    {
        if (!CanTurn(CurrentLane - 1))
        {
            return;
        }
        
        FireMissile(1.0f);
        ChangeLane(CurrentLane - 1);
    }

    private void FireMissile(float directionMod)
    {
        var missile = MasterPooler.Get<Missile>(transform.position, transform.rotation);
        missile.Shoot(directionMod);
    }

    private bool CanTurn(int newLane)
    {
        if (_isTurning)
        {
            return false;
        }
        
        if (!GameManager.RT.IsValidLane(newLane))
        {
            return false;
        }

        return true;
    }

    private void ChangeLane(int newLane)
    {
        _targetLane = newLane;
        _isTurning = true;
        _animation.SetTrigger("Jump");
    }

    private void MoveToLane()
    {
        // Calculate current pos
        var currentLaneX = GameManager.RT.GetLaneX(CurrentLane);
        var targetLaneX = GameManager.RT.GetLaneX(_targetLane);
        var progress = Mathf.InverseLerp(currentLaneX, targetLaneX, transform.position.x);

        // Increase and apply
        progress += Time.deltaTime * TurnLerpIncrease;

        if (progress > 0.95f)
        {
            progress = 1.0f;
            _isTurning = false;
            CurrentLane = _targetLane;
        }

        // Mark player in the air for the middle of turn
        InAir = progress > 0.1f && progress < 0.75f;

        var newPos = transform.position;
        newPos.x = Mathf.Lerp(currentLaneX, targetLaneX, progress);
        transform.position = newPos;
    }
}
