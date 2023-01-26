using UnityEngine;

public class Avatar : MonoBehaviour
{
    public void MoveToPosition(Vector3 position)
    {
        var worldTransform = transform;
        position.y = worldTransform.position.y;
        worldTransform.position = position;
    }
}
