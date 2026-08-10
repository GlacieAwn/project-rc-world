using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Vector3 previousCheckpointPos;
    private RaceData raceData;

    void Awake()
    {
        raceData = new RaceData();
    }

	public void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Checkpoint"))
        {
            Debug.Log("Checkpoint Passed!");
            raceData.currentCheckpoint += 1;
            previousCheckpointPos = other.transform.position;
        }
	}
}
