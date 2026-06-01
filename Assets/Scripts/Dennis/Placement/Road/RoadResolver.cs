using UnityEngine;
using Dennis.Placement.Building;
//*** De Col ***\\
namespace Dennis.Placement.Road
{
    public static class RoadResolver
    {
        public static (GameObject prefab, float rotation) Resolve(
            Vector2Int cell, BuildingGrid grid, RoadData data)
        {
            var n = grid.IsRoad(cell + Vector2Int.up);
            var s = grid.IsRoad(cell + Vector2Int.down);
            var e = grid.IsRoad(cell + Vector2Int.right);
            var w = grid.IsRoad(cell + Vector2Int.left);
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
    }
}