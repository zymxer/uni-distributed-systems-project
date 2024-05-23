using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    private TcpClient _client;
    private bool _isRunning;
    private Thread _readThread;
    private Thread _writeThread;

    private int _drawnIndex;
    
    private ClientData _receivedData;
    private List<ClientData> _dataToSend;
    
    private AutoResetEvent _serverResetEvent;

    public ClientData ReceivedData => _receivedData;
    public List<ClientData> DataToSend => _dataToSend;
    public TcpClient Client => _client;
    public int DrawnIndex => _drawnIndex;
    
    public void Initialize(TcpClient client, AutoResetEvent resetEvent, int drawnIndex)
    {
        _client = client;
        _serverResetEvent = resetEvent;
        _isRunning = true;

        _drawnIndex = drawnIndex;
        
        _readThread = new Thread(ReadThread);
        _writeThread = new Thread(WriteThread);
        
        _readThread.Start();
        _writeThread.Start();
    }

    private void ReadThread()
    {
        while (_isRunning)
        {
           
            //...
        }
    }
    
    private void WriteThread()
    {
        while (_isRunning)
        {
            _serverResetEvent.WaitOne();
            //...
            
            
            _dataToSend.Clear();
        }
    }

    public void Shutdown()
    {
        _isRunning = false;
        _readThread.Abort();
        _writeThread.Abort();
        _client.Close();
    }
}
