namespace Construct.Core
{
    // What the aim reticle should show. Lives in Core so gameplay (Weapon) and the HUD (UI) share it
    // without depending on each other.
    public enum ReticleState
    {
        Hidden,     // not aiming
        Neutral,    // nothing in range: the throw would miss (white)
        Enemy,      // an enemy in range: the sword hits and returns (cyan)
        Breached    // a Breached enemy in range: the sword plants, hack possible (magenta)
    }
}
