using NotificationSystem;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using VRGlass.SocketIO;
using VRGlass.SocketIO.Data;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    public Text status;
    public Text messageChat;
    public Text players;
    public string Texto;

    [Header("Indify")]
    public int id;
    public string username;
    public string token;

    private NotificationManager _NotificationManager = new();
    private ChatManager _ChatManager = new();
    private ISocketConnection _connection;

    private void FixedUpdate()
    {
        players.text = "Players on-line: " + _ChatManager.ListUsers.Count.ToString();
        _ChatManager.RunFixedUpdate();

        if (_connection == null) return;

        if (!_connection.IsConnected())
        {
            messageChat.text = "Chat :";
        } 

        status.text = "Status :" + _connection.IsConnected().ToString();
    }

    public void RecebidoMessage(MessageDataChat messageData)
    {
        Debug.Log(messageData);
        Debug.Log("Recebido :" + messageData.Message);
        messageChat.text += " " + messageData.Message;
    }

    public void RecebidoListUsers(List<UserChat> users)
    {
        users.ForEach(x => Debug.Log("Users : " + x.Name));

        Debug.Log(users.Count);
    }

    public void RecebidoUserConected(User user)
    {
        Debug.Log("novo player entrou: " + user.UserName);
        messageChat.text += " " + "novo player entrou: " + user.UserName;

    }

    public void RecebidoUserDisconected(User user)
    {
        Debug.Log(user);
    }

    public void SendChatmessage()
    {
        _ChatManager.SendMessageChat(Texto);
    }

    public void LeaveRoom()
    {
        if (!_connection.IsConnected())
            return;

        _ChatManager.LeaveRoom();
        _connection.Disconnect();
    }
    public void ConectedRoom()
    {
        StartCoroutine(Conected());
    }

    public IEnumerator Conected()
    {
        IdentifyData identifyData = new()
        {
            UserId = id,
            UserName = username,
            Token = token
        };

        //Uri uri = new Uri("https://chat-atvos.virtual.town/");

        Uri uri = new Uri("https://chat-vt16.virtual.town/");

        SocketOptions options = new SocketOptions(uri, identifyData);

        _connection = new SocketIOConnection(options);
        _connection.Conect();

        yield return new WaitForSeconds(1f);
        _ChatManager.MessageChatCallback += RecebidoMessage;
        _ChatManager.UpdateListUsersChatCallback += RecebidoListUsers;
        _ChatManager.UserConnectedCallback += RecebidoUserConected;
        _ChatManager.UserDisconnectedCallback += RecebidoUserDisconected;
        _ChatManager.Initialize(_connection);

        yield return new WaitForSeconds(1);
        _ChatManager.JoinRoom("ola", 230);
    }
}
