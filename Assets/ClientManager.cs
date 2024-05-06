using System.Collections;
using System.Collections.Generic;
using System.Net;
using SampleNetClient.Runtime;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    private NetClientHandler _netClientHandler;
    
    void Start()
    {
        var ip = IPAddress.Parse("127.0.0.1");
        var serverEp = new IPEndPoint(ip, 5555);
        _netClientHandler = new NetClientHandler(new LiteNetRUDPTransport());
        _netClientHandler.ConnResultReceived += NetClientHandlerOnConnResultReceived;
        _netClientHandler.Connect(serverEp, "1234");
    }

    private void NetClientHandlerOnConnResultReceived(EConnectionResult arg1, string arg2)
    {
        Debug.Log($"NetClientHandlerOnConnResultReceived: {arg1}");
    }

    void Update()
    {
        _netClientHandler.Tick();
    }
}
