using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIOClient.Newtonsoft.Json;
using Newtonsoft.Json;
using static ConectionManager.SocketIOConnection;

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

    private SocketIOConnection _connection;

    public SocketIOUnity _socket;
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

        SocketIOConnection.SocketOptions options = new SocketIOConnection.SocketOptions(uri, identifyData);

        _connection = new ConectionManager.SocketIOConnection();
        yield return new WaitForSeconds(0.1f);
        _connection.Conected(options);
        yield return new WaitForSeconds(0.1f);
        AddCallbacks("message", RecebidoMessage);
        _connection.AddDictionaryCallback(_callbacks);
        yield return new WaitForSeconds(0.1f);
        isConectVerific = true;

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

    public void RecebidoMessage(SocketIOResponse response)
    {
        Debug.Log(response);
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

    public class SocketIOConnection
    {
        public class SocketOptions
        {
            public readonly Uri Uri;
            public readonly IdentifyData UserData;

            public SocketOptions(Uri uri, IdentifyData userData)
            {
                Uri = uri;
                UserData = userData;
            }
        }

        [Serializable]
        public class IdentifyData
        {
            [JsonProperty("userId")]
            public int UserId { get; set; }

            [JsonProperty("userName")]
            public string UserName { get; set; }

            [JsonProperty("jwt")]
            public string Token { get; set; }
        }
        public SocketIOUnity _socket;

        private Dictionary<string, Action<SocketIOResponse>> _callbacks = new Dictionary<string, Action<SocketIOResponse>>();

        public SocketOptions Options { get; set; }

        public Action OnConected { get; set; }

        public Action OnDisconected { get; set; }

        public void AddCallbacks(string nameCallback, Action<SocketIOResponse> action)
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

        public void AddDictionaryCallback(Dictionary<string, Action<SocketIOResponse>> callbacks)
        {
            foreach (var item in callbacks)
            {
                _socket.On(item.Key, item.Value);
            }
        }

        public void Conected(SocketOptions options)
        {
            Options = options;

            _socket = new SocketIOUnity(Options.Uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                EIO = 4,                            //^_^\\
                Reconnection = true,                //|?|\\ Habilita reconexão automática
                ReconnectionAttempts = 5,           //|?|\\ Número máximo de tentativas de reconexão
                ReconnectionDelay = 1000,           //|?|\\ Atraso entre as tentativas de reconexão em milissegundos
                ReconnectionDelayMax = 5000,        //|?|\\ Atraso máximo entre as tentativas de reconexão
                RandomizationFactor = 0.5           //|?|\\ Fator de randomização para evitar reconexões simultâneas de vários clientes
            });

            _socket.JsonSerializer = new NewtonsoftJsonSerializer();

            foreach (var item in _callbacks)
            {
                _socket.On(item.Key, item.Value);
            }
            _socket.Connect();

            Idetifyed();
        }


        private void Idetifyed()
        {
            _socket.Emit("identify", Options.UserData);

            if (_socket.Connected)
                OnConected?.Invoke();
        }

        public void Disconnect()
        {
            _socket.DisconnectAsync();

            _callbacks.Clear();

            if (!_socket.Connected)
                OnDisconected?.Invoke();
        }

        public void Emit(string name, object data)
        {
            _socket.Emit(name, data);
        }

        public bool IsConnected()
        {
            return _socket == null ? false : _socket.Connected;
        }

        public SocketOptions GetOptions()
        {
            return Options;
        }
    }
}

