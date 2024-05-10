using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Server : MonoBehaviour
{
    private TcpListener _server;
    private Thread _serverThread;
    private bool _isRunning = false;
    private AutoResetEvent _resetEvent;

    private Team2[] teams = new Team2[2];
    void Start()
    {
        _resetEvent = new AutoResetEvent(false);
        StartServer();
    }
    
    private void Update()
    {
        _resetEvent.Set();
    }
    private void StartServer()
    {
        try
        {
            _server = new TcpListener(IPAddress.Any, 7777);
            _server.Start();
            Debug.Log("Server started on port 7777");
            _isRunning = true;

            _serverThread = new Thread(ListenForClients);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Error starting server: " + e.Message);
        }
    }
    
    
    private void ListenForClients()
    {
        _resetEvent.WaitOne();
        try
        {
            while (_isRunning)
            {
                TcpClient client = _server.AcceptTcpClient();
                Debug.Log("Client connected from: " + client.Client.RemoteEndPoint);

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        catch (SocketException)
        {
            
        }
    }
    
    void HandleClient(TcpClient client)
    {
        _resetEvent.WaitOne();
        try
        {
            // Add code here to handle communication with the client
            // For example, reading from and writing to the client's network stream
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling client: " + e.Message);
        }
        finally
        {
            client.Close(); // Ensure client connection is closed when done
        }
    }
    
    private void OnDestroy()
    {
        StopServer();
    }

    private void StopServer()
    {
        _isRunning = false;

        if (_server != null)
        {
            _server.Stop();
            Debug.Log("Server stopped");
        }
    }
}
