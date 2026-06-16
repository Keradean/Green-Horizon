using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Furkan
{
    public class CarSpawner : MonoBehaviour
    { 
        /// <summary>
        /// Spawns a random car prefab as a child of this GameObject when the scene starts.
        /// Useful for placing random cars in the scene for variety or testing.
        /// </summary>
        // Array of car prefabs to choose from when spawning 
        
        public GameObject[] carPrefabs;

        /// <summary>
        /// Called on scene start. Instantiates a random car prefab as a child of this spawner.
        /// </summary>
        private void Start()
        {
            Instantiate(SelectACarPrefab(), transform);
        }

        /// <summary>
        /// Selects a random car prefab from the array.
        /// </summary>
        private GameObject SelectACarPrefab()
        {
            var randomIndex = Random.Range(0, carPrefabs.Length);
            return carPrefabs[randomIndex];
        }
    }
}
