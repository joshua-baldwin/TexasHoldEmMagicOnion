using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class PopupUi : MonoBehaviour
    {
        [SerializeField] private GameObject contents;
        [SerializeField] private Text messageText;
        [SerializeField] private Button okButton;
        
        private void Awake()
        {
            okButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CloseMessage())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowMessage(string message)
        {
            contents.SetActive(true);
            messageText.text = message;
        }

        public void CloseMessage()
        {
            contents.SetActive(false);
        }
    }
}
