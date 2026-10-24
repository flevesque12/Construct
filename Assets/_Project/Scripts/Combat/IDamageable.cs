namespace Construct.Combat
{
    // Anything that can be hit. Hitboxes only know this contract, not what they hit.
    public interface IDamageable
    {
        void ReceiveHit(in HitInfo hit);
    }
}
