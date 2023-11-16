using UnityEngine;

namespace HootyBird.Minimal.Repositories
{
    /// <summary>
    /// Repositories container.
    /// </summary>
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler Instance { get; private set; }

        [SerializeField]
        private UIRepository uiRepository;
        [SerializeField]
        private AudioRepository audioRepository;

        public UIRepository UIRepository => uiRepository;

        public AudioRepository AudioRepository => audioRepository;

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
        }
    }
}
