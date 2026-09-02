using UnityEngine;
using UnityEngine.EventSystems;
using WordsToolkit.Scripts.Popups;

namespace WordsToolkit.Scripts.Debugger
{
    public class DebugPanelOpener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private DebugPanel debugPanelPrefab;

        [SerializeField]
        private MenuManager menuManager;

        [SerializeField]
        private float holdDuration = 5f;

        private bool isHolding;
        private float holdTimer;
        private bool panelOpenedDuringHold;
        private bool hasOpenedOnce;

        private void Awake()
        {
            if (menuManager == null)
            {
                menuManager = FindObjectOfType<MenuManager>();
                if (menuManager == null)
                {
                    Debug.LogWarning("[DebugPanelOpener] MenuManager reference is not assigned and could not be found in the scene.");
                }
            }
        }

        private void Update()
        {
            if (!isHolding || panelOpenedDuringHold)
            {
                return;
            }

            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= holdDuration)
            {
                OpenDebugPanel();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (hasOpenedOnce)
            {
                OpenDebugPanel();
                return;
            }

            isHolding = true;
            panelOpenedDuringHold = false;
            holdTimer = 0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetHoldState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetHoldState();
        }

        private void ResetHoldState()
        {
            isHolding = false;
            holdTimer = 0f;
            panelOpenedDuringHold = false;
        }

        private void OpenDebugPanel()
        {
            if (debugPanelPrefab == null)
            {
                Debug.LogWarning("[DebugPanelOpener] DebugPanel prefab reference is not assigned.");
                return;
            }

            if (menuManager == null)
            {
                Debug.LogWarning("[DebugPanelOpener] MenuManager reference is missing. Unable to open the debug panel.");
                return;
            }

            menuManager.ShowPopup(debugPanelPrefab);
            panelOpenedDuringHold = true;
            hasOpenedOnce = true;
        }
    }
}
