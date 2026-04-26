using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayerController;
using System.Collections;
using Cutscenes;

public class StoryIntro : CutsceneBase
{
    [Header("UI Elements")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private TextMeshProUGUI storyText;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float textDisplayDuration = 3f;
    [SerializeField] private float textDelay = 0.5f;

    [TextArea(3, 6)]
    [SerializeField]
    private string message;

    // For testing purposes, allow skipping the intro
    [Header("Debug")]
    [SerializeField] private bool skipIntro = false;

    private void Start()
    {
        if(fadeOverlay == null || storyText == null) {
            Debug.LogError("Missing UI elements for StoryIntro!");
            return;
        }

        if (skipIntro)
        {
            // Instantly disable everything
            fadeOverlay.color = new Color(0, 0, 0, 0);
            storyText.text = "";
            fadeOverlay.raycastTarget = false;
            
            return;
        }
        
        Play();
    }

    protected override void OnStartCutscene() 
    {
        StartCoroutine(PlayIntroSequence());
    }

    // Plays the intro sequence with fade-in, text display, and fade-out
    private IEnumerator PlayIntroSequence() 
    {

        Color overlayColor = fadeOverlay.color;
        overlayColor.a = 1f;
        fadeOverlay.color = overlayColor;

        Color textColor = storyText.color;
        textColor.a = 0f;
        storyText.color = textColor;
        storyText.text = "";

        yield return new WaitForSeconds(textDelay);

        storyText.text = message;
        textColor.a = 1f;
        storyText.color = textColor;

        yield return new WaitForSeconds(textDisplayDuration);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            overlayColor.a = Mathf.Lerp(1f, 0f, t);
            textColor.a = Mathf.Lerp(1f, 0f, t);

            fadeOverlay.color = overlayColor;
            storyText.color = textColor;

            yield return null;
        }

        overlayColor.a = 0f;
        textColor.a = 0f;

        fadeOverlay.color = overlayColor;
        storyText.color = textColor;

        fadeOverlay.raycastTarget = false;

        EndCutscene();
    }


}
