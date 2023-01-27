using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Avatar : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    
    private HashSet<(int x, int y)> walls;

    private void Start()
    {
        walls = SearchUtils.FindWalls().ToHashSet();
    }

    public void MoveToPosition(Vector3 position)
    {
        StopAllCoroutines();
        
        if (!SearchUtils.FindPath(
                transform.position.ToCoordinates(),
            position.ToCoordinates(),
                walls,
                out var path))
        {
            Debug.Log("Failed to find a path to the destination.");
            return;
        }

        var curvedPath = SplineFactory.CreateCatmullRom(
            path
                .Select(coordinates => coordinates.ToFloat3())
                .ToList());
        
        StartCoroutine(FollowPath(curvedPath));
    }

    private IEnumerator FollowPath(Spline path)
    {
        var distance = path.GetLength();
        var weight = 0.0f;

        while (weight <= 1.0f)
        {
            path.Evaluate(weight, out var position, out _, out _);
            transform.position = position;
            weight += Time.deltaTime * speed / distance;
            yield return null;
        }
    }
}
