using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Furkan
{
    /// <summary>
    /// A graph structure for pathfinding using A* search algorithm.
    /// Vertices are identified by Vector3 positions.
    /// </summary>
    public class AdjacencyGraph
    {
        private Dictionary<Vector3, List<Vector3>> adjacencyList = new();

        public void AddVertex(Vector3 vertex)
        {
            if (!adjacencyList.ContainsKey(vertex))
            {
                adjacencyList[vertex] = new List<Vector3>();
            }
        }

        public void AddEdge(Vector3 from, Vector3 to)
        {
            AddVertex(from);
            AddVertex(to);

            if (!adjacencyList[from].Contains(to))
            {
                adjacencyList[from].Add(to);
            }
        }

        public void ClearGraph()
        {
            adjacencyList.Clear();
        }

        public List<Vector3> GetVertices()
        {
            return adjacencyList.Keys.ToList();
        }

        public List<Vector3> GetConnectedVerticesTo(Vector3 vertex)
        {
            if (adjacencyList.ContainsKey(vertex))
            {
                return new List<Vector3>(adjacencyList[vertex]);
            }
            return new List<Vector3>();
        }

        /// <summary>
        /// A* pathfinding algorithm. Returns the shortest path from start to goal, or null if not found.
        /// </summary>
        public static List<Vector3> AStarSearch(AdjacencyGraph graph, Vector3 start, Vector3 goal)
        {
            if (graph == null || graph.adjacencyList.Count == 0)
                return null;

            var openSet = new HashSet<Vector3> { start };
            var cameFrom = new Dictionary<Vector3, Vector3>();
            var gScore = new Dictionary<Vector3, float>();
            var fScore = new Dictionary<Vector3, float>();

            foreach (var vertex in graph.adjacencyList.Keys)
            {
                gScore[vertex] = float.MaxValue;
                fScore[vertex] = float.MaxValue;
            }

            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(v => fScore.ContainsKey(v) ? fScore[v] : float.MaxValue).First();

                if (Vector3.Distance(current, goal) < 0.1f)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (var neighbor in graph.GetConnectedVerticesTo(current))
                {
                    var tentativeGScore = gScore[current] + Vector3.Distance(current, neighbor);

                    if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private static float Heuristic(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        private static List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
        {
            var path = new List<Vector3> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
