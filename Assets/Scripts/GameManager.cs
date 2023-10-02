using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int, PlayerManager> playersDic = new Dictionary<int, PlayerManager>();

    public PlayerManager localPlayerPrefab;
    public PlayerManager playerPrefab;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Instanve already exists, destroying this obj");
            Destroy(this.gameObject);
        }
    }
    public void SpawnPlayer(int _id, string _userName, Vector3 _position, Quaternion _rotation)
    {
        PlayerManager _player;
        if (_id == Client.instance.myId)
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        else
            _player = Instantiate(playerPrefab, _position, _rotation);

        _player.id = _id;
        _player.username = _userName;
        playersDic.Add(_id, _player);
    }

    public void DisconnectPlayer(int _id)
    {
        PlayerManager player = playersDic[_id];
        Destroy(player.gameObject);
        playersDic.Remove(_id);
    }
}
