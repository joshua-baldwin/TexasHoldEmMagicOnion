using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace THE.SceneUis
{
    public abstract class BaseLayoutUi : MonoBehaviour
    {
        private PopupUi popupUi;
        
        protected void ShowMessage(string message, Action onClose)
        {
            popupUi = FindFirstObjectByType<PopupUi>();
            popupUi.ShowMessage(message, onClose);
        }
        
        protected async UniTaskVoid ShowConfirmation(string message, Action onConfirm, Action onCancel)
        {
            popupUi = FindFirstObjectByType<PopupUi>();
            popupUi.ShowConfirmation(message, onConfirm, onCancel);
        }
    }
}