using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class SearchUtils
{
    private readonly struct PathNode
    {
        public readonly (int x, int y) Coordinates;
        public readonly (int x, int y) Parent;
        public readonly int Cost;
        public readonly int Heuristic;

        public PathNode((int x, int y) coordinates, (int x, int y) parent, int cost, int heuristic)
        {
            Coordinates = coordinates;
            Parent = parent;
            Cost = cost;
            Heuristic = heuristic;
        }
    }

    private const float StepSize = 0.5f;
    
    public static IEnumerable<Transform> FindWalls() => GameObject.FindGameObjectsWithTag("Wall")
        .Select(wall => wall.transform);
    
    public static bool FindPath(
        (int x, int y) start,
        (int x, int y) end,
        ICollection<(int x, int y)> walls,
        out List<(int x, int y)> path)
    {
        var open = new List<PathNode>();
        var closed = new List<PathNode>();

        foreach (var neighbor in start.GetNeighbors()
                .Where(coordinate => !walls.Contains(coordinate)))
        {
            open.Add(new PathNode(
                neighbor,
                start,
                1,
                Estimate(neighbor, end)));
        }

        open.Sort(ComparePathsShort);

        while (open.Any())
        {
            var current = open.First();
            open.Remove(current);
            closed.Add(current);

            if (current.Coordinates == end)
            {
                // Got there! Exit and build path.
                var result = new List<(int x, int y)> {end};

                while (!result.Contains(start))
                {
                    var next = closed.First(element => element.Coordinates == result.Last());
                    result.Add(next.Parent);
                }

                result.Reverse();
                path = result;
                return true;
            }

            foreach (var neighbor in current.Coordinates.GetNeighbors()
                         .Where(neighbor => closed.All(element => element.Coordinates != neighbor))
                         .Where(neighbor => open.All(element => element.Coordinates != neighbor))
                         .Where(coordinate => !walls.Contains(coordinate)))
            {
                open.Add(new PathNode(
                    neighbor,
                    current.Coordinates,
                    current.Cost + 1,
                    Estimate(neighbor, end)));
            }

            open.Sort(ComparePathsShort);
        }

        path = new List<(int x, int y)>();
        return false;
    }

    private static int Estimate((int x, int y) start, (int x, int y) end) =>
        Math.Abs(end.x - start.x) + Math.Abs(end.y - start.y);

    private static int ComparePathsShort(PathNode first, PathNode second) =>
        (first.Cost + first.Heuristic).CompareTo(second.Cost + second.Heuristic);

    public static List<Vector3> SmoothPath(
        List<Vector3> path,
        ICollection<Transform> walls,
        float avatarRadius,
        float wallRadius)
    {
        var radiusSquared = (avatarRadius + wallRadius) * (avatarRadius + wallRadius);

        var forwardPath = path.ToList();
        var frontToBack = CullPath(forwardPath, walls, radiusSquared);

        var reversePath = path.ToList();
        reversePath.Reverse();
        var backToFront = CullPath(reversePath, walls, radiusSquared);
        backToFront.Reverse();

        return ComputeLength(frontToBack) < ComputeLength(backToFront) ? frontToBack : backToFront;
    }

    private static List<Vector3> CullPath(
        ICollection<Vector3> path,
        ICollection<Transform> walls,
        float radiusSquared)
    {
        var result = new List<Vector3>();
        
        var current = path.First();
        path.Remove(current);
        result.Add(current);

        var intermediate = path.First();
        path.Remove(intermediate);
        
        while (path.Any())
        {
            var next = path.First();
            path.Remove(next);

            if (!IsWalkable(current, next, walls, radiusSquared))
            {
                result.Add(intermediate);
                current = intermediate;
            }

            intermediate = next;
            
            if (!path.Any())
            {
                result.Add(next);
            }
        }

        return result;
    }
    
    private static bool IsWalkable(
        Vector3 start,
        Vector3 end,
        ICollection<Transform> walls,
        float radiusSquared)
    {
        var direction = end - start;
        var steps = Mathf.FloorToInt(direction.magnitude / StepSize);

        for (int step = 1; step < steps; ++step)
        {
            var position = start + direction * step / steps;
            if (walls.Any(wall => (position - wall.position).sqrMagnitude < radiusSquared))
            {
                return false;
            }
        }
        
        return true;
    }

    private static float ComputeLength(List<Vector3> path)
    {
        var sum = 0.0f;
        
        for (int index = 1; index < path.Count; ++index)
        {
            sum += (path[index] - path[index - 1]).magnitude;
        }

        return sum;
    }
}
