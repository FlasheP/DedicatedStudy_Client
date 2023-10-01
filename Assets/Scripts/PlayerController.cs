using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void FixedUpdate()
    {
        SendInputToServer();
    }
    private void SendInputToServer()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //일부러 이동 한 후 위치 값을 보내는게 아니라 인풋 방향만 보낸다.
        //안그러면 혹시 클라 변조? 같은거로 위치 조작 할 수도있으니까..
        ClientSend.PlayerMovement(horizontal, vertical);
    }
}
