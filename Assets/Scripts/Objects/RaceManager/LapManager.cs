using UnityEngine;

public class LapManager : MonoBehaviour
{
    private int cooldown;
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
        if(other.CompareTag("Finish Line"))
        {
            if(cooldown >= 0)
            {
                Debug.Log("Lap Incremented!");
                raceData.currentLap += 1;   
            }
        }
	}
}
