using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Rooms;
    public GameObject Game;
    public GameObject player;
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
        GameManager.Instance.NetworkManager.StartTcpServer();
        Game.SetActive(true);
    }

    public void JoinIpGame()
    {
        Rooms.SetActive(false);
        GameManager.Instance.NetworkManager.StopBroadcastClient();
        GameManager.Instance.NetworkManager.ConnectToTcpServer();
        Game.SetActive(true);
    }

    public void JoinIpGame(BroadcastMessage broadcastMessage)
    {
        Rooms.SetActive(false);
        GameManager.Instance.NetworkManager.StopBroadcastClient();
        GameManager.Instance.NetworkManager.ConnectToTcpServer(broadcastMessage);
        Game.SetActive(true);
    }
}
