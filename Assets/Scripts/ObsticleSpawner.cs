using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsticleSpawner : MonoBehaviour
{
    public GameObject[] Obsticles1;
    public GameObject[] Obsticles2;
    public GameObject[] Obsticles4;
    public GameObject[] Bonuses;
    
    public GameObject GetObsticle(int size)
    {
        switch (size)
        {
            case 1:
                return Instantiate(Obsticles1[Random.Range(0, Obsticles1.Length)]);
            case 2:
                return Instantiate(Obsticles2[Random.Range(0, Obsticles2.Length)]);
            case 4:
                return Instantiate(Obsticles4[Random.Range(0, Obsticles4.Length)]);
        }

        return null;
    }

    public void SpawnObsticle(Vector3 localPos, Transform parent, int size)
    {
        var go = GetObsticle(size);

        if (go == null)
        {
            return;
        }
        
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
    }
    
    public void SetupObsticles(RoadSegment segment, int maxObjects)
    {
        var numberOfObjects = Random.Range(0, maxObjects + 1);
        var interval = (GameManager.RT.ScreenExtentsY * 2.0f) / numberOfObjects;
        var height = interval;

        while (height < GameManager.RT.ScreenExtentsY * 2.0f)
        {
            // Random for lanes
            int rndLane;

            // Random to determine size of obsticle
            int rndSize = Random.Range(0, 10);

            if (rndSize == 0)
            {
                // 4 piece obsticle
                SpawnObsticle(
                    new Vector3(GameManager.RT.GetLaneX(0), height, 0.0f),
                    segment.ObstacleParent,
                    4);
            }
            else if (rndSize < 5)
            {
                // 2 piece obsticle
                rndLane = Random.Range(0, GameManager.RT.NumberOfLanes - 1);
                SpawnObsticle(
                    new Vector3(GameManager.RT.GetLaneX(rndLane), height, 0.0f),
                    segment.ObstacleParent,
                    2);
            }
            else
            {
                rndLane = Random.Range(0, GameManager.RT.NumberOfLanes);
                SpawnObsticle(
                    new Vector3(GameManager.RT.GetLaneX(rndLane), height, 0.0f),
                    segment.ObstacleParent,
                    1);
            }
            
            height += interval;
        }
    }

    public void SpawnBonus(Vector3 localPos, Transform parent)
    {
        var go = Instantiate(Bonuses[Random.Range(0, Bonuses.Length)]);
        
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
    }
}
