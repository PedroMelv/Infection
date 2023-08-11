using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuObj;

    private LobbyController lobby;

    private void Awake()
    {
        lobby = GetComponent<LobbyController>();
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
}
