using HootyBird.Minimal.Repositories;
using HootyBird.Minimal.Tween;
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
        public static MenuController ActiveMenuController { get; private set; }
        /// <summary>
        /// All available controllers.
        /// </summary>
        public static Dictionary<string, MenuController> Controllers = new Dictionary<string, MenuController>(); 

        [SerializeField]
        public bool active = false;

        private TweenBase tween;
        private GraphicRaycaster raycaster;

        public List<MenuOverlay> overlays { get; protected set; }
        public List<MenuOverlay> overlaysStack { get; protected set; }
        public MenuOverlay currentOverlay { get; protected set; }
        public bool initialized { get; private set; }

        protected virtual void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Handles "android back"/"keyboard escape" button.
        /// </summary>
        protected virtual void Update()
        {
            if (active && overlaysStack.Count > 0 && Input.GetKeyDown(KeyCode.Escape))
            {
                MenuOverlay overlay = overlaysStack.Last();

                if (!overlay.Interactable) return;

                // Invoke OnBack on current screen.
                overlay.OnBack();
            }
        }


        protected virtual void OnEnable()
        {
            // Reopen active overlay when this controller activated.
            if (currentOverlay)
            {
                currentOverlay.Open();
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
            currentOverlay = overlays[index];

            if (overlaysStack.Count == 0)
            {
                overlaysStack.Add(currentOverlay);
            }
        }

        /// <summary>
        /// Sets overlay as current.
        /// </summary>
        /// <param name="screen">Target.</param>
        public void SetCurrentOverlay(MenuOverlay screen) => SetCurrentOverlay(overlays.IndexOf(screen));

        /// <summary>
        /// Closes current screen, and opens previous one if available.
        /// </summary>
        public virtual void GoBack(bool animate = true)
        {
            if (overlaysStack.Count > 0)
            {
                CloseCurrent(animate);

                if (overlaysStack.Count > 0)
                {
                    MenuOverlay previous = overlaysStack.Last();
                    SetCurrentOverlay(previous);
                    previous.Open();
                }
            }
        }

        /// <summary>
        /// Close target overlay, if it's a current one, <see cref="GoBack(bool)"/> one overlay.
        /// </summary>
        /// <param name="overlay">Target overlay.</param>
        /// <param name="animate">Animated transition?</param>
        public void CloseOverlay(MenuOverlay overlay, bool animate = true)
        {
            if (overlaysStack.Contains(overlay))
            {
                if (currentOverlay == overlay)
                {
                    GoBack(animate);
                }
                else
                {
                    overlaysStack.Remove(overlay);
                    overlay.Close(animate);
                }
            }
        }

        /// <summary>
        /// Open overlay and set it as current one.
        /// </summary>
        /// <param name="index">Target index/</param>
        public void OpenOverlay(int index) => OpenOverlay(overlays[index]);

        /// <summary>
        /// Open overlay and set it as current one.
        /// </summary>
        /// <param name="overlay">Target overlay.</param>
        public void OpenOverlay(MenuOverlay overlay)
        {
            if (currentOverlay && currentOverlay != overlay && currentOverlay.IsOpened && overlay.ClosePreviousWhenOpened)
            {
                currentOverlay.Close();
            }

            if (!currentOverlay || (currentOverlay != overlay))
            {
                overlaysStack.Add(overlay);
            }

            SetCurrentOverlay(overlay);
            currentOverlay.Open();
        }

        /// <summary>
        /// Find overlay in list of available overlays and return it.
        /// </summary>
        /// <typeparam name="T">Overlay type.</typeparam>
        /// <returns>Target overlay by type.</returns>
        public T GetOverlay<T>() where T : MenuOverlay
        {
            Initialize();

            T overlay = overlays.Find(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T))) as T;

            if (overlay == null)
            {
                return AddOverlay<T>();
            }
            else
            {
                return overlay;
            }
        }

        /// <summary>
        /// Find overlays in list of available overlays by type.
        /// </summary>
        /// <typeparam name="T">Overlay type.</typeparam>
        /// <returns>Overlays list by type.</returns>
        public IEnumerable<T> GetOverlays<T>() where T : MenuOverlay
        {
            Initialize();

            return overlays
                .FindAll(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T)))
                .Cast<T>();
        }

        /// <summary>
        /// Changes controller state.
        /// </summary>
        /// <param name="state">State value.</param>
        public virtual void SetActive(bool state)
        {
            if (state == active)
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
                gameObject.SetActive(true);
                tween.PlayForward(false);
            }
            else
            {
                tween.PlayBackward(false);
            }

            active = state;

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
            overlays.Add(newOverlay);

            return (T)newOverlay;
        }

        /// <summary>
        /// Add overlay to controller using <see cref="DataHandler.UIRepository"/> UI assets.
        /// </summary>
        /// <typeparam name="T">Overlay to take from DataHandler.</typeparam>
        /// <returns>Overlay instance (by type).</returns>
        public T AddOverlay<T>() where T : MenuOverlay
        {
            return AddOverlay<T>(DataHandler.Instance.UIRepository.GetOverlay<T>());
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            overlays = new List<MenuOverlay>(transform.GetComponentsInChildren<MenuOverlay>());
            overlaysStack = new List<MenuOverlay>();

            raycaster = GetComponent<GraphicRaycaster>();
            tween = GetComponent<TweenBase>();
            tween._onProgress += OnTweenProgress;

            Controllers.Add(name, this);

            if (active)
            {
                ActiveMenuController = this;
            }
            else
            {
                raycaster.enabled = false;
            }
        }

        private void CloseCurrent(bool animate)
        {
            MenuOverlay last = overlaysStack.Last();
            last.Close(animate);
            overlaysStack.Remove(last);
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
    }
}