using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text connectionResult;
    private Team2 _team;
    private TcpClient client;
    private Thread receiveThread;
    private Thread sendThread;
    private AutoResetEvent _resetEvent;
    void Start()
    {
        _resetEvent = new AutoResetEvent(false);
    }

    private void Update()
    {
        _resetEvent.Set();
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(ipInputField.text), 10001);
            connectionResult.text = "Connected successfully";
        }
        catch (Exception e)
        {
            connectionResult.text = "Error while connecting";
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

