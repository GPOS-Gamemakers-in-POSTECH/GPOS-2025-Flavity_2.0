using UnityEngine;

public class GravitySwitcher : MonoSingleton<GravitySwitcher>
{
    public GravityDirection CurrentGravityDirection { get; private set; } = GravityDirection.SOUTH;

    public void SwitchGravity(GravityDirection gravityDirection)
    {
        CurrentGravityDirection = gravityDirection;

        Physics.gravity = gravityDirection.GetVector() * FlavityConstants.GRAVITY_MAGNITUDE;

        MyDebug.Log("새로운 중력 방향: " + Physics.gravity);
    }
}