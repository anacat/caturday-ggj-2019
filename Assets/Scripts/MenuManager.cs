using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Rooms;
    public GameObject Game;
    public GameObject IpAddress;
    private Text _ipAddressText;

    private void Start()
    {
        _ipAddressText = IpAddress.GetComponent<Text>();
    }

    public void ShowMenu()
    {
        Rooms.SetActive(false);
        GameManager.Instance.NetworkManager.StopBroadcastClient();
        MainMenu.SetActive(true);
    }

    public void ShowRooms()
    {
        MainMenu.SetActive(false);
        GameManager.Instance.NetworkManager.StartBroadcastClient();
        Rooms.SetActive(true);
    }

    public void StartGame()
    {
        MainMenu.SetActive(false);
        GameManager.Instance.NetworkManager.StartBroadcasting();
        Game.SetActive(true);
    }

    public void JoinGame()
    {
        Rooms.SetActive(false);
        GameManager.Instance.NetworkManager.StopBroadcastClient();
    }

    public void JoinIpGame()
    {
        //_ipAddressText.text
    }
}
