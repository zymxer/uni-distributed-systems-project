using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
public class Server : MonoBehaviour
{
    [SerializeField] private GameObject tank;
    [SerializeField] private GameObject missile;
    [SerializeField] private TMP_Text ipText;
    [SerializeField] private TMP_Text clientsText;
    public TMP_Text receivedText;
    private TcpListener _server;
    private List<ClientHandler> _clientHandlers;
    private Thread _serverThread;
    private bool _isAccepting = true;
    private AutoResetEvent _resetEvent;

    private int _clientsAmount = 0;
    
    private List<List<GameObject>> _drawnTanks;
    private List<List<GameObject>> _drawnMissiles;
    

    private void Start()
    {
        _server = new TcpListener(IPAddress.Any, 10001);
        _server.Start();
        _clientHandlers = new List<ClientHandler>();

        _drawnTanks = new List<List<GameObject>>();
        _drawnMissiles = new List<List<GameObject>>();
        
        _serverThread = new Thread(AcceptClients);
        _serverThread.Start();

        _resetEvent = new AutoResetEvent(false);
        
        ipText.text = GetLocalIPAddress();
    }

    private void Update()
    {
        if (_isAccepting)
            return;
        SendData();
        DrawData();
        
    }

    private void AcceptClients()
    {
        while (_isAccepting)
        {
            try
            {
                TcpClient client = _server.AcceptTcpClient();
                clientsText.text += client.Client + " connected\n";
                ClientHandler handler = gameObject.AddComponent<ClientHandler>();
                
                _drawnTanks.Add(new List<GameObject>());
                _drawnMissiles.Add(new List<GameObject>());
                
                handler.Initialize(client, _resetEvent, _clientsAmount++);
                _clientHandlers.Add(handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private void SendData()
    {
        foreach (ClientHandler handler in _clientHandlers)
        {
            foreach (ClientHandler handler2 in _clientHandlers)
            {
                if (handler != handler2)
                {
                    if(handler2.ReceivedData != null)
                        handler.DataToSend.Add(handler2.ReceivedData);
                }
            }
        }
        _resetEvent.Set();
    }

    private void DrawData()
    {
        //receivedText.text = handler.ReceivedData == null? "NUll received" : "Normal data";
        foreach (ClientHandler handler in _clientHandlers)
        {
            DrawTanks(handler);
            DrawMissiles(handler);
        }
    }

    private void DrawTanks(ClientHandler handler)
    {
        List<ObjectData> tanksData = handler.ReceivedData.Tanks;
        Color teamColor = handler.ReceivedData.TeamColor;
        int handlerIndex = handler.DrawnIndex;
        List<GameObject> drawnList = _drawnTanks[handlerIndex];
        for (int i = 0; i < tanksData.Count; i++)
        {
            if (drawnList.Count <= i)
            {
                drawnList.Add(Instantiate(tank, Vector3.zero, Quaternion.identity));
            }

            drawnList[i].transform.position = tanksData[i].Position;
            drawnList[i].transform.rotation = tanksData[i].Rotation;
            drawnList[i].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = teamColor;
            drawnList[i].SetActive(tanksData[i].Active);
        }
    }
    private void DrawMissiles(ClientHandler handler)
    {
        List<ObjectData> missilesData = handler.ReceivedData.Missiles;
        int handlerIndex = handler.DrawnIndex;
        List<GameObject> drawnList = _drawnMissiles[handlerIndex];
        for (int i = 0; i < missilesData.Count; i++)
        {
            if (drawnList.Count <= i)
            {
                drawnList.Add(Instantiate(missile, Vector3.zero, Quaternion.identity));
            }

            drawnList[i].transform.position = missilesData[i].Position;
            drawnList[i].transform.rotation = missilesData[i].Rotation;
            drawnList[i].SetActive(missilesData[i].Active);
        }
    }

    public void StartGame()
    {
        SendStartMessage();
        _isAccepting = false;
        _serverThread.Abort();
    }

    private void SendStartMessage()
    {
        try
        {
            
            byte[] data = BitConverter.GetBytes(true);
            
            byte[] sizeBytes = BitConverter.GetBytes(data.Length);

            foreach (ClientHandler handler in _clientHandlers)
            {
                handler.StartHandler();
                handler.Client.GetStream().Write(sizeBytes, 0, sizeBytes.Length);
                handler.Client.GetStream().Write(data, 0, data.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in SendGameStartMessage: " + e.Message);
        }
    }
    
    private String GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
    private void Shutdown()
    {
        if (_isAccepting)
        {
            _isAccepting = false;
            _serverThread.Abort();
        }
        foreach (ClientHandler handler in _clientHandlers)
        {
            handler.Shutdown();
        }
    }
    private void OnApplicationQuit()
    {
        Shutdown();
    }
}
