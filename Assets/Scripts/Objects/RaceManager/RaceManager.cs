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
    [SerializeField] private float currentTime = 0f;


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

    void Start()
    {
        SetState(RaceState.Countdown);
    }

	void Awake()
	{
		OnRaceStateChanged += HandleRaceStateChanged;
	}

    void Update()
    {
        time = TimeSpan.FromSeconds(currentTime);
        

        if(raceState == RaceState.Racing)
        {
            currentTime += Time.deltaTime;
        }

        UpdateDebugText();
    }

    private void BeginCountdown()
    {
        StartCoroutine(countdown());
    }

    private bool StartRace()
    {
        currentTime = 0f;
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
                break;

            case RaceState.Racing:
                StartRace();
                break;

            case RaceState.Finished:
                FinishRace();
                break;
        }
    }

    private void UpdateDebugText()
    {
        int hundredths = time.Milliseconds / 10;

        debugText.text =
        "Race Manager Values:\n" +
        $"currentState: {raceState}\n" +
        $"currentTime:{time.Minutes:00}:{time.Seconds:00}.{hundredths:00}";
    }

    IEnumerator countdown()
    {
        yield return new WaitForSeconds(3f);
        SetState(RaceState.Racing);
    }
}
