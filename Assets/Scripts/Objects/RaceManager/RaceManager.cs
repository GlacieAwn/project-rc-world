using System;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    /// <summary>
    /// Coordinates the overall race lifecycle.
    /// Transitions between race states and notifies other systems.
    /// </summary>
    [SerializeField] private RaceState raceState;
    
    public enum RaceState
    {
        Waiting,
        Countdown,
        Racing,
        Finished
    }

    public Action OnCountdownStarted;
    public Action OnRaceStarted;
    public Action OnRaceFinished;
    public Action<RaceState, RaceState> OnRaceStateChanged;


	void Awake()
	{
		raceState = RaceState.Waiting;
	}

    private void BeginCountdown()
    {
        
    }

    private bool RaceStarted()
    {   
        return true;
    }

    private bool RaceFinished()
    {   
        return true;
    }

    private bool SetState(RaceState newState)
    {   
        return true;
    }
}
