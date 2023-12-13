using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuObj;
    [SerializeField] private GameObject tutorialObj;

    private LobbyController lobby;

    private void Awake()
    {
        lobby = GetComponent<LobbyController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayButton()
    {
        QuitMenu();

        lobby.EnterLobby();
    }

    public void OptionsButton()
    {
        QuitMenu();
    }

    public void TutorialButton()
    {
        QuitMenu();
        EnterTutorial();
    }
    
    public void CloseTutorial()
    {
        EnterMenu();
        QuitTutorial();
    }

    public void QuitButton()
    {
        Application.Quit();
    }
    public void EnterMenu()
    {
        mainMenuObj.SetActive(true);
    }
    public void QuitMenu()
    {
        mainMenuObj.SetActive(false);
    }

    public void EnterTutorial()
    {
        tutorialObj.SetActive(true);
    }
    public void QuitTutorial()
    {
        tutorialObj.SetActive(false);
    }
}
