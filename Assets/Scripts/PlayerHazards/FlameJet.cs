using UnityEngine;
/*
 * This script controls the behavior of a flame jet hazard in the game. 
 * The flame jet will alternate between being active (emitting flames) and inactive (not emitting flames) based on time durations
 * The script manages the visual effect of the flame and its collider to ensure that it only damages the player when active.
 */
public class FlameJet : MonoBehaviour
{
    [Header("FlameJet References")]
    [SerializeField] private GameObject flameEffect;
    [SerializeField] private BoxCollider2D flameCollider;

    [Header("FlameJet Settings")]
    [SerializeField] private float offDuration = 2f; // Time the flame is off
    [SerializeField] private float onDuration = 1.5f; // Time the flame is on
    [SerializeField] private bool startActive = false; // flag set to true when flame is active

    private float _timer; // internal timer to track timing
    private bool _isActive = false; // internal state to track if the flame is currently active or not

    // Initializes the flame state based on the startActive flag and sets the timer accordingly.
    private void Start()
    {
        _isActive = startActive;
        SetFlameState(startActive);
    }

    // Updates the timer and toggles the flame state when the timer reaches zero, then resets the timer based on the new state.
    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer > 0f) return;

        _isActive = !_isActive;
        SetFlameState(_isActive);
        _timer = _isActive ? onDuration : offDuration;
    }

    // Sets the flame's active state by enabling/disabling the flame effect and collider based on the provided active parameter.
    private void SetFlameState(bool active)
    {
        _isActive = active;

        // toggles both the flame effect and collider
        if(flameEffect != null)
            flameEffect.SetActive(active);
        if(flameCollider != null)
            flameCollider.enabled = active;
    }

}
