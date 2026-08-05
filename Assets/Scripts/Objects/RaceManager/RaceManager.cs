using System;
using TMPro;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    /// <summary>
    /// Coordinates the overall race lifecycle.
    /// Transitions between race states and notifies other systems.
    /// </summary>


    [SerializeField] private RaceState raceState;
    [SerializeField] private TMP_Text debugText;


    public enum RaceState
    {
        Waiting,
        Countdown,
        Racing,
        Finished
    }
    // public RaceState currentState => raceState;

    public Action OnCountdownStarted;
    public Action OnRaceStarted;
    public Action OnRaceFinished;
    public Action<RaceState, RaceState> OnRaceStateChanged;


	void Awake()
	{
		raceState = RaceState.Waiting;
	}

    void Update()
    {
        UpdateDebugText();
    }

    private void BeginCountdown()
    {
        
    }

    private bool StartRace()
    {   
        return true;
    }

    private bool FinishRace()
    {   
        return true;
    }

    private bool SetState(RaceState newState)
    {   
        return true;
    }

    private void UpdateDebugText()
    {
        debugText.text =
        "Race Manager Values:\n" +
        $"currentState: {raceState}";
    }
}
