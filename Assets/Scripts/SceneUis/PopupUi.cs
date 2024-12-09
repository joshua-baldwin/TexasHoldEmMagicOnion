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
        private Action onCloseAction;

        private void Awake()
        {
            okButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CloseMessage())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowMessage(string message, Action onClose = null)
        {
            onCloseAction = onClose;
            contents.SetActive(true);
            messageText.text = message;
        }

        public void CloseMessage()
        {
            onCloseAction?.Invoke();
            contents.SetActive(false);
        }
    }
}
