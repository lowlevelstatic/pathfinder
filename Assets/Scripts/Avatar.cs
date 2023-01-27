using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Avatar : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;

    private const float AvatarRadius = 0.5f;
    private static readonly float WallRadius = Mathf.Sqrt(0.5f * 0.5f + 0.5f * 0.5f);
    
    private List<Transform> walls;
    private HashSet<(int x, int y)> wallCoordinates;

    private void Start()
    {
        walls = SearchUtils.FindWalls().ToList();
        wallCoordinates = walls
            .Select(wall => wall.position)
            .Select(SearchExtensions.ToCoordinates)
            .ToHashSet();
    }

    public void MoveToPosition(Vector3 position)
    {
        StopAllCoroutines();
        
        if (!SearchUtils.FindPath(
                transform.position.ToCoordinates(),
            position.ToCoordinates(),
                wallCoordinates,
                out var path))
        {
            Debug.Log("Failed to find a path to the destination.");
            return;
        }

        var smoothedPath = SearchUtils.SmoothPath(
            path.Select(SearchExtensions.ToVector3).ToList(),
            walls,
            AvatarRadius,
            WallRadius);
        
        var curvedPath = SplineFactory.CreateCatmullRom(
            smoothedPath
                .Select(SearchExtensions.ToFloat3)
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
