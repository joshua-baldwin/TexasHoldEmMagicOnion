using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class PopupUi : MonoBehaviour
    {
        [SerializeField] private GameObject contents;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button okButton;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        
        private Action onCloseAction;
        private Action onYesAction;
        private Action onNoAction;

        private void Awake()
        {
            okButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CloseMessage())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            yesButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OnConfirm(true))
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            noButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OnConfirm(false))
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowMessage(string message, Action onClose = null)
        {
            onCloseAction = onClose;
            contents.SetActive(true);
            okButton.gameObject.SetActive(true);
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
            messageText.text = message;
        }

        public void ShowConfirmation(string message, Action onYes, Action onNo)
        {
            onYesAction = onYes;
            onNoAction = onNo;
            contents.SetActive(true);
            okButton.gameObject.SetActive(false);
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
            messageText.text = message;
        }

        public void CloseMessage()
        {
            onCloseAction?.Invoke();
            contents.SetActive(false);
        }
        
        private void OnConfirm(bool isYes)
        {
            if (isYes)
                onYesAction?.Invoke();
            else
                onNoAction?.Invoke();
            
            contents.SetActive(false);
        }
    }
}
