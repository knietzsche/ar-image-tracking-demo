using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [SerializeField] private Button _setSessionActiveButton = null;

    private void Awake()
    {
        if (_setSessionActiveButton != null)
        {
            _setSessionActiveButton.onClick.AddListener(OnSessionActiveButtonClick);
        }
    }

    private void OnSessionActiveButtonClick()
    {
        ARSessionAction.SetActive?.Invoke(null);
    }
}
