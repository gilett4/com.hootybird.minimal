using HootyBird.Minimal.Services;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.Minimal.Menu
{
    public class PromptOverlay : MenuOverlay
    {
        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private TMP_Text description;
        [SerializeField]
        private TMP_Text accept;
        [SerializeField]
        private TMP_Text reject;

        [Space]
        [SerializeField]
        private Button acceptButton;
        [SerializeField]
        private Button rejectButton;

        private bool closeOnAccept;
        private bool closeOnReject;
        private Action OnAccept;
        private Action OnReject;

        protected override void Awake()
        {
            base.Awake();

            if (acceptButton)
            {
                acceptButton.onClick.AddListener(Accept);
            }

            if (rejectButton)
            {
                rejectButton.onClick.AddListener(Reject);
            }
        }

        public override void OnBack()
        {
            Reject();
        }

        public void SetPromptData(
            string titleText,
            string descriptionText = "",
            string acceptText = "", 
            string rejectText = "", 
            Action OnAccept = null, 
            Action OnReject = null)
        {
            title.text = titleText;

            if (!string.IsNullOrEmpty(descriptionText))
            {
                description.gameObject.SetActive(true);
                description.text = descriptionText;
            }
            else
            {
                description.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(rejectText))
            {
                reject.text = rejectText;
            }

            if (!string.IsNullOrEmpty(acceptText))
            {
                accept.text = acceptText;
            }

            SetButtonsEvents(OnAccept, OnReject);

            rejectButton.gameObject.SetActive(!string.IsNullOrEmpty(rejectText));
            acceptButton.gameObject.SetActive(!string.IsNullOrEmpty(acceptText));
        }

        public void SetButtonsEvents(Action OnAccept, Action OnReject)
        {
            this.OnAccept = OnAccept;
            this.OnReject = OnReject;
        }

        public void CloseOnAccept()
        {
            closeOnAccept = true;
        }

        public void CloseOnReject()
        {
            closeOnReject = true;
        }

        public void Accept()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);

            if (OnAccept == null)
            {
                CloseSelf();
            }
            else
            {
                OnAccept?.Invoke();

                if (closeOnAccept)
                {
                    CloseSelf();
                }
            }

            closeOnAccept = false;
        }

        public void Reject()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);

            if (OnReject == null)
            {
                CloseSelf();
            }
            else
            {
                OnReject.Invoke();
                
                if (closeOnReject)
                {
                    CloseSelf();
                }
            }

            closeOnReject = false;
        }
    }
}
