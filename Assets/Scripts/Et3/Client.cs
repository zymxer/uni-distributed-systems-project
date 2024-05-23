using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
public class Client : MonoBehaviour
{
    [SerializeField] private Team2 _team2;
    private TcpClient client;
    private Thread receiveThread;
    private Thread sendThread;
    private AutoResetEvent _resetEvent;
    void Start()
    {
        _resetEvent = new AutoResetEvent(false);
        ConnectToServer();
    }

    private void Update()
    {
        _resetEvent.Set();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(IPAddress.Loopback, 7777);
            Debug.Log("Connected to server");
            
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            sendThread = new Thread(SendData);
            sendThread.IsBackground = true;
            sendThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }
    
    
    void ReceiveData()
    {
        _resetEvent.WaitOne();
        try
        {
            byte[] buffer = new byte[1024];
            while (client.Connected)
            {
                
            }
        }
        catch (Exception e)
        {
            
        }
    }

    void SendData()
    {
        _resetEvent.WaitOne();
        try
        {
            while (client.Connected)
            {
                byte[] data = Team2.Serialize(_team2);
                client.GetStream().Write(data, 0, data.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data: " + e.Message);
        }
    }

    void OnDestroy()
    {
        DisconnectFromServer();
    }

    void DisconnectFromServer()
    {
        if (client != null)
        {
            client.Close();
            Debug.Log("Disconnected from server");
        }
    }
    
}

