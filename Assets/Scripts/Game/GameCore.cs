using System;
using System.Collections.Generic;
using System.Net;
using Game.Data;
using Game.Utils;
using Game.Views;
using SampleNetClient.Runtime;
using SampleNetClient.Runtime.Messages;
using UnityEngine;

public class GameCore : MonoBehaviour
{
    private NetClientHandler _netClientHandler;
    [SerializeField] private MinionView _minionPrefab;
    private readonly List<Minion> _blueTeamMinions = new();
    private readonly List<Minion> _redTeamMinions = new();
    private readonly Dictionary<ushort, GameEntity> _gameEntities = new();
    
    void Start()
    {
        var ip = IPAddress.Parse("127.0.0.1");
        var serverEp = new IPEndPoint(ip, 5555);
        _netClientHandler = new NetClientHandler(new LiteNetRUDPTransport());
        _netClientHandler.ConnResultReceived += NetClientHandlerOnConnResultReceived;
        _netClientHandler.Connect(serverEp, "1234");
        _netClientHandler.OnCustomMessageReceived += NetClientHandlerOnOnCustomMessageReceived;
    }

    private void NetClientHandlerOnOnCustomMessageReceived(
        ECustomMessageType type, 
        ArraySegment<byte> payload
    )
    {
        switch (type)
        {
            case ECustomMessageType.Spawn:
                OnSpawn(payload);
                break;
            case ECustomMessageType.Position:
                OnPositionReceived(payload);
                break;
        }
    }

    private void NetClientHandlerOnConnResultReceived(EConnectionResult arg1, string arg2)
    {
        Debug.Log($"NetClientHandlerOnConnResultReceived: {arg1}");
    }

    void Update()
    {
        _netClientHandler.Tick();
    }

    private void OnDisable()
    {
        _netClientHandler.Stop();
    }

    private void OnSpawn(ArraySegment<byte> spawnData)
    {
        var byteReader = new SegmentByteReader(spawnData);
        var team = (ETeam)byteReader.ReadByte();
        var pos = byteReader.ReadVector3();
        var id = byteReader.ReadUshort();

        var minionView = Instantiate(_minionPrefab, pos, Quaternion.identity);
        var minion = new Minion(minionView);
        minion.Id = id;

        if (team == ETeam.Blue)
            _blueTeamMinions.Add(minion);
        else _redTeamMinions.Add(minion);
        
        _gameEntities.Add(id, minion);
    }

    private void OnPositionReceived(ArraySegment<byte> data)
    {
        var byteReader = new SegmentByteReader(data);
        var id = byteReader.ReadUshort();
        var entityId = byteReader.ReadUshort();
        var position = byteReader.ReadVector3();

        _gameEntities.TryGetValue(id, out var entity);
        
        if (entity == null) return;
        
        entity.SetPosition(position);
    }
}
