using UnityEngine;

public class LapManager : MonoBehaviour
{
    private int cooldown;
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
        if (cooldown > 0)
        {
            cooldown -= 1;
        }
    }

public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish Line"))
        {
            if (raceData == null)
            {
                Debug.LogError("LapManager requires a RaceManager with RaceData.");
                return;
            }

            if (cooldown > 0)
            {
                return;
            }

            Debug.Log("Lap Incremented!");
            Debug.Log($"Lap Number: {raceData.currentLap}");
            Debug.Log($"Checkpoint trigger: {other.name} | {other.transform.root.name}");

            raceData.currentLap += 1;
            cooldown = 100;
        }
    }
}