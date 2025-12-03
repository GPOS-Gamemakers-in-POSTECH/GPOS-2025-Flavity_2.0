using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    public GameObject settingPanel;
    public Button startGameButton;
    public Button settingButton;
    public Button exitGameButton;
    public Button closeButton;

    public void OnStartGameClick()
    {
        SceneManager.LoadScene("GravityTest");
    }

    public void OnSettingClick()
    {
        settingPanel.SetActive(true);
    }

    public void OnExitGameClick()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }

    public void OnCloseClick()
    {
        settingPanel.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClick);

        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClick);

        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(OnExitGameClick);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (settingPanel != null && settingPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                settingPanel.SetActive(false);
            }
        }
    }
}
