using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneManager : MonoBehaviour
{
    [Header("Networking Ref")]
    [SerializeField]
    private NetworkManager _networkManager;

    [Header("UI Ref")]
    [SerializeField]
    private GameObject _mainMenu;
    [SerializeField]
    private TMP_InputField _nameInputField;

    [Header("- Player Data container")]
    public LocalPlayerData LocalPlayerDataContainer;


    public void EnterInRoom() 
    {
        //set player name
        if (!string.IsNullOrEmpty(_nameInputField.text))
        {
            LocalPlayerDataContainer.PlayerName = _nameInputField.text;
        }
        //Start client
        _networkManager.StartClient();
        //disable main menu
        _mainMenu.SetActive(false);
        
    }

}
