using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
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
    private ManualResetEvent _resetEvent;
    private List<AutoResetEvent> _handlerEvents;

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

        _resetEvent = new ManualResetEvent(false);
        _handlerEvents = new List<AutoResetEvent>();
        
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
                Debug.Log(client.Client + " accepted");
                ClientHandler handler = gameObject.AddComponent<ClientHandler>();
                
                _drawnTanks.Add(new List<GameObject>());
                _drawnMissiles.Add(new List<GameObject>());
                
                handler.Initialize(client, _resetEvent ,_clientsAmount++);
                _clientHandlers.Add(handler);
                _handlerEvents.Add(handler.ResetEvent);
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
        //Debug.Log("SERVER: START WAITING FOR HANDLERS TO SEND DATA");
        foreach (AutoResetEvent resetEvent in _handlerEvents)
        {
            resetEvent.WaitOne();
        }
        //Debug.Log("SERVER: START PREPARING DATA FOR HANDLERS");

        _resetEvent.Reset();
        
        foreach (ClientHandler handler in _clientHandlers)
        {
            foreach (ClientHandler handler2 in _clientHandlers)
            {
                if (handler != handler2)
                {
                    if (handler2.ReceivedData != null)
                    {
                        handler.AddDataToSend(handler2.ReceivedData);
                    }

                }
            }
            //Debug.Log("SERVER: " + handler.DataToSend.Count + " OF DATAS WILL BE SEND TO client");
        }
        _resetEvent.Set();
    }

    private void DrawData()
    {
        foreach (ClientHandler handler in _clientHandlers)
        {
            DrawTanks(handler);
            DrawMissiles(handler);
        }
    }

    private void DrawTanks(ClientHandler handler)
    {
        if(handler.ReceivedData == null)
            return;
        List<ObjectData> tanksData = handler.ReceivedData.Tanks;
        if(tanksData == null)
            return;
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
        
        for (int i = 0; i < drawnList.Count; i++)
        {
            if (i > tanksData.Count)
            {
                Destroy(drawnList[i]);
            }
        }
    }
    private void DrawMissiles(ClientHandler handler)
    {
        if(handler.ReceivedData == null)
            return;
        List<ObjectData> missilesData = handler.ReceivedData.Missiles;
        if(missilesData == null)
            return;
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
        for (int i = 0; i < drawnList.Count; i++)
        {
            if (i > missilesData.Count)
            {
                Destroy(drawnList[i]);
            }
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
        }
    }
    private String GetLocalIPAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Check for Ethernet and Wi-Fi network interfaces
            if ((ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && ni.OperationalStatus == OperationalStatus.Up)
            {
                var ipProperties = ni.GetIPProperties();
                foreach (var ip in ipProperties.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
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
