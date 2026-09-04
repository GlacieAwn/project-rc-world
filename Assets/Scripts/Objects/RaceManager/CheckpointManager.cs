using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public int currentCheckpoint;
    private int cooldown;
    private Vector3 previousCheckpointPos;
    private RaceData raceData;
    [SerializeField] private RaceManager raceManager;

    void Awake()
    {
        cooldown = 100;
    }

    void Start()
    {
        if (raceManager == null)
        {
            raceManager = FindAnyObjectByType<RaceManager>();
        }

        raceData = raceManager != null ? raceManager.RaceData : null;
    }

	void Update()
	{
		cooldown -= 1;
        if(cooldown <= 0)
        {
            cooldown = 100;
        }
	}

	public void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Checkpoint"))
        {
            if (raceData == null)
            {
                Debug.LogError("CheckpointManager requires a RaceManager with RaceData.");
                return;
            }

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
