using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    /// <summary>
    /// Coordinates the overall race lifecycle.
    /// Transitions between race states and notifies other systems.
    /// </summary>


    [SerializeField] private RaceState raceState;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private CarMovement playerMovement;


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

    private TimeSpan time;
    private RaceData raceData;

    void Start()
    {
        SetState(RaceState.Countdown);
    }

	void Awake()
	{
		OnRaceStateChanged += HandleRaceStateChanged;

		if (playerMovement == null)
		{
			playerMovement = FindAnyObjectByType<CarMovement>();
		}

        raceData = new RaceData();

	}

    void Update()
    {
        time = TimeSpan.FromSeconds(raceData.currentTime);
        

        if(raceState == RaceState.Racing)
        {
            raceData.currentTime += Time.deltaTime;
        }

        UpdateDebugText();
    }

    private void BeginCountdown()
    {
        StartCoroutine(countdown());
    }

    private bool StartRace()
    {
        raceData.currentTime = 0f;
        return true;
    }

    private bool FinishRace()
    {   
        return true;
    }

    private void SetState(RaceState newState)
    {
        if (raceState == newState)
            return;

        RaceState previous = raceState;
        raceState = newState;

        OnRaceStateChanged?.Invoke(previous, newState);
    }

    private void HandleRaceStateChanged(RaceState previous, RaceState current)
    {
        switch (current)
        {
            case RaceState.Countdown:
                BeginCountdown();
                SetPlayerMovementEnabled(false);
                break;

            case RaceState.Racing:
                StartRace();
                SetPlayerMovementEnabled(true);
                break;

            case RaceState.Finished:
                FinishRace();
                SetPlayerMovementEnabled(false);
                // TODO: take control from the player and follow the spline.
                break;
        }
    }

    private void SetPlayerMovementEnabled(bool enabled)
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.SetMovementEnabled(enabled);
    }

    private void UpdateDebugText()
    {
        int hundredths = time.Milliseconds / 10;

        debugText.text =
        "Race Manager Values:\n" +
        $"currentState: {raceState}\n" +
        $"currentLap: {raceData.currentLap}\n" +
        $"currentCheckpoint: {raceData.currentCheckpoint}\n" + 
        $"currentPlace: {raceData.currentPlace}\n" +
        $"finished: {raceData.finished}\n" +
        $"currentTime: {time.Minutes:00}:{time.Seconds:00}.{hundredths:00}\n";
    }

    IEnumerator countdown()
    {
        yield return new WaitForSeconds(3f);
        SetState(RaceState.Racing);
    }
}
