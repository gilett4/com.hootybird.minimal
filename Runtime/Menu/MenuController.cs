using HootyBird.Minimal.Tween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem; // New Input System
#endif

namespace HootyBird.Minimal.Menu
{
    /// <summary>
    /// Menu controller. Handles menu overlays under it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class MenuController : MonoBehaviour
    {
        /// <summary>
        /// Active controller reference.
        /// </summary>
        public static MenuController ActiveMenuController { get; protected set; }

        /// <summary>
        /// All available controllers.
        /// </summary>
        public static readonly Dictionary<string, MenuController> Controllers = new Dictionary<string, MenuController>();

        [SerializeField] private bool state = false;
        [SerializeField] private bool sequentialTransition = false;

        protected TweenBase tween;
        protected GraphicRaycaster raycaster;
        protected Coroutine currentTransitionRoutine;

        public List<MenuOverlay> Overlays { get; protected set; }

#if UNITY_EDITOR
        [Space(10f)]
        [Header("Only exposed in the editor.")]
        [SerializeField] public List<MenuOverlay> OverlaysStack;
#else
        public List<MenuOverlay> OverlaysStack { get; protected set; }
#endif

        public MenuOverlay CurrentOverlay { get; protected set; }
        public bool Initialized { get; private set; }
        public bool State => state;

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnEnable()
        {
            // Reopen active overlay when this controller activated.
            if (CurrentOverlay)
            {
                CurrentOverlay.Open();
            }
        }

        protected virtual void OnDestroy()
        {
            if (tween != null)
            {
                tween._onProgress -= OnTweenProgress;
            }

            // Remove from Controllers map if present.
            if (Controllers.TryGetValue(name, out var controller) && controller == this)
            {
                Controllers.Remove(name);
            }

            // If we're the active controller, clear it.
            if (ActiveMenuController == this)
            {
                ActiveMenuController = null;
            }
        }

        /// <summary>
        /// Handles "android back"/"keyboard escape" button.
        /// </summary>
        protected virtual void Update()
        {
            bool backPressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // New Input System: Escape on Keyboard, Back on Mouse (XButton1), common gamepad back/start buttons.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                backPressed = true;
            else if (Mouse.current != null && Mouse.current.backButton.wasPressedThisFrame)
                backPressed = true;
            else if (Gamepad.current != null &&
                     (Gamepad.current.startButton.wasPressedThisFrame ||
                      Gamepad.current.selectButton.wasPressedThisFrame ||
                      Gamepad.current.bButton.wasPressedThisFrame)) // often mapped as 'back/cancel'
                backPressed = true;
#else
            // Old Input Manager
            backPressed = Input.GetKeyDown(KeyCode.Escape);
#endif

            if (state && OverlaysStack.Count > 0 && backPressed)
            {
                MenuOverlay overlay = OverlaysStack.Last();

                if (!overlay.Interactable) return;

                // Invoke OnBack on current screen.
                overlay.OnBack();
            }
        }

        public static T GetMenuController<T>(string name) where T : MenuController
        {
            if (Controllers.TryGetValue(name, out var ctrl))
            {
                return ctrl as T;
            }

            return null;
        }

        public static void SetActive(string name, bool state)
        {
            GetMenuController<MenuController>(name)?.SetActive(state);
        }

        /// <summary>
        /// Sets overlay as current.
        /// </summary>
        /// <param name="index">Index.</param>
        public void SetCurrentOverlay(int index)
        {
            CurrentOverlay = Overlays[index];

            if (OverlaysStack.Count == 0)
            {
                OverlaysStack.Add(CurrentOverlay);
            }
        }

        /// <summary>
        /// Sets overlay as current.
        /// </summary>
        /// <param name="screen">Target.</param>
        public void SetCurrentOverlay(MenuOverlay screen) => SetCurrentOverlay(Overlays.IndexOf(screen));

        /// <summary>
        /// Closes current screen, and opens previous one if available.
        /// </summary>
        public virtual void GoBack(bool animate = true)
        {
            StartCoroutine(CloseCurrentOverlayRoutine(animate));
        }

        /// <summary>
        /// Open overlay and set it as current one.
        /// </summary>
        /// <param name="index">Target index/</param>
        public void OpenOverlay(int index)
        {
            OpenOverlay(Overlays[index]);
        }

        /// <summary>
        /// Open overlay and set it as current one.
        /// </summary>
        public void OpenOverlay<T>() where T : MenuOverlay
        {
            OpenOverlay(Overlays.Find(overlay => overlay is T) as T);
        }

        /// <summary>
        /// Open overlay and set it as current one.
        /// </summary>
        /// <param name="overlay">Target overlay.</param>
        public virtual void OpenOverlay(MenuOverlay overlay)
        {
            if (overlay == null)
            {
                Debug.LogError("Given overlay is null.");
                return;
            }

            StartCoroutine(OpenOverlayRoutine(overlay));
        }

        /// <summary>
        /// Find overlay in list of available overlays and return it.
        /// </summary>
        public T GetOverlay<T>() where T : MenuOverlay
        {
            Initialize();
            return Overlays.Find(overlay => overlay is T) as T;
        }

        /// <summary>
        /// Find overlays in list of available overlays by type.
        /// </summary>
        public IEnumerable<T> GetOverlays<T>() where T : MenuOverlay
        {
            Initialize();
            return Overlays
                .FindAll(overlay => overlay is T)
                .Cast<T>();
        }

        /// <summary>
        /// Changes controller state.
        /// </summary>
        /// <param name="state">State value.</param>
        public virtual void SetActive(bool state)
        {
            if (this.state == state)
            {
                return;
            }

            if (state)
            {
                if (ActiveMenuController != null && ActiveMenuController != this)
                {
                    ActiveMenuController.SetActive(false);
                }

                ActiveMenuController = this;

                // Activate GO.
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                // Animate menu controller.
                if (tween != null) tween.PlayForward(false);

                // Refresh active overlay.
                ActiveMenuController.CurrentOverlay?.RefreshContent();
            }
            else
            {
                // Animate backward; if no tween, disable immediately.
                if (tween != null)
                    tween.PlayBackward(false);
                else
                    gameObject.SetActive(false);
            }

            this.state = state;

            // Toggle raycaster.
            if (raycaster != null) raycaster.enabled = state;
        }

        /// <summary>
        /// Add overlay to controller overlays list.
        /// </summary>
        public T AddOverlay<T>(MenuOverlay prefab) where T : MenuOverlay
        {
            if (!prefab) return null;

            MenuOverlay newOverlay = Instantiate(prefab, transform);
            newOverlay.transform.localScale = Vector3.one;
            Overlays.Add(newOverlay);

            return (T)newOverlay;
        }

        private void Initialize()
        {
            if (Initialized) return;

            Initialized = true;

            Overlays = new List<MenuOverlay>(transform.GetComponentsInChildren<MenuOverlay>(true));
            OverlaysStack = OverlaysStack ?? new List<MenuOverlay>();

            raycaster = GetComponent<GraphicRaycaster>();
            tween = GetComponent<TweenBase>();

            if (tween != null)
            {
                tween._onProgress -= OnTweenProgress; // avoid double-subscribe
                tween._onProgress += OnTweenProgress;
            }

            // Ensure unique/updated registration by name
            Controllers[name] = this;

            if (state)
            {
                ActiveMenuController = this;
                if (raycaster != null) raycaster.enabled = true;
            }
            else
            {
                if (raycaster != null) raycaster.enabled = false;
                // If starting inactive and no tween will hide us, ensure GO matches state.
                if (tween == null && gameObject.activeSelf) gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Deactivates controller when fadeOut animation is complete.
        /// </summary>
        private void OnTweenProgress(float value)
        {
            if (tween != null &&
                tween.playbackDirection == PlaybackDirection.BACKWARD &&
                Mathf.Approximately(value, 0f))
            {
                gameObject.SetActive(false);
            }
        }

        private void CheckCurrentTransitionRoutineCancel()
        {
            if (currentTransitionRoutine != null)
            {
                StopCoroutine(currentTransitionRoutine);
                currentTransitionRoutine = null;
            }
        }

        private IEnumerator OpenOverlayRoutine(MenuOverlay overlay)
        {
            CheckCurrentTransitionRoutineCancel();

            if (CurrentOverlay && CurrentOverlay != overlay && CurrentOverlay.IsOpened && overlay.ClosePreviousWhenOpened)
            {
                currentTransitionRoutine = StartCoroutine(CurrentOverlay.Close());

                if (sequentialTransition)
                {
                    yield return currentTransitionRoutine;
                }
            }

            if (CurrentOverlay != overlay)
            {
                OverlaysStack.Add(overlay);
            }

            SetCurrentOverlay(overlay);
            currentTransitionRoutine = StartCoroutine(CurrentOverlay.Open());

            yield return currentTransitionRoutine;

            currentTransitionRoutine = null;
        }

        private IEnumerator CloseCurrentOverlayRoutine(bool animate)
        {
            // Close current.
            if (OverlaysStack.Count == 0)
            {
                yield break;
            }

            CheckCurrentTransitionRoutineCancel();

            MenuOverlay last = OverlaysStack.Last();
            currentTransitionRoutine = StartCoroutine(last.Close(animate));

            if (sequentialTransition)
            {
                yield return currentTransitionRoutine;
            }

            OverlaysStack.Remove(last);

            // Open previous.
            if (OverlaysStack.Count == 0)
            {
                currentTransitionRoutine = null;
                yield break;
            }

            MenuOverlay previous = OverlaysStack.Last();
            SetCurrentOverlay(previous);
            currentTransitionRoutine = StartCoroutine(previous.Open());

            yield return currentTransitionRoutine;

            currentTransitionRoutine = null;
        }
    }
}
