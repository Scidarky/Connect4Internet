using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    private NetworkDriver mDriver;
    NetworkConnection mConnection;
    void Start()
    {
        mDriver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        mConnection = mDriver.Connect(endpoint);
    }

    private void OnDestroy()
    {
        mDriver.Dispose();
    }

    void Update()
    {
        mDriver.ScheduleUpdate().Complete();

        if (!mConnection.IsCreated) return;

        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = mConnection.PopEvent(mDriver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected");

                uint value = 1;
                mDriver.BeginSend(mConnection, out var writer);
                writer.WriteUInt(value);
                mDriver.EndSend(writer);
            }
            
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log($"Got the value {value}");

                mConnection.Disconnect(mDriver);
                mConnection = default;
            }
        }
    }
}
