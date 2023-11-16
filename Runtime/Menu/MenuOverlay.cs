//@vadym udod

using HootyBird.Minimal.Services;
using HootyBird.Minimal.Tween;
using System.Collections.Generic;
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
        /// <summary>
        /// Is currently active overlay?
        /// </summary>
        public virtual bool IsCurrent => MenuController ? MenuController.currentOverlay == this : false;
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
                UpdateWidgets();
                IsOpened = true;
            }
        }

        /// <summary>
        /// Invoked when overlay is opened.
        /// </summary>
        public virtual void Open()
        {
            UpdateWidgets();

            if (IsOpened) return;

            IsOpened = true;

            if (tween)
            {
                tween.PlayForward(true);
            }

            SetInteractable(true);
            SetBlockRaycasts(true);
        }

        /// <summary>
        /// Invoked when overlay is closed.
        /// </summary>
        public virtual void Close(bool animate = true)
        {
            IsOpened = false;

            if (tween)
            {
                if (animate)
                {
                    tween.PlayBackward(true);
                }
                else
                {
                    tween.Progress(0f, PlaybackDirection.FORWARD);
                }
            }

            SetInteractable(false);
            SetBlockRaycasts(false);
        }

        /// <summary>
        /// Removes overlay from active overlays stack, and closes it.
        /// </summary>
        /// <param name="animate"></param>
        public void CloseSelf(bool animate = true)
        {
            if (MenuController)
            {
                if (IsCurrent)
                {
                    MenuController.GoBack(animate);
                }
                else
                {
                    Close(animate);
                    if (MenuController.overlaysStack.Contains(this))
                    {
                        MenuController.overlaysStack.Remove(this);
                    }
                }
            }
            else
            {
                Close(animate);
            }
        }

        /// <summary>
        /// Blocks input on overlay.
        /// </summary>
        public virtual void BlockInput()
        {
            SetInteractable(false);
            canvasGroup.blocksRaycasts = true;
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
        /// Invoked when overlay regains focus.
        /// </summary>
        private void UpdateWidgets()
        {
            foreach (MenuWidget widget in widgets)
            {
                widget.UpdateWidget();
            }
        }
    }
}