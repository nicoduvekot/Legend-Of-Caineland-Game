//***************************************************************************************
// Writer: Stylish Esper (Modified for Coroutine Safety)
//***************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Esper.Freeloader
{
    /// <summary>
    /// A screen shown to the player while the game is loading.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public LoadingScreenSettings settings;
        protected UIDocument document;
        protected Label tipsTitleLabel;
        protected Label tipLabel;
        protected VisualElement tipsContainer;
        protected VisualElement loadingBar;
        protected Label loadingLabel;
        protected VisualElement loadingIconContainer;
        protected VisualElement spinnerIcon;
        protected VisualElement root;
        protected VisualElement loadingContent;
        protected Label continueLabel;
        protected VisualElement backgroundOne;
        protected VisualElement backgroundTwo;
        protected IVisualElementScheduledItem spinnerAction;

        private bool _isDestroyed = false;

        protected int currentTipIndex = -1;
        protected int currentProcessIndex = 0;
        protected int currentBackgroundIndex = 0;
        protected LoadingProgressTracker[] processes;

        [Space(5)] public UnityEvent onStart = new();
        [Space(5)] public UnityEvent<float> onProgressChanged = new();
        [Space(5)] public UnityEvent onComplete = new();
        [Space(5)] public UnityEvent onClose = new();

        public float Progress { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsOpen { get => root != null && root.enabledSelf; }
        public static LoadingScreen Instance { get; private set; }

        // Track the active loading routine to prevent ghosting
        private Coroutine activeLoadingRoutine;

        protected virtual void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                _isDestroyed = true; // Mark as a duplicate
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            document = GetComponent<UIDocument>();
            tipsTitleLabel = document.rootVisualElement.Q<Label>("TipsTitleLabel");
            tipLabel = document.rootVisualElement.Q<Label>("TipLabel");
            tipsContainer = document.rootVisualElement.Q<VisualElement>("TipsContainer");
            loadingBar = document.rootVisualElement.Q<VisualElement>("LoadingBar");
            root = document.rootVisualElement.Q<VisualElement>("Root");
            loadingLabel = document.rootVisualElement.Q<Label>("LoadingLabel");
            loadingIconContainer = document.rootVisualElement.Q<VisualElement>("LoadingIconContainer");
            spinnerIcon = document.rootVisualElement.Q<VisualElement>("SpinnerIcon");
            loadingContent = document.rootVisualElement.Q<VisualElement>("LoadingContent");
            continueLabel = document.rootVisualElement.Q<Label>("ContinueLabel");
            backgroundOne = document.rootVisualElement.Q<VisualElement>("BackgroundOne");
            backgroundTwo = document.rootVisualElement.Q<VisualElement>("BackgroundTwo");

            RefreshElementStates();
        }

        private void Start()
        {
            if (_isDestroyed) return;
            Close(false);
        }

        protected virtual void RefreshElementStates()
        {
            tipsTitleLabel.text = settings.tipsTitle;
            tipsContainer.style.display = settings.showTips ? DisplayStyle.Flex : DisplayStyle.None;
            loadingBar.style.display = settings.hideBar ? DisplayStyle.None : DisplayStyle.Flex;
            loadingIconContainer.style.display = settings.showSpinner ? DisplayStyle.Flex : DisplayStyle.None;
            spinnerIcon.style.backgroundImage = settings.spinnerIcon;
        }

        public virtual void Load(string sceneName = "", params LoadingProgressTracker[] processes)
        {
            if (IsLoading)
            {
                Debug.LogWarning("Freeloader: cannot load a scene while loading is ongoing.");
                return;
            }
            else if (string.IsNullOrEmpty(sceneName) && processes.Length == 0)
            {
                Debug.LogWarning("Freeloader: nothing to load.");
                return;
            }

            Open();
            activeLoadingRoutine = StartCoroutine(LoadingCoroutine(sceneName, processes));
        }

        protected IEnumerator LoadingCoroutine(string sceneName, params LoadingProgressTracker[] additionalProcesses)
        {
            yield return new WaitForSeconds(0.5f);
            IsLoading = true;
            onStart.Invoke();

            List<LoadingProgressTracker> allProcesses = new List<LoadingProgressTracker>();
            AsyncOperation sceneOperation = null;

            if (!string.IsNullOrEmpty(sceneName))
            {
                sceneOperation = SceneManager.LoadSceneAsync(sceneName);
                sceneOperation.allowSceneActivation = false;

                allProcesses.Add(new LoadingProgressTracker(
                    settings.defaultLoadingText,
                    () => Mathf.Clamp01(sceneOperation.progress / 0.9f) * 100f
                ));
            }

            if (additionalProcesses != null) allProcesses.AddRange(additionalProcesses);
            processes = allProcesses.ToArray();

            for (int i = 0; i < processes.Length; i++)
            {
                currentProcessIndex = i;
                var process = processes[i];
                float previousProgress = -1f;

                yield return new WaitUntil(() =>
                {
                    if (!IsOpen) return true; // Safety break if screen closed early

                    Progress = (process.Progress + (100 * i)) / (100 * processes.Length) * 100f;
                    if (!Mathf.Approximately(previousProgress, Progress))
                    {
                        previousProgress = Progress;
                        onProgressChanged.Invoke(Progress);
                        UpdateLoadingBar();
                    }
                    return process.Progress >= 100f;
                });

                if (!IsOpen) yield break;
            }

            if (sceneOperation != null && IsOpen)
            {
                while (sceneOperation.progress < 0.9f) yield return null;
                Progress = 100f;
                onProgressChanged.Invoke(Progress);
                UpdateLoadingBar();

                sceneOperation.allowSceneActivation = true;
                yield return new WaitUntil(() => sceneOperation.isDone);
            }

            IsLoading = false;
            onComplete.Invoke();
            activeLoadingRoutine = null;

            if (settings.requireInputToContinue && IsOpen)
                ShowContinueContent();
            else
                Close();
        }

        public virtual void Close(bool invokeEvent = true)
        {
            // If this object is a duplicate being destroyed, don't do anything
            if (_isDestroyed) return;

            // KILL the current loading logic immediately
            // Only stop the routine if this specific script instance is still "alive"
            if (activeLoadingRoutine != null && this != null)
            {
                StopCoroutine(activeLoadingRoutine);
                activeLoadingRoutine = null;
            }

            IsLoading = false;
            loadingLabel.UnregisterCallback<TransitionEndEvent>(ToggleLoadingLabelClass);
            continueLabel.UnregisterCallback<TransitionEndEvent>(ToggleContinueLabelClass);
            backgroundOne.UnregisterCallback<TransitionEndEvent>(ToggleBackgroundOneScaleClass);
            backgroundTwo.UnregisterCallback<TransitionEndEvent>(ToggleBackgroundTwoScaleClass);

            if (spinnerAction != null)
            {
                spinnerAction.Pause();
                spinnerAction = null;
            }

            continueLabel.style.display = DisplayStyle.None;
            root.SetEnabled(false);
            if (invokeEvent) onClose.Invoke();
            SetPickingModeRecursively(root, PickingMode.Ignore);
        }

        public virtual void Open()
        {
            SetPickingModeRecursively(root, PickingMode.Position);
            loadingLabel.RegisterCallback<TransitionEndEvent>(ToggleLoadingLabelClass);

            if (settings.enableBackgroundZoom)
            {
                backgroundOne.RegisterCallback<TransitionEndEvent>(ToggleBackgroundOneScaleClass);
                backgroundTwo.RegisterCallback<TransitionEndEvent>(ToggleBackgroundTwoScaleClass);
            }

            root.SetEnabled(true);
            BeginBackgroundSlideshow();
            BeginTipSlideshow();
            UpdateLoadingBar();

            if (settings.showSpinner)
            {
                float speed = -12 * settings.spinnerSpeed;
                spinnerAction = spinnerIcon.schedule.Execute(() => UpdateSpinner(speed)).Every(17);
            }

            ToggleLoadingLabelClass(null);
            if (loadingContent.ClassListContains("fadeOut"))
                loadingContent.RemoveFromClassList("fadeOut");
        }

        // --- Helper Methods (Keep as originally defined) ---
        protected virtual void UpdateSpinner(float changeValue) =>
            spinnerIcon.style.rotate = new Rotate(new Angle(spinnerIcon.style.rotate.value.angle.value + changeValue));

        protected virtual void UpdateLoadingBar()
        {
            loadingBar.style.width = new Length(Progress, LengthUnit.Percent);
            if (processes != null && processes.Length > 0)
            {
                string percentText = !settings.showPercentage ? string.Empty : $" {Progress:0.##}%";
                loadingLabel.text = $"{processes[currentProcessIndex].displayText}{percentText}";
            }
            else loadingLabel.text = settings.defaultLoadingText;
        }

        protected virtual void ShowContinueContent()
        {
            continueLabel.text = settings.continueText;
            continueLabel.style.display = DisplayStyle.Flex;
            continueLabel.RegisterCallback<TransitionEndEvent>(ToggleContinueLabelClass);
            loadingContent.AddToClassList("fadeOut");
            if (spinnerAction != null) { spinnerAction.Pause(); spinnerAction = null; }
            InputSystem.onAnyButtonPress.CallOnce(x => Close());
        }

        protected virtual void NextTip()
        {
            currentTipIndex++;
            if (currentTipIndex >= settings.tips.Count) currentTipIndex = 0;
            tipLabel.text = settings.tips[currentTipIndex];
        }

        protected virtual void BeginBackgroundSlideshow()
        {
            if (settings.backgrounds.Count == 0) return;
            currentBackgroundIndex = 0;
            backgroundOne.SetEnabled(true);
            backgroundOne.style.backgroundImage = settings.backgrounds[currentBackgroundIndex];
            if (settings.backgrounds.Count > 1) StartCoroutine(BackgroundSlideshowLoopCoroutine());
        }

        protected IEnumerator BackgroundSlideshowLoopCoroutine()
        {
            currentBackgroundIndex = (currentBackgroundIndex + 1) % settings.backgrounds.Count;
            yield return new WaitForSeconds(0.5f);
            if (backgroundOne.enabledSelf) backgroundTwo.style.backgroundImage = settings.backgrounds[currentBackgroundIndex];
            else backgroundOne.style.backgroundImage = settings.backgrounds[currentBackgroundIndex];
            yield return new WaitForSeconds(settings.backgroundDisplayLength);
            backgroundOne.SetEnabled(!backgroundOne.enabledSelf);
            if (IsOpen) StartCoroutine(BackgroundSlideshowLoopCoroutine());
        }

        protected virtual void BeginTipSlideshow()
        {
            if (settings.tips.Count == 0) return;
            if (settings.tips.Count > 1) StartCoroutine(TipSlideshowLoopCoroutine());
            else tipLabel.text = settings.tips[0].ToString();
        }

        protected IEnumerator TipSlideshowLoopCoroutine()
        {
            yield return new WaitForSeconds(tipLabel.ClassListContains(settings.tipAnimatorPseudoClassName) ? 1f : settings.tipDisplayLength);
            if (IsOpen) { ToggleTipClassAndDisplayNextTip(); StartCoroutine(TipSlideshowLoopCoroutine()); }
        }

        protected virtual void ToggleTipClassAndDisplayNextTip() { ToggleTipClass(); if (!tipLabel.ClassListContains(settings.tipAnimatorPseudoClassName)) NextTip(); }
        protected virtual void ToggleTipClass() => tipLabel.ToggleInClassList(settings.tipAnimatorPseudoClassName);
        protected virtual void ToggleLoadingLabelClass(TransitionEndEvent e) => loadingLabel.ToggleInClassList(settings.loadingLabelAnimatorPseudoClassName);
        protected virtual void ToggleContinueLabelClass(TransitionEndEvent e) => continueLabel.ToggleInClassList(settings.continueLabelAnimatorPseudoClassName);
        protected virtual void ToggleBackgroundOneScaleClass(TransitionEndEvent e) => backgroundOne.ToggleInClassList(settings.backgroundScaleAnimatorPseudoClassName);
        protected virtual void ToggleBackgroundTwoScaleClass(TransitionEndEvent e) => backgroundTwo.ToggleInClassList(settings.backgroundScaleAnimatorPseudoClassName);
        private void SetPickingModeRecursively(VisualElement e, PickingMode m) { e.pickingMode = m; foreach (var child in e.hierarchy.Children()) SetPickingModeRecursively(child, m); }
        public virtual void Load(int b, params LoadingProgressTracker[] p) => Load(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(b)), p);
    }
}