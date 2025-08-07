using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class NetworkManagerP2P : MonoBehaviour
{
    public static NetworkManagerP2P Instance;

    TcpClient client;
    TcpListener server;
    NetworkStream stream;
    Thread receiveThread;
    public bool isHost;

    public event Action<int> OnMoveReceived;
    public event Action OnConnectedToHost;

    private bool triggerLoadScene = false;

    // 🔧 Fila para processar jogadas na thread principal
    private Queue<int> pendingMoves = new Queue<int>();
    private readonly object moveLock = new object();

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
        // 🔁 Gatilho para carregar cena após conexão
        if (triggerLoadScene)
        {
            triggerLoadScene = false;
            OnConnectedToHost?.Invoke();
        }

        // ✅ Processa jogadas na thread principal
        lock (moveLock)
        {
            while (pendingMoves.Count > 0)
            {
                int move = pendingMoves.Dequeue();
                OnMoveReceived?.Invoke(move);
            }
        }
    }

    public void StartHost(int port)
    {
        try
        {
            isHost = true;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            server.BeginAcceptTcpClient(OnClientConnected, null);
            Debug.Log("Aguardando conexão do cliente...");
        }
        catch (SocketException e)
        {
            Debug.LogError("Erro ao iniciar host: " + e.Message);
        }
    }

    public void ConnectToHost(string ip, int port)
    {
        try
        {
            isHost = false;
            client = new TcpClient();
            client.BeginConnect(ip, port, OnConnected, null);
            Debug.Log($"Tentando conectar ao host {ip}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao conectar: " + e.Message);
        }
    }

    void OnClientConnected(IAsyncResult result)
    {
        client = server.EndAcceptTcpClient(result);
        stream = client.GetStream();
        StartReceiving();
        Debug.Log("Cliente conectado.");
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

                        // ✅ Enfileira jogada para ser processada na thread principal
                        lock (moveLock)
                        {
                            pendingMoves.Enqueue(column);
                        }
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