using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Vector3Event onPressEvent;
    
    private void OnMouseDown()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit))
        {
            return;
        }

        onPressEvent?.Invoke(hit.point);
    }
}
