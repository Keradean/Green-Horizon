using System.Collections.Generic;
using UnityEngine;
using Dennis.Placement.Building;
//*** De Col ***\\
namespace Dennis.Placement.Road
{
    public static class RoadResolver
    {
        // Normaler Aufruf für echtes Grid
        public static (GameObject prefab, float rotation) Resolve(
            Vector2Int cell, BuildingGrid grid, RoadData data)
        {
            return Resolve(cell, grid, data, null);
        }

        // Erweiterter Aufruf mit temporärem Pfad für Preview
        public static (GameObject prefab, float rotation) Resolve(
            Vector2Int cell, BuildingGrid grid, RoadData data, HashSet<Vector2Int> tempRoads)
        {
            var n = IsRoad(cell + Vector2Int.up,    grid, tempRoads);
            var s = IsRoad(cell + Vector2Int.down,  grid, tempRoads);
            var e = IsRoad(cell + Vector2Int.right, grid, tempRoads);
            var w = IsRoad(cell + Vector2Int.left,  grid, tempRoads);

            var connections = (n ? 1 : 0) + (s ? 1 : 0)
                            + (e ? 1 : 0) + (w ? 1 : 0);
            return connections switch
            {
                // ── Kreuzung ──────────────────────────────────────
                4 => (data.crossPrefab, 0f),
                // ── T-Kreuzungen ──────────────────────────────────
                3 when n && s && e => (data.tJunctionPrefab,  180f),
                3 when n && s && w => (data.tJunctionPrefab, 0f),
                3 when s && e && w => (data.tJunctionPrefab,   -90f),
                3 when n && e && w => (data.tJunctionPrefab, -270f),
                // ── Ecken ─────────────────────────────────────────
                2 when n && e      => (data.cornerPrefab,  -90f),
                2 when n && w      => (data.cornerPrefab, 180f),
                2 when s && w      => (data.cornerPrefab, -270f),
                2 when s && e      => (data.cornerPrefab,   0f),
                // ── Gerade ────────────────────────────────────────
                2 when n && s => (data.straightPrefab,  90f),
                2 when e && w => (data.straightPrefab,   0f),
                1 when n || s => (data.straightPrefab,  90f),  // einzelne mit N oder S Verbindung
                _             => (data.straightPrefab,   0f),   // einzelne mit E oder W
            };
        }

        // Prüft ob Zelle eine Straße ist — im echten Grid oder im temporären Pfad
        private static bool IsRoad(Vector2Int cell, BuildingGrid grid, HashSet<Vector2Int> tempRoads)
        {
            return grid.IsRoad(cell) || (tempRoads != null && tempRoads.Contains(cell));
        }
    }
}