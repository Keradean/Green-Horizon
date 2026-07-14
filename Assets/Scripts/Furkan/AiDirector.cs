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
        [SerializeField] private float densityCheckRadius = 6f;
        [SerializeField] private float rushHourMultiplier = 1.5f;
        [SerializeField] private float rushHourStart = 8f;
        [SerializeField] private float rushHourEnd = 18f;
        [SerializeField] private bool autoSpawnCars = true;

        private float nextCarSpawnTime;
        private List<CarAI> activeCars = new List<CarAI>();

        AdjacencyGraph pedestrianGraph = new AdjacencyGraph();
        AdjacencyGraph carGraph = new AdjacencyGraph();

        List<Vector3> carPath = new List<Vector3>();

        public void SpawnAllAagents()
        {
            foreach (var house in placementManager.GetAllHouses())
            {
                TrySpawningAnAgent(house, placementManager.GetRandomSpecialStrucutre());
            }
            foreach (var specialStructure in placementManager.GetAllSpecialStructures())
            {
                TrySpawningAnAgent(specialStructure, placementManager.GetRandomHouseStructure());
            }
        }

        private void TrySpawningAnAgent(StructureModel startStructure, StructureModel endStructure)
        {
            if (startStructure != null && endStructure != null)
            {
                var startPosition = ((INeedingRoad)startStructure).RoadPosition;
                var endPosition = ((INeedingRoad)endStructure).RoadPosition;

                var startMarkerPosition = placementManager.GetStructureAt(startPosition).GetPedestrianSpawnMarker(startStructure.transform.position);
                var endMarkerPosition = placementManager.GetStructureAt(endPosition).GetNearestPedestrianMarkerTo(endStructure.transform.position);

                var agent = Instantiate(GetRandomPedestrian(), startMarkerPosition.Position, Quaternion.identity);
                var path = placementManager.GetPathBetween(startPosition, endPosition, true);
                if (path.Count > 0)
                {
                    path.Reverse();
                    List<Vector3> agentPath = GetPedestrianPath(path, startMarkerPosition.Position, endMarkerPosition);
                    var aiAgent = agent.GetComponent<AiAgent>();
                    aiAgent.Initialize(agentPath);
                }
            }
        }

        private void Awake()
        {
            if (buildingGrid == null)
                buildingGrid = FindObjectOfType<BuildingGrid>();
        }

        private void Start()
        {
            ScheduleNextCarSpawn();
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
            if (path == null || path.Count < 3)
                return false;

            path.Reverse();

            var startMarkerPosition = placementManager.GetStructureAt(startRoadPosition).GetCarSpawnMarker(path[1]);
            var endMarkerPosition = placementManager.GetStructureAt(endRoadPosition).GetCarEndMarker(path[path.Count - 2]);
            var markerCarPath = GetRoadMarkerCarPath(startRoadPosition, endRoadPosition);

            if (markerCarPath.Count >= 2)
            {
                carPath = new List<Vector3> { startMarkerPosition.Position };
                if (markerCarPath.Count > 0)
                {
                    if (markerCarPath[0] != carPath[0])
                        carPath.AddRange(markerCarPath);
                    else
                        carPath.AddRange(markerCarPath.Skip(1));
                }

                if (carPath.Count == 0 || carPath[carPath.Count - 1] != endMarkerPosition.Position)
                    carPath.Add(endMarkerPosition.Position);

                var car = Instantiate(carPrefab, startMarkerPosition.Position, Quaternion.identity);
                var carAi = car.GetComponent<CarAI>();
                carAi.SetPath(carPath);
                activeCars.Add(carAi);
                return true;
            }

            return false;
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
            Dictionary<Marker, Vector3> tempDictionary = new Dictionary<Marker, Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(currentPosition);
                var markersList = roadStructure.GetPedestrianMarkers();
                bool limitDistance = markersList.Count == 4;
                tempDictionary.Clear();
                foreach (var marker in markersList)
                {
                    pedestrianGraph.AddVertex(marker.Position);
                    foreach (var markerNeighbourPosition in marker.GetAdjacentPositions())
                    {
                        pedestrianGraph.AddEdge(marker.Position, markerNeighbourPosition);
                    }

                    if (marker.OpenForconnections && i + 1 < path.Count)
                    {
                        var nextRoadStructure = placementManager.GetStructureAt(path[i + 1]);
                        if (limitDistance)
                        {
                            tempDictionary.Add(marker, nextRoadStructure.GetNearestPedestrianMarkerTo(marker.Position));
                        }
                        else
                        {
                            pedestrianGraph.AddEdge(marker.Position, nextRoadStructure.GetNearestPedestrianMarkerTo(marker.Position));
                        }
                    }
                }
                if (limitDistance && tempDictionary.Count == 4)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    for (int j = 0; j < 2; j++)
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
            Dictionary<Marker, Vector3> tempDictionary = new Dictionary<Marker, Vector3>();
            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(currentPosition);
                var markersList = roadStructure.GetCarMarkers();
                var limitDistance = markersList.Count > 3;
                tempDictionary.Clear();

                foreach (var marker in markersList)
                {
                    carGraph.AddVertex(marker.Position);
                    foreach (var markerNeighbour in marker.adjacentMarkers)
                    {
                        carGraph.AddEdge(marker.Position, markerNeighbour.Position);
                    }
                    if (marker.OpenForconnections && i + 1 < path.Count)
                    {
                        var nextRoadPosition = placementManager.GetStructureAt(path[i + 1]);
                        if (limitDistance)
                        {
                            tempDictionary.Add(marker, nextRoadPosition.GetNearestCarMarkerTo(marker.Position));
                        }
                        else
                        {
                            carGraph.AddEdge(marker.Position, nextRoadPosition.GetNearestCarMarkerTo(marker.Position));
                        }
                    }
                }
                if (limitDistance && tempDictionary.Count > 2)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    foreach (var item in distanceSortedMarkers)
                    {
                        Debug.Log(Vector3.Distance(item.Key.Position, item.Value));
                    }
                    for (int j = 0; j < 2; j++)
                    {
                        carGraph.AddEdge(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value);
                    }
                }
            }
        }

        private GameObject GetRandomPedestrian()
        {
            return pedestrianPrefabs[UnityEngine.Random.Range(0, pedestrianPrefabs.Length)];
        }

        private void Update()
        {
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

        private bool IsRushHour()
        {
            var hour = System.DateTime.Now.Hour;
            return hour >= rushHourStart && hour < rushHourEnd;
        }

        private void CleanupInactiveCars()
        {
            activeCars.RemoveAll(car => car == null);
        }

        private void DrawGraph(AdjacencyGraph graph)
        {
            foreach (var vertex in graph.GetVertices())
            {
                foreach (var vertexNeighbour in graph.GetConnectedVerticesTo(vertex))
                {
                    Debug.DrawLine(vertex.Position + Vector3.up, vertexNeighbour.Position + Vector3.up, Color.red);
                }
            }
        }


    }
}

