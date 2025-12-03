using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject uiImage;
    public GameObject settingPanel;
    public Button resumeButton;
    public Button settingButton;
    public Button exitButton;
    public Button closeButton;

    private bool isPaused = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiImage.SetActive(false);
        settingPanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClick);

        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClick);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClick);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel != null && settingPanel.activeSelf)
            {
                OnCloseClick();
            }
            else
            {
                if (isPaused)
                {
                    OnResumeClick();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        uiImage.SetActive(true);
    }

    void OnResumeClick()
    {
        isPaused = false;
        Time.timeScale = 1f;
        uiImage.SetActive(false);
        settingPanel.SetActive(false);
    }

    void OnSettingClick()
    {
        settingPanel.SetActive(true);
        uiImage.SetActive(false);
    }

    void OnCloseClick()
    {
        settingPanel.SetActive(false);
        uiImage.SetActive(true);
    }

    void OnExitClick()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
