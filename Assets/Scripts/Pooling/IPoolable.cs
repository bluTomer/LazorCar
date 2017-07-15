public interface IPoolable
{
    // Called when the object is retreived from the pool
    void Init();

    // Called when the object is returned to the pool
    void Reset();
}
