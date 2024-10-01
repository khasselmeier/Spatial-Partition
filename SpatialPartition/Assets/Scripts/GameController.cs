using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace SpatialPartitionPattern
{
    public class GameController : MonoBehaviour
    {
        public GameObject friendlyObj;
        public GameObject enemyObj;

        public TMP_Text timeSpentText;
        public TMP_Text spatialPartitionStatusText;
        /*public TMP_InputField soldierInputField;
        public Button spawnButton;
        public TMP_Text spawnButtonText;*/

        //Change materials to detect which enemy is the closest
        public Material enemyMaterial;
        public Material closestEnemyMaterial;

        //To get a cleaner workspace, parent all soldiers to these empty gameobjects
        public Transform enemyParent;
        public Transform friendlyParent;

        //Store all soldiers in these lists
        List<Soldier> enemySoldiers = new List<Soldier>();
        List<Soldier> friendlySoldiers = new List<Soldier>();

        //Save the closest enemies to easier change back its material
        List<Soldier> closestEnemies = new List<Soldier>();

        //Grid data
        float mapWidth = 50f;
        int cellSize = 10;

        //Number of soldiers on each team
        int numberOfSoldiers = 300;

        //The Spatial Partition grid
        Grid grid;

        float updateTimeSpent = 0f;
        bool isSpatialPartitionEnabled = true;

        void Start()
        {
            //Create a new grid
            grid = new Grid((int)mapWidth, cellSize);

            //Add random enemies and friendly and store them in a list
            for (int i = 0; i < numberOfSoldiers; i++)
            {
                //Give the enemy a random position
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                //Create a new enemy
                GameObject newEnemy = Instantiate(enemyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the enemy to a list
                enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));

                //Parent it
                newEnemy.transform.parent = enemyParent;


                //Give the friendly a random position
                randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                //Create a new friendly
                GameObject newFriendly = Instantiate(friendlyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the friendly to a list
                friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));

                //Parent it 
                newFriendly.transform.parent = friendlyParent;
            }

            UpdateSpatialPartitionStatusText();
        }

        void Update()
        {
            // Toggle spatial partitioning on/off when the spacebar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isSpatialPartitionEnabled = !isSpatialPartitionEnabled;
                UpdateSpatialPartitionStatusText();  // Update UI to reflect toggle status
            }

            // Start measuring time
            float startTime = Time.realtimeSinceStartup;

            // Move the enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                enemySoldiers[i].Move();
            }

            // Reset material of the closest enemies
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                closestEnemies[i].soldierMeshRenderer.material = enemyMaterial;
            }

            closestEnemies.Clear();

            // For each friendly, find the closest enemy and change its color and chase it
            for (int i = 0; i < friendlySoldiers.Count; i++)
            {
                Soldier closestEnemy;

                // Use spatial partitioning if enabled, otherwise use the slow method
                if (isSpatialPartitionEnabled)
                {
                    // Find closest enemy using grid (spatial partitioning)
                    closestEnemy = grid.FindClosestEnemy(friendlySoldiers[i]);
                }
                else
                {
                    // Use slow method to find closest enemy without spatial partitioning
                    closestEnemy = FindClosestEnemySlow(friendlySoldiers[i]);
                }

                // If we found an enemy, chase it
                if (closestEnemy != null)
                {
                    closestEnemy.soldierMeshRenderer.material = closestEnemyMaterial;
                    closestEnemies.Add(closestEnemy);
                    friendlySoldiers[i].Move(closestEnemy);  // Friendly chases the closest enemy
                }
            }

            // Calculate time spent in Update
            float endTime = Time.realtimeSinceStartup;
            updateTimeSpent = endTime - startTime;

            // Display the time spent in Update
            if (timeSpentText != null)
            {
                timeSpentText.text = "Time spent in Update: " + updateTimeSpent.ToString("F6") + " seconds";
            }
        }

        //Find the closest enemy - slow version
        Soldier FindClosestEnemySlow(Soldier soldier)
        {
            Soldier closestEnemy = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop thorugh all enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = (soldier.soldierTrans.position - enemySoldiers[i].soldierTrans.position).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestEnemy = enemySoldiers[i];
                }
            }

            return closestEnemy;
        }

        void UpdateSpatialPartitionStatusText()
        {
            if (spatialPartitionStatusText != null)
            {
                spatialPartitionStatusText.text = "Spatial Partition: " + (isSpatialPartitionEnabled ? "Enabled" : "Disabled");
            }
        }
    }
}