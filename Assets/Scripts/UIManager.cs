using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private Button ConnectButton;

    private void OnEnable()
    {
        ConnectButton.onClick.AddListener(() => ConnectToServer());
    }
    private void Start()
    {

    }

    public void ConnectToServer()
    {
        startMenuPanel.SetActive(false);
        userNameInputField.interactable = false;
        Client.instance.userName = userNameInputField.text;
        Client.instance.ConnectToServer();
    }
}
