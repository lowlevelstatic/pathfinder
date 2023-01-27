using System;
using System.Collections.Generic;
using System.Linq;
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

    public static IEnumerable<(int x, int y)> FindWalls() => GameObject.FindGameObjectsWithTag("Wall")
        .Select(wall => wall.transform.position)
        .Select(SearchExtensions.ToCoordinates);
    
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
}
