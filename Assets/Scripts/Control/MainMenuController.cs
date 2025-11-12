using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    // Drag your two panels here in the Inspector
    public GameObject mainButtonsPanel;
    public GameObject settingsPanel;

    void Start()
    {
        if (mainButtonsPanel != null)
        {
            mainButtonsPanel.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // This function is called by the "Settings" button
    public void OpenSettingsMenu()
    {
        if (mainButtonsPanel != null)
        {
            mainButtonsPanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // This function is called by the "Back" button (inside the settings panel)
    public void CloseSettingsMenu()
    {
        if (mainButtonsPanel != null)
        {
            mainButtonsPanel.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}