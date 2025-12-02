using UnityEngine;
public static class GravityExtensions
{
    public static Vector3 GetVector(this GravityDirection direction)
    {
        return direction switch
        {
            GravityDirection.SOUTH => Vector3.down,
            GravityDirection.EAST => Vector3.right,
            GravityDirection.NORTH => Vector3.up,
            GravityDirection.WEST => Vector3.left,
            _ => Vector3.down,
        };
    }
}