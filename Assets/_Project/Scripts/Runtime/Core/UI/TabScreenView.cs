using UnityEngine;

namespace TestTask.Core.UI
{
    /// <summary>
    /// Базовый класс экрана вкладки.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TabScreenView : MonoBehaviour
    {
        public bool IsVisible => gameObject.activeSelf;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
