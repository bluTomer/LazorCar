using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float MaxHealth = 100.0f;
    public float CurrentHealth = 100.0f;
    public float CollisionDamage = 20.0f;

    private PlayerController _controller;
    
    private void Awake()
    {
        CurrentHealth = MaxHealth;
        _controller = GetComponent<PlayerController>();
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0.0f);

        if (CurrentHealth > 0)
        {
            var e = new CollisionEvent();
            e.CurrentHealth = CurrentHealth;
            e.MaxHealth = MaxHealth;
            Messenger.Broadcast(e);
        }
        else
        {
            Messenger.Broadcast(new DeathEvent());
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_controller.InAir)
        {
            // No collisions in the air
            return;
        }
        
        TakeDamage(CollisionDamage);
    }

    public class CollisionEvent
    {
        public float CurrentHealth;
        public float MaxHealth;
    }
    
    public class DeathEvent {}
}
