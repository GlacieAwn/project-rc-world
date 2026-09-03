using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public int currentCheckpoint;
    private int cooldown;
    private Vector3 previousCheckpointPos;
    private RaceData raceData;

    void Awake()
    {
        raceData = new RaceData();
        cooldown = 20;
    }

	void Update()
	{
		cooldown -= 1;
        if(cooldown <= 0)
        {
            cooldown = 20;
        }
	}

	public void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Checkpoint"))
        {
            Debug.Log("Checkpoint Passed!");
            Debug.Log("CurrentCheckpoint: " + currentCheckpoint);
            Debug.Log("CurrentRaceDataCheckpoint: " + raceData.currentCheckpoint);

            if(cooldown >= 0)
            {
                currentCheckpoint += 1;
                raceData.currentCheckpoint += 1;
            }

            previousCheckpointPos = other.transform.position;
        }
	}
}
