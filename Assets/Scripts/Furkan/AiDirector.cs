using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dennis.Placement.Building;

namespace Furkan
{
    public class AiDirector : MonoBehaviour
    {
        public PlacementManager placementManager;
        public GameObject[] pedestrianPrefabs;

        public GameObject carPrefab;
        public BuildingGrid buildingGrid;

        [SerializeField] private float minSpawnInterval = 2f;
        [SerializeField] private float maxSpawnInterval = 5f;
        [SerializeField] private float maxActiveCars = 8f;
        [SerializeField] private float maxActivePedestrians = 12f;
        [SerializeField] private float densityCheckRadius = 6f;
        [SerializeField] private float laneOffset = 0.2f;
        [SerializeField] private float pedestrianLaneOffset = 0.3f;
        [SerializeField] private float rushHourMultiplier = 1.5f;
        [SerializeField] private float rushHourStart = 8f;
        [SerializeField] private float rushHourEnd = 18f;
        [SerializeField] private bool autoSpawnCars = true;
        [SerializeField] private bool autoSpawnPedestrians = true;

        private float nextCarSpawnTime;
        private float nextPedestrianSpawnTime;
        private List<CarAI> activeCars = new List<CarAI>();
        private List<PedestrianAI> activePedestrians = new List<PedestrianAI>();

        AdjacencyGraph pedestrianGraph = new AdjacencyGraph();
        AdjacencyGraph carGraph = new AdjacencyGraph();

        List<Vector3> carPath = new List<Vector3>();

        public void SpawnAllAagents()
        {
            if (placementManager == null || pedestrianPrefabs == null || pedestrianPrefabs.Length == 0)
                return;

            var specialStructures = placementManager.GetAllSpecialStructures();
            foreach (var house in placementManager.GetAllHouses())
            {
                var destination = placementManager.GetRandomSpecialStrucutre();
                if (destination == null && specialStructures.Count == 0)
                    destination = GetRandomHouseDestination(house);

                if (destination == null || destination == house)
                    continue;

                TrySpawningAnAgent(house, destination);
            }

            foreach (var specialStructure in specialStructures)
            {
                TrySpawningAnAgent(specialStructure, placementManager.GetRandomHouseStructure());
            }
        }

        private void TrySpawningAnAgent(StructureModel startStructure, StructureModel endStructure)
        {
            if (startStructure == null || endStructure == null || startStructure == endStructure)
                return;

            var startPosition = ((INeedingRoad)startStructure).RoadPosition;
            var endPosition = ((INeedingRoad)endStructure).RoadPosition;

            var startRoadStructure = placementManager.GetStructureAt(startPosition);
            var endRoadStructure = placementManager.GetStructureAt(endPosition);

            var startMarkerPosition = startRoadStructure != null
                ? startRoadStructure.GetPedestrianSpawnMarker(startStructure.transform.position)
                : new MarkerInfo { Position = startStructure.transform.position };

            var endMarkerPosition = endRoadStructure != null
                ? endRoadStructure.GetNearestPedestrianMarkerTo(endStructure.transform.position)
                : new MarkerInfo { Position = endStructure.transform.position };

            var pedestrianPrefab = GetRandomPedestrian();
            if (pedestrianPrefab == null)
                return;

            var agent = Instantiate(pedestrianPrefab, startMarkerPosition.Position, Quaternion.identity);

            var path = placementManager.GetPathBetween(startPosition, endPosition, true);
            if (path == null || path.Count == 0)
            {
                Destroy(agent);
                return;
            }

            path.Reverse();

            var markerCarPath = GetRoadMarkerCarPath(startPosition, endPosition);
            var fallbackRoadPath = BuildWorldPathFromGridPath(path);
            var chosenPath = markerCarPath.Count >= 2 ? markerCarPath : fallbackRoadPath;

            if (chosenPath == null || chosenPath.Count < 2)
            {
                chosenPath = GetPedestrianPath(path, startMarkerPosition.Position, endMarkerPosition.Position);
            }

            if (chosenPath == null || chosenPath.Count < 2)
            {
                chosenPath = new List<Vector3>
                {
                    startMarkerPosition.Position,
                    endMarkerPosition.Position
                };
            }

            var fullPath = ApplyLaneOffsetToPath(chosenPath, pedestrianLaneOffset);

            var pedestrianAi = agent.GetComponent<PedestrianAI>();
            if (pedestrianAi == null)
                pedestrianAi = agent.AddComponent<PedestrianAI>();

            pedestrianAi.SetPath(fullPath);
            activePedestrians.Add(pedestrianAi);
        }

        private void Awake()
        {
            if (buildingGrid == null)
                buildingGrid = UnityEngine.Object.FindFirstObjectByType<BuildingGrid>();
        }

        private void Start()
        {
            ScheduleNextCarSpawn();
            ScheduleNextPedestrianSpawn();
        }

        public void SpawnAPedestrian()
        {
            CleanupInactivePedestrians();
            if (GetActivePedestrianCountNearSpawnPoint() >= maxActivePedestrians)
                return;

            if (placementManager == null || pedestrianPrefabs == null || pedestrianPrefabs.Length == 0)
                return;

            var houses = placementManager.GetAllHouses();
            if (houses == null || houses.Count <= 1)
                return;

            var randomHouseIndex = UnityEngine.Random.Range(0, houses.Count);
            var randomHouse = houses[randomHouseIndex];

            var randomDestination = placementManager.GetRandomSpecialStrucutre();
            if (randomDestination == null)
                randomDestination = GetRandomHouseDestination(randomHouse);

            if (randomDestination == null)
                return;

            TrySpawningAnAgent(randomHouse, randomDestination);
        }

        public void SpawnACar()
        {
            CleanupInactiveCars();
            if (GetActiveCarCountNearSpawnPoint() >= maxActiveCars)
                return;

            var houses = placementManager.GetAllHouses();
            if (houses == null || houses.Count == 0)
                return;

            var randomHouseIndex = UnityEngine.Random.Range(0, houses.Count);
            var randomHouse = houses[randomHouseIndex];

            // Erst Spezialstruktur versuchen, dann fallback auf Haus-zu-Haus wenn keine Spezialstruktur vorhanden ist.
            var randomDestination = placementManager.GetRandomSpecialStrucutre();
            if (!TrySpawninACar(randomHouse, randomDestination))
            {
                var houseDestination = GetRandomHouseDestination(randomHouse);
                if (houseDestination != null)
                {
                    TrySpawninACar(randomHouse, houseDestination);
                }
            }
        }

        private StructureModel GetRandomHouseDestination(StructureModel sourceHouse)
        {
            var houses = placementManager.GetAllHouses();
            if (houses == null || houses.Count <= 1)
                return null;

            StructureModel destination = null;
            for (int i = 0; i < 10; i++)
            {
                var candidate = houses[UnityEngine.Random.Range(0, houses.Count)];
                if (candidate != sourceHouse)
                {
                    destination = candidate;
                    break;
                }
            }

            if (destination == null)
            {
                foreach (var house in houses)
                {
                    if (house != sourceHouse)
                    {
                        destination = house;
                        break;
                    }
                }
            }

            return destination;
        }

        private bool TrySpawninACar(StructureModel startStructure, StructureModel endStructure)
        {
            if (startStructure == null || endStructure == null || startStructure == endStructure)
                return false;

            var startRoadPosition = ((INeedingRoad)startStructure).RoadPosition;
            var endRoadPosition = ((INeedingRoad)endStructure).RoadPosition;

            var path = placementManager.GetPathBetween(startRoadPosition, endRoadPosition, true);
            if (path == null || path.Count < 2)
                return false;

            path.Reverse();
            var markerCarPath = GetRoadMarkerCarPath(startRoadPosition, endRoadPosition);

            var fallbackRoadPath = BuildWorldPathFromGridPath(path);
            var chosenPath = markerCarPath.Count >= 2 ? markerCarPath : fallbackRoadPath;

            if (chosenPath != null && chosenPath.Count >= 2)
            {
                var laneShiftedPath = ApplyLaneOffsetToPath(chosenPath, laneOffset);
                if (laneShiftedPath == null || laneShiftedPath.Count < 2)
                    return false;

                carPath = new List<Vector3>(laneShiftedPath);
                var car = Instantiate(carPrefab, carPath[0], Quaternion.identity);
                var carAi = car.GetComponent<CarAI>();
                if (carAi == null)
                {
                    Destroy(car);
                    return false;
                }

                carAi.SetPath(carPath);
                activeCars.Add(carAi);
                return true;
            }

            return false;
        }

        private List<Vector3> BuildWorldPathFromGridPath(List<Vector3Int> gridPath)
        {
            var worldPath = new List<Vector3>();
            if (gridPath == null || gridPath.Count < 2)
                return worldPath;

            if (buildingGrid == null)
                return worldPath;

            foreach (var step in gridPath)
            {
                var cell = new Vector2Int(step.x, step.z);
                if (!buildingGrid.IsRoad(cell))
                    continue;

                worldPath.Add(buildingGrid.CellToWorld(cell));
            }

            return worldPath;
        }

        private List<Vector3> ApplyLaneOffsetToPath(List<Vector3> sourcePath, float offset)
        {
            if (sourcePath == null || sourcePath.Count == 0 || Mathf.Approximately(offset, 0f))
                return sourcePath ?? new List<Vector3>();

            // Keep lane offset inside a safe portion of one grid cell width.
            offset = Mathf.Clamp(offset, -0.35f, 0.35f);

            var shifted = new List<Vector3>(sourcePath.Count);
            for (int i = 0; i < sourcePath.Count; i++)
            {
                Vector3 direction;
                if (i == 0)
                {
                    direction = sourcePath.Count > 1 ? sourcePath[1] - sourcePath[0] : Vector3.forward;
                }
                else if (i == sourcePath.Count - 1)
                {
                    direction = sourcePath[i] - sourcePath[i - 1];
                }
                else
                {
                    var inDir = sourcePath[i] - sourcePath[i - 1];
                    var outDir = sourcePath[i + 1] - sourcePath[i];
                    direction = (inDir.normalized + outDir.normalized);
                }

                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector3.forward;

                direction.y = 0f;
                direction.Normalize();

                // Right-hand lane offset: each direction gets its own side of the road.
                var side = Vector3.Cross(Vector3.up, direction).normalized;
                shifted.Add(sourcePath[i] + side * offset);
            }

            return shifted;
        }

        private List<Vector3> GetPedestrianPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            pedestrianGraph.ClearGraph();
            CreatAPedestrianGraph(path);
            Debug.Log(pedestrianGraph);
            return AdjacencyGraph.AStarSearch(pedestrianGraph, startPosition, endPosition);
        }


        private void CreatAPedestrianGraph(List<Vector3Int> path)
        {
            Dictionary<MarkerInfo, Vector3> tempDictionary = new Dictionary<MarkerInfo, Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(new Vector2Int(currentPosition.x, currentPosition.z));
                if (roadStructure == null) continue;

                var markersList = roadStructure.GetPedestrianMarkers();
                bool limitDistance = markersList.Count == 4;
                tempDictionary.Clear();
                foreach (var marker in markersList)
                {
                    pedestrianGraph.AddVertex(marker.Position);

                    if (i + 1 < path.Count)
                    {
                        var nextRoadStructure = placementManager.GetStructureAt(new Vector2Int(path[i + 1].x, path[i + 1].z));
                        if (nextRoadStructure != null)
                        {
                            var nextMarker = nextRoadStructure.GetNearestPedestrianMarkerTo(marker.Position);
                            if (limitDistance)
                            {
                                tempDictionary.Add(marker, nextMarker.Position);
                            }
                            else
                            {
                                pedestrianGraph.AddEdge(marker.Position, nextMarker.Position);
                            }
                        }
                    }
                }
                if (limitDistance && tempDictionary.Count >= 2)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    for (int j = 0; j < Mathf.Min(2, distanceSortedMarkers.Count); j++)
                    {
                        pedestrianGraph.AddEdge(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value);
                    }
                }
            }
        }

        private List<Vector3> GetRoadMarkerCarPath(Vector2Int startRoadPosition, Vector2Int endRoadPosition)
        {
            if (buildingGrid == null)
                return new List<Vector3>();

            var startMarker = buildingGrid.GetRoadMarkerAt(startRoadPosition);
            var endMarker = buildingGrid.GetRoadMarkerAt(endRoadPosition);
            if (startMarker == null || endMarker == null)
                return new List<Vector3>();

            carGraph.ClearGraph();
            var allMarkers = buildingGrid.GetRoadMarkers();
            foreach (var marker in allMarkers)
            {
                carGraph.AddVertex(marker.Position);
                foreach (var neighbour in marker.AdjacentMarkers)
                {
                    if (neighbour != null)
                        carGraph.AddEdge(marker.Position, neighbour.Position);
                }
            }

            var foundPath = AdjacencyGraph.AStarSearch(carGraph, startMarker.Position, endMarker.Position);
            return foundPath ?? new List<Vector3>();
        }

        private List<Vector3> GetCarPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            carGraph.ClearGraph();
            CreatACarGraph(path);
            Debug.Log(carGraph);

            var foundPath = AdjacencyGraph.AStarSearch(carGraph, startPosition, endPosition);
            if (foundPath == null || foundPath.Count < 2)
            {
                return new List<Vector3> { startPosition, endPosition };
            }

            return foundPath;
        }

        private void CreatACarGraph(List<Vector3Int> path)
        {
            Dictionary<MarkerInfo, Vector3> tempDictionary = new Dictionary<MarkerInfo, Vector3>();
            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(new Vector2Int(currentPosition.x, currentPosition.z));
                if (roadStructure == null) continue;

                var markersList = roadStructure.GetCarMarkers();
                var limitDistance = markersList.Count > 3;
                tempDictionary.Clear();

                foreach (var marker in markersList)
                {
                    carGraph.AddVertex(marker.Position);

                    if (i + 1 < path.Count)
                    {
                        var nextRoadPosition = placementManager.GetStructureAt(new Vector2Int(path[i + 1].x, path[i + 1].z));
                        if (nextRoadPosition != null)
                        {
                            if (limitDistance)
                            {
                                tempDictionary.Add(marker, nextRoadPosition.GetNearestCarMarkerTo(marker.Position).Position);
                            }
                            else
                            {
                                carGraph.AddEdge(marker.Position, nextRoadPosition.GetNearestCarMarkerTo(marker.Position).Position);
                            }
                        }
                    }
                }
                if (limitDistance && tempDictionary.Count > 2)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    for (int j = 0; j < Mathf.Min(2, distanceSortedMarkers.Count); j++)
                    {
                        Debug.Log(Vector3.Distance(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value));
                        carGraph.AddEdge(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value);
                    }
                }
            }
        }

        private GameObject GetRandomPedestrian()
        {
            if (pedestrianPrefabs == null || pedestrianPrefabs.Length == 0)
                return null;

            return pedestrianPrefabs[UnityEngine.Random.Range(0, pedestrianPrefabs.Length)];
        }

        private void Update()
        {
            if (autoSpawnPedestrians && Time.time >= nextPedestrianSpawnTime)
            {
                SpawnAPedestrian();
                ScheduleNextPedestrianSpawn();
            }

            if (autoSpawnCars && Time.time >= nextCarSpawnTime)
            {
                SpawnACar();
                ScheduleNextCarSpawn();
            }

            //DrawGraph(carGraph);
            for (int i = 1; i < carPath.Count; i++)
            {
                Debug.DrawLine(carPath[i - 1] + Vector3.up, carPath[i] + Vector3.up, Color.magenta);
            }
        }

        private void ScheduleNextCarSpawn()
        {
            var activeCarsCount = GetActiveCarCountNearSpawnPoint();
            var densityFactor = Mathf.Clamp01(activeCarsCount / maxActiveCars);
            var timeFactor = IsRushHour() ? rushHourMultiplier : 1f;
            var interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, densityFactor) / timeFactor;
            nextCarSpawnTime = Time.time + interval;
        }

        private void ScheduleNextPedestrianSpawn()
        {
            var activePedestriansCount = GetActivePedestrianCountNearSpawnPoint();
            var densityFactor = Mathf.Clamp01(activePedestriansCount / maxActivePedestrians);
            var timeFactor = IsRushHour() ? rushHourMultiplier : 1f;
            var interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, densityFactor) / timeFactor;
            nextPedestrianSpawnTime = Time.time + interval;
        }

        private int GetActiveCarCountNearSpawnPoint()
        {
            CleanupInactiveCars();
            var count = 0;
            foreach (var car in activeCars)
            {
                if (car != null && Vector3.Distance(car.transform.position, transform.position) <= densityCheckRadius)
                {
                    count++;
                }
            }
            return count;
        }

        private int GetActivePedestrianCountNearSpawnPoint()
        {
            CleanupInactivePedestrians();
            var count = 0;
            foreach (var pedestrian in activePedestrians)
            {
                if (pedestrian != null && Vector3.Distance(pedestrian.transform.position, transform.position) <= densityCheckRadius)
                {
                    count++;
                }
            }
            return count;
        }

        private bool IsRushHour()
        {
            var hour = System.DateTime.Now.Hour;
            return hour >= rushHourStart && hour < rushHourEnd;
        }

        private void CleanupInactiveCars()
        {
            activeCars.RemoveAll(car => car == null);
        }

        private void CleanupInactivePedestrians()
        {
            activePedestrians.RemoveAll(pedestrian => pedestrian == null);
        }

        private void DrawGraph(AdjacencyGraph graph)
        {
            foreach (var vertex in graph.GetVertices())
            {
                foreach (var vertexNeighbour in graph.GetConnectedVerticesTo(vertex))
                {
                    Debug.DrawLine(vertex + Vector3.up, vertexNeighbour + Vector3.up, Color.red);
                }
            }
        }


    }
}

