namespace Construct.Core
{
    // What gameplay is allowed to ask of the camera. Only Construct.Cameras
    // implements it, so nothing else ever depends on Cinemachine.
    public interface ICameraRig
    {
        void SetLockTarget(ITargetable target);
        void ClearLockTarget();
    }
}
