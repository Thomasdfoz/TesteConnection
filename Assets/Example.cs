using NotificationSystem;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using VRGlass.SocketIO;
using VRGlass.SocketIO.Data;
using SocketIOClient;
using TMPro.EditorUtilities;
using UnityEditor.Build.Content;
using VRGlass.Essentials.Http;

public class Example : MonoBehaviour
{
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
        if (_connection != null)
            Debug.Log(_connection.IsConnected());

        _ChatManager.RunFixedUpdate();
    }
    IEnumerator Start()
    {
        

        yield return new WaitForSeconds(10000);
       
    }
    

    public void RecebidoMessage(MessageDataChat messageData)
    {
        Debug.Log(messageData);
        Debug.Log("Recebido :" + messageData.Message);
    }

    public void RecebidoListUsers(List<UserChat> users)
    {
        users.ForEach(x => Debug.Log("Users : " + x.Name));
    }

    public void RecebidoUserConected(User user)
    {
        Debug.Log(user);

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

        _connection = new SocketIOConnection();
        _NotificationManager.Initialize(_connection, options, new HttpRequest.HttpOptions("pt-br", token));
        yield return new WaitForSeconds(1);
        _connection.Conect(options);
        yield return new WaitForSeconds(1);
        _connection.AddDictionaryCallback(_NotificationManager.GetCallbacks());
        yield return new WaitForSeconds(1);

        _ChatManager.Initialize(_connection, new Dictionary<string, Action<SocketIOResponse>>());
        _ChatManager.MessageChatCallback += RecebidoMessage;
        _ChatManager.UpdateListUsersChatCallback += RecebidoListUsers;
        _ChatManager.UserConnectedCallback += RecebidoUserConected;
        _ChatManager.UserDisconnectedCallback += RecebidoUserDisconected;       

        yield return new WaitForSeconds(1);

        _connection.AddDictionaryCallback(_ChatManager.GetCallbacks());


        yield return new WaitForSeconds(1);
        _ChatManager.JoinRoom("ola", 230);

        yield return new WaitForSeconds(1);
        Debug.Log(_ChatManager.ListUsers.Count);

    }

        void Update()
    {
        
    }
}
