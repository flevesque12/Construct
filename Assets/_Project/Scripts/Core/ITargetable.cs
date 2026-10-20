using UnityEngine;

namespace Construct.Core
{
    /*
     Player, Weapon and enemies will depend on it
     */
    public interface ITargetable
    {
        //the position of the aim usually the chest of the target
        Transform AimPoint { get; }
        bool IsAlive {  get; }
    }
}
