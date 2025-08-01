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
        
        // Criar driver e lista
        mDriver = NetworkDriver.Create();
        mConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        // Escutar conexões
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
        if (mDriver.Bind(endpoint) != 0)
        {
            Debug.LogError($"Failed to bind to port 7777.");
            return;
        }
        mDriver.Listen();
    }
    void OnDestroy()
    {
        // Se dispor de memória em excesso
        if (mDriver.IsCreated)
        {
            mDriver.Dispose();
            mConnections.Dispose();
        }
    }
    
    void Update()
    {
        mDriver.ScheduleUpdate().Complete();
        
        // Limpar conexões antigas
        for (int i = 0; i < mConnections.Length; i++)
        {
            if (!mConnections[i].IsCreated)
            {
                mConnections.RemoveAtSwapBack(i);
                i--;
            }
        }
        
        // Aceitar novas conexões
        NetworkConnection c;
        while ((c = mDriver.Accept()) != default)
        {
            mConnections.Add(c);
            Debug.Log("Conexão aceita");
        }

        for (int i = 0; i < mConnections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = mDriver.PopEventForConnection(mConnections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();
                    Debug.Log($"Got {number} from a client, adding 2 to it.");
                    number += 2;

                    mDriver.BeginSend(NetworkPipeline.Null, mConnections[i], out var writer);
                    writer.WriteUInt(number);
                    mDriver.EndSend(writer);
                }
                
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected");
                    mConnections[i] = default;
                    break;
                }
                
                
            }
        }
    }
}