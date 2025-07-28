using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
public class ServerBehaviour : MonoBehaviour
{
    private NetworkDriver mDriver;
    NativeList<NetworkConnection> mConnections;
    void Start()
    {
        serverThread = new Thread(new ThreadStart(StartServer));
        serverThread.IsBackground = true;
        serverThread.Start();
    }
    void StartServer()
    {
        IPAddress localAddr = IPAddress.Parse("10.57.10.39");
        server = new TcpListener(localAddr, 8080);
        server.Start();
    }
    
    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();
    }
}