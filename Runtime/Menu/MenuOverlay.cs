using HootyBird.Minimal.Services;
using HootyBird.Minimal.Tween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.Minimal.Menu
{
    /// <summary>
    /// Base overlay that menu consists of.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuOverlay : MonoBehaviour
    {
        [SerializeField]
        private bool closePreviousWhenOpened = true;

        [SerializeField]
        [Tooltip("What audio to play when onBack invoked")]
        protected string onBackSfx = "menu-back";

        [SerializeField]
        protected bool isDefault = false;

        protected List<MenuWidget> widgets;
        protected CanvasGroup canvasGroup;
        protected TweenBase tween;

        /// <summary>
        /// Controller that this overlay is under.
        /// </summary>
        public MenuController MenuController { get; private set; }
        /// <summary>
        /// Was overlay opened?
        /// </summary>
        public bool IsOpened { get; private set; }
        public TransitionState Transition { get; private set; } = TransitionState.None;
        /// <summary>
        /// Is currently active overlay?
        /// </summary>
        public virtual bool IsCurrent => MenuController ? MenuController.CurrentOverlay == this : false;
        public bool ClosePreviousWhenOpened => closePreviousWhenOpened;
        public bool Interactable => canvasGroup ? canvasGroup.interactable : gameObject.activeInHierarchy;
        public RectTransform RectTransform { get; protected set; }

        protected virtual void Awake()
        {
            MenuController = GetComponentInParent<MenuController>();

            canvasGroup = GetComponent<CanvasGroup>();
            tween = GetComponent<TweenBase>();
            RectTransform = GetComponent<RectTransform>();

            widgets = new List<MenuWidget>(GetComponentsInChildren<MenuWidget>());
        }

        protected virtual void Start()
        {
            if (isDefault)
            {
                MenuController.SetCurrentOverlay(this);
                RefreshContent();
                IsOpened = true;
            }
        }

        protected virtual void OnDisable()
        {
            switch (Transition)
            {
                case TransitionState.Openning:
                    tween.Progress(1f, PlaybackDirection.FORWARD);

                    SetInteractable(true);
                    SetBlockRaycasts(true);

                    break;

                case TransitionState.Closing:
                    tween.Progress(0f, PlaybackDirection.FORWARD);

                    SetInteractable(false);
                    SetBlockRaycasts(false);

                    break;

            }

            Transition = TransitionState.None;
        }

        /// <summary>
        /// Invoked when overlay is opened.
        /// </summary>
        public virtual IEnumerator Open(bool animate = true)
        {
            RefreshContent();

            // Already opened...
            if (IsOpened)
            {
                yield break;
            }

            Transition = TransitionState.Openning;
            IsOpened = true;

            if (tween)
            {
                if (animate)
                {
                    tween.PlayForward(true);

                    // Wait for tween to finish.
                    while (tween.isPlaying)
                    {
                        yield return null;
                    }
                }
                else
                {
                    tween.Progress(1f, PlaybackDirection.FORWARD);
                }
            }

            SetInteractable(true);
            SetBlockRaycasts(true);

            Transition = TransitionState.None;
        }

        public virtual void RefreshContent() 
        {
            UpdateWidgets();
        }

        /// <summary>
        /// Invoked when overlay is closed.
        /// </summary>
        public virtual IEnumerator Close(bool animate = true)
        {
            // Already closed...
            if (!IsOpened)
            {
                yield break;
            }

            Transition = TransitionState.Closing;
            IsOpened = false;

            SetInteractable(false);
            SetBlockRaycasts(false);

            if (tween)
            {
                if (animate)
                {
                    tween.PlayBackward(true);

                    // Wait for tween to finish.
                    while (tween.isPlaying)
                    {
                        yield return null;
                    }
                }
                else
                {
                    tween.Progress(0f, PlaybackDirection.FORWARD);
                }
            }

            Transition = TransitionState.None;
        }

        public T GetWidget<T>() where T : MenuWidget
        {
            return widgets.Find(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T))) as T;
        }

        public IEnumerable<T> GetWidgets<T>() where T : MenuWidget
        {
            return widgets
                .FindAll(overlay => overlay.GetType() == typeof(T) || overlay.GetType().IsSubclassOf(typeof(T)))
                .Cast<T>();
        }

        /// <summary>
        /// Invoked when back button is pressed (<see cref="MenuController.Update"/>);
        /// </summary>
        public virtual void OnBack()
        {
            AudioService.Instance.PlaySfx(onBackSfx);

            MenuController.GoBack();
        }

        /// <summary>
        /// Change overlay interactable state.
        /// </summary>
        /// <param name="state"></param>
        public void SetInteractable(bool state)
        {
            if (canvasGroup)
            {
                canvasGroup.interactable = state;
            }
        }

        public void SetBlockRaycasts(bool state)
        {
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = state;
            }
        }

        /// <summary>
        /// Invoked when overlay is refreshed.
        /// </summary>
        private void UpdateWidgets()
        {
            foreach (MenuWidget widget in widgets)
            {
                widget.UpdateWidget();
            }
        }

        public enum TransitionState 
        { 
            None = 0,
            Openning = 1,
            Closing = 2,
        }
    }
}