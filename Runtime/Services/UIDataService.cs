using HootyBird.Minimal.Repositories;
using UnityEngine;

namespace HootyBird.Minimal.Services
{
    public class UIDataService : MonoBehaviour
    {
        public static UIDataService Instance { get; private set; }

        [SerializeField]
        private UIRepository uiRepository;

        public UIRepository UIRepository => uiRepository;

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
