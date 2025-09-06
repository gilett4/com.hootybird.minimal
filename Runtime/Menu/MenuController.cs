using HootyBird.Minimal.Tween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.Minimal.Menu
{
    /// <summary>
    /// Menu controller. Handles menu overlays under it.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        /// <summary>
        /// Active controller reference.
        /// </summary>
        public static MenuController ActiveMenuController { get; protected set; }
        /// <summary>
        /// All available controllers.
        /// </summary>
        public static Dictionary<string, MenuController> Controllers = new Dictionary<string, MenuController>(); 

        [SerializeField]
        public bool state = false;
        [SerializeField]
        private bool sequentialTransition = false;

        protected TweenBase tween;
        protected GraphicRaycaster raycaster;

        public List<MenuOverlay> Overlays { get; protected set; }
        public List<MenuOverlay> OverlaysStack { get; protected set; }
        public MenuOverlay CurrentOverlay { get; protected set; }
        public bool Initialized { get; private set; }
        public bool State => state;

        protected virtual void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Handles "android back"/"keyboard escape" button.
        /// </summary>
        protected virtual void Update()
        {
            if (state && OverlaysStack.Count > 0 && Input.GetKeyDown(KeyCode.Escape))
            {
                MenuOverlay overlay = OverlaysStack.Last();

                if (!overlay.Interactable) return;

                // Invoke OnBack on current screen.
                overlay.OnBack();
            }
        }


        protected virtual void OnEnable()
        {
            // Reopen active overlay when this controller activated.
            if (CurrentOverlay)
            {
                CurrentOverlay.Open();
            }
        }

        public static T GetMenuController<T>(string name) where T : MenuController
        {
            if (Controllers.ContainsKey(name))
            {
                return Controllers[name] as T;
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
        /// <param name="overlay">Target overlay.</param>
        public void OpenOverlay<T>() where T : MenuOverlay
        {
            OpenOverlay(Overlays.Find(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T))) as T);
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
        /// <typeparam name="T">Overlay type.</typeparam>
        /// <returns>Target overlay by type.</returns>
        public T GetOverlay<T>() where T : MenuOverlay
        {
            Initialize();

            return Overlays.Find(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T))) as T;
        }

        /// <summary>
        /// Find overlays in list of available overlays by type.
        /// </summary>
        /// <typeparam name="T">Overlay type.</typeparam>
        /// <returns>Overlays list by type.</returns>
        public IEnumerable<T> GetOverlays<T>() where T : MenuOverlay
        {
            Initialize();

            return Overlays
                .FindAll(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T)))
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
                if (ActiveMenuController != this)
                {
                    ActiveMenuController.SetActive(false);
                }

                ActiveMenuController = this;
                // Activate GO.
                gameObject.SetActive(true);
                // Animate menu controller.
                tween.PlayForward(false);
                // Refresh active overlay.
                ActiveMenuController.CurrentOverlay?.RefreshContent();
            }
            else
            {
                tween.PlayBackward(false);
            }

            this.state = state;

            // Toogle raycaster.
            raycaster.enabled = state;
        }

        /// <summary>
        /// Add overlay to controller overlays list.
        /// </summary>
        /// <typeparam name="T">Return overlay type.</typeparam>
        /// <param name="prefab">Overlay prefab.</param>
        /// <returns></returns>
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
            if (Initialized)
            {
                return;
            }

            Initialized = true;
            Overlays = new List<MenuOverlay>(transform.GetComponentsInChildren<MenuOverlay>());
            OverlaysStack = new List<MenuOverlay>();

            raycaster = GetComponent<GraphicRaycaster>();
            tween = GetComponent<TweenBase>();
            tween._onProgress += OnTweenProgress;

            Controllers.Add(name, this);

            if (state)
            {
                ActiveMenuController = this;
            }
            else
            {
                raycaster.enabled = false;
            }
        }

        /// <summary>
        /// Deactivates controller when fadeOut animation is complete.
        /// </summary>
        /// <param name="value"></param>
        private void OnTweenProgress(float value)
        {
            if (tween.playbackDirection == PlaybackDirection.BACKWARD && value == 0f)
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator OpenOverlayRoutine(MenuOverlay overlay)
        {
            if (CurrentOverlay && CurrentOverlay != overlay && CurrentOverlay.IsOpened && overlay.ClosePreviousWhenOpened)
            {
                if (sequentialTransition)
                {
                    yield return CurrentOverlay.Close();
                }
                else
                {
                    StartCoroutine(CurrentOverlay.Close());
                }
            }

            if (!CurrentOverlay || (CurrentOverlay != overlay))
            {
                OverlaysStack.Add(overlay);
            }

            SetCurrentOverlay(overlay);
            StartCoroutine(CurrentOverlay.Open());
        }

        private IEnumerator CloseCurrentOverlayRoutine(bool animate)
        {
            // Close current.
            if (OverlaysStack.Count == 0)
            {
                yield break;
            }

            MenuOverlay last = OverlaysStack.Last();
            if (sequentialTransition)
            {
                yield return last.Close(animate);
            }
            else
            {
                StartCoroutine(last.Close(animate));
            }

            OverlaysStack.Remove(last);

            // Open previous.
            if (OverlaysStack.Count == 0)
            {
                yield break;
            }

            MenuOverlay previous = OverlaysStack.Last();
            SetCurrentOverlay(previous);
            StartCoroutine(previous.Open());
        }
    }
}