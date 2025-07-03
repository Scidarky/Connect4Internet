using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
public class TcpServerUnity : MonoBehaviour
{
    TcpListener server;
    Thread serverThread;
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