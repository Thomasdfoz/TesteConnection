using NotificationSystem;
using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRGlass.Essentials.Http;
using VRGlass.SocketIO;
using VRGlass.SocketIO.Data;

public class ConectionManager : MonoBehaviour
{
    private ISocketConnection _connection;
    private ChatManager _chat;
    private NotificationManager _notification;
    public string textoEnvio;
    public Text text;
    public Text textChat;
    public Text players;
    public bool isConect;
    public bool isConectVerific;

    [Header("Indify")]
    public int id;
    public string username;
    public string token;

    //private ISocketConnection _connection;
    private Dictionary<string, Action<SocketIOResponse>> _callbacks = new Dictionary<string, Action<SocketIOResponse>>();

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        if (_connection != null)
            Debug.Log(_connection.IsConnected());

        if (_connection != null)
        {
            ChangeText();
            _chat.RunFixedUpdate();

        }
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

        HttpRequest.HttpOptions httpOptions = new HttpRequest.HttpOptions("pt-br", token);

        SocketOptions options = new SocketOptions(uri, identifyData);
        _connection = new SocketIOConnection();
        _notification = new NotificationManager();
        _chat = new ChatManager();   
        _notification.Initialize(_connection, options, httpOptions);
        yield return new WaitForSeconds(0.1f);
        _connection.Conect(options);
        yield return new WaitForSeconds(0.1f);
        _chat.Initialize(_connection, _callbacks);        
        _connection.AddDictionaryCallback(_callbacks);
        _chat.MessageChatCallback += RecebidoMessage;
        _chat.UpdateListUsersChatCallback += UpdateListUsers;
        yield return new WaitForSeconds(0.1f);
        isConectVerific = true;
        yield return new WaitForSeconds(0.1f);
        _chat.JoinRoom("teste", 151);

    }

    private void AddCallbacks(string nameCallback, Action<SocketIOResponse> action)
    {
        if (_callbacks.ContainsKey(nameCallback))
        {
            _callbacks[nameCallback] += action;
        }
        else
        {
            _callbacks.Add(nameCallback, action);
        }
    }

    public void SendMessagechat()
    {
        _chat.SendMessageChat(textoEnvio);
    }

    public void RecebidoMessage(MessageDataChat response)
    {
        Debug.Log(response.Message);
        textChat.text += response.Message;
    }
    public void UpdateListUsers(List<UserChat> users)
    {
        players.text = "Player online : " + users.Count;
    }
    public void LeaveRoom()
    {
        _connection.Disconnect();

        isConectVerific = true;
    }

    public void ConectedRoom()
    {
        StartCoroutine(Conected());
    }

    // Update is called once per frame

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