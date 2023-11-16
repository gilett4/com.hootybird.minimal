using UnityEngine;

namespace HootyBird.Minimal.Menu
{
    /// <summary>
    /// Base widget implementation that is updated when <see cref="Menu.MenuOverlay"/> is opened or regains focus.
    /// </summary>
    public abstract class MenuWidget : MonoBehaviour
    {
        /// <summary>
        /// MenuOverlay reference that this widget is under.
        /// </summary>
        public MenuOverlay MenuOverlay { get; set; }

        protected virtual void Awake()
        {
            MenuOverlay = GetComponentInParent<MenuOverlay>();
        }

        public virtual void UpdateWidget() { }
    }
}
