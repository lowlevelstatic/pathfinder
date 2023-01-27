using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class SearchExtensions
{ 
    public static (int x, int y) ToCoordinates(this Vector3 source) =>
        (Mathf.RoundToInt(source.x), Mathf.RoundToInt(source.z));

    public static Vector3 ToVector3(this (int x, int y) source) =>
        new (source.x, 0.5f, source.y);
    
    public static float3 ToFloat3(this Vector3 source) =>source;
    
    public static IEnumerable<(int x, int y)> GetNeighbors(this (int x, int y) source)
    {
        yield return source.GetNorth();
        yield return source.GetEast();
        yield return source.GetSouth();
        yield return source.GetWest();
    }
    
    public static (int x, int y) GetNorth(this (int x, int y) source) => new(source.x, source.y - 1);
    public static (int x, int y) GetEast(this (int x, int y) source) => new(source.x + 1, source.y);
    public static (int x, int y) GetSouth(this (int x, int y) source) => new(source.x, source.y + 1);
    public static (int x, int y) GetWest(this (int x, int y) source) => new(source.x - 1, source.y);
}