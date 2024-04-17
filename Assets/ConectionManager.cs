using NotificationSystem;
using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VRGlass.Essentials.Http;
using VRGlass.SocketIO.Data;
using VRGlass.SocketIO;

public class ConectionManager : MonoBehaviour
{

    public Text text;
    public Text textChat;
    public bool isConect;
    public bool isConectVerific;

    [Header("Indify")]
    public int id;
    public string username;
    public string token;

    private NotificationManager _NotificationManager = new();
    private ChatManager _ChatManager = new();
    private ISocketConnection _connection;



    private void Start()
    {

        
    }
    private void FixedUpdate()
    {
        if (_connection != null)
            Debug.Log(_connection.IsConnected());

        _ChatManager.RunFixedUpdate();
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
        yield return new WaitForSeconds(0.1f);
        _connection.Conect(options);
        yield return new WaitForSeconds(0.1f);
        _connection.AddDictionaryCallback(_NotificationManager.GetCallbacks());
        yield return new WaitForSeconds(0.1f);

        _ChatManager.Initialize(_connection, new Dictionary<string, Action<SocketIOResponse>>());
        _ChatManager.MessageChatCallback += RecebidoMessage;
        _ChatManager.UpdateListUsersChatCallback += RecebidoListUsers;
        _ChatManager.UserConnectedCallback += RecebidoUserConected;
        _ChatManager.UserDisconnectedCallback += RecebidoUserDisconected;

        yield return new WaitForSeconds(0.1f);

        _connection.AddDictionaryCallback(_ChatManager.GetCallbacks());


        yield return new WaitForSeconds(0.1f);
        _ChatManager.JoinRoom("ola", 230);


        yield return new WaitForSeconds(0.1f);
        Debug.Log(_ChatManager.ListUsers.Count);

        isConectVerific = true;

    }


    public void RecebidoMessage(MessageDataChat messageData)
    {
        textChat.text += messageData.Message;
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

    public void SendChatmessage(string text)
    {
        _ChatManager.SendMessageChat(text);
    }

    public async void LeaveRoom()
    {
        await _connection.Disconnect();

        isConectVerific = true;
    }

    public void ConectedRoom()
    {
        StartCoroutine(Conected());
    }

    // Update is called once per frame
    void Update()
    {
        if (_connection != null)
        {
            isConect = _connection.IsConnected();
        }


        if (isConectVerific)
        {
            ChangeText();
        }  
    }

    public void ChangeText()
    {
        isConect = _connection.IsConnected();

        isConectVerific = false;

        if (isConect)
        {
            text.text = "Status On-Line";
        }
        else
        {
            text.text = "Status OFF-Line";
        }

    }
}
