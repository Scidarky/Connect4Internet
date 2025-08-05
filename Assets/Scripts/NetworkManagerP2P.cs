using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkManagerP2P : MonoBehaviour
{
    public static NetworkManagerP2P Instance;

    TcpClient client;
    TcpListener server;
    NetworkStream stream;
    Thread receiveThread;
    public bool isHost;

    // Evento para quando uma jogada (coluna) for recebida
    public event Action<int> OnMoveReceived;

    // Flag para indicar carregamento na thread principal
    private bool triggerLoadScene = false;

    // Evento para notificar conexão estabelecida na thread principal
    public event Action OnConnectedToHost;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Chama o evento de conexão na thread principal
        if (triggerLoadScene)
        {
            triggerLoadScene = false;
            OnConnectedToHost?.Invoke();
        }
    }

    public void StartHost(int port)
    {
        isHost = true;
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        server.BeginAcceptTcpClient(OnClientConnected, null);
        Debug.Log("Aguardando conexão do cliente...");
    }

    public void ConnectToHost(string ip, int port)
    {
        isHost = false;
        client = new TcpClient();
        client.BeginConnect(ip, port, OnConnected, null);
        Debug.Log($"Tentando conectar ao host {ip}:{port}");
    }

    void OnClientConnected(IAsyncResult result)
    {
        client = server.EndAcceptTcpClient(result);
        stream = client.GetStream();
        StartReceiving();
        Debug.Log("Cliente conectado.");
        
        // Opcional: notificar host que cliente chegou
        triggerLoadScene = true;
    }

    void OnConnected(IAsyncResult result)
    {
        try
        {
            client.EndConnect(result);
            stream = client.GetStream();
            StartReceiving();
            Debug.Log("Conectado ao host.");

            triggerLoadScene = true;
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao conectar: " + e.Message);
        }
    }

    void StartReceiving()
    {
        receiveThread = new Thread(() =>
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[256];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) continue;

                    string msg = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (int.TryParse(msg, out int column))
                    {
                        Debug.Log($"Mensagem recebida: coluna {column}");
                        OnMoveReceived?.Invoke(column);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Erro na thread de recebimento: " + e.Message);
            }
        });
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void SendMove(int column)
    {
        if (stream == null) return;

        try
        {
            byte[] data = Encoding.ASCII.GetBytes(column.ToString());
            stream.Write(data, 0, data.Length);
            Debug.Log($"Jogada enviada: coluna {column}");
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao enviar jogada: " + e.Message);
        }
    }
}