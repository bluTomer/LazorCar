using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour, IPoolable
{
    public float Speed;
    public float DragFactor = 0.5f;
    public bool IsMoving;
    public SpriteRenderer Image;

    private float _directionMod;

    private void Update()
    {
        if (!IsMoving)
        {
            return;
        }

        var movement = new Vector3(
            Speed * Time.deltaTime * _directionMod,
            DragFactor * Time.deltaTime * -1.0f
            );
        
        transform.Translate(movement);
    }

    public void Shoot(float directionMod)
    {
        _directionMod = directionMod;
        SetImageOrientation();
        IsMoving = true;
    }

    private void SetImageOrientation()
    {
        var newScale = Image.transform.localScale;
        newScale.x *= _directionMod;
        Image.transform.localScale = newScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        MasterPooler.Return(this);
        
        if (other.CompareTag("Killzone"))
        {
            return;
        }

        Messenger.Broadcast(new HitEvent
        {
            Other = other,
            Position = transform.position
        });
    }

    public void Init()
    {
    }

    public void Reset()
    {
    }

    public class HitEvent
    {
        public Vector3 Position;
        public Collider Other;
    }
}
