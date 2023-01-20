using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground_Behaviour : MonoBehaviour
{
    public int numberOfUAVs = 10; // The number of UAVs to be spawned
    public float uavRange = 5f; // The range at which the UAVs can communicate
    public float uavSpeed = 5f; // The speed at which the UAVs move
    public float areaSize = 20f; // The size of the area in which the UAVs will be spawned
    public int areaBetweenUavs = 2; // The minimum distance between the UAVs when they are spawned
    public float delayBetweenRounds = 1f; // The delay between communication rounds

    public GameObject uavPrefab;
    private List<UAV_Behaviour> uavs = new List<UAV_Behaviour>();

    void Start()
    {
        // Instantiate the UAVs and add them to the list
        for (int i = 0; i < numberOfUAVs; i++)
        {
            UAV_Behaviour newUAV = Instantiate(uavPrefab).GetComponent<UAV_Behaviour>();
            uavs.Add(newUAV);
        }

        // Set the properties of the UAVs and randomly position them within the area
        for(int i = 0; i < numberOfUAVs; i++)
        {
            UAV_Behaviour uav = uavs[i];
            Vector3 newPosition;
            do
            {
                newPosition = new Vector3(transform.position.x + Random.Range(-areaSize, areaSize), 0, transform.position.z + Random.Range(-areaSize, areaSize));
            }
            while (IsTooCloseToOtherUAVs(newPosition, i));

            uav.gameObject.name = "UAV " + i;
            uav.ID = i;
            uav.speed = uavSpeed;
            uav.range = uavRange;
            uav.delayBetweenRounds = delayBetweenRounds;
            uav.transform.position = newPosition;    
        }

        // Start the communication process for each UAV
        foreach(UAV_Behaviour uav in uavs)
        {
            uav.Lift();
        }

    }

    // Check if the new position for a UAV is too close to other UAVs
    bool IsTooCloseToOtherUAVs(Vector3 newPosition, int currentUAVIndex)
    {
        for (int i = 0; i < numberOfUAVs; i++)
        {
            if (i != currentUAVIndex)
            {
                UAV_Behaviour uav = uavs[i];
                if (Vector3.Distance(newPosition, uav.transform.position) < areaBetweenUavs)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
