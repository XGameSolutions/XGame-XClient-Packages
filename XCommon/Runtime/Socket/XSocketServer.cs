
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace XCommon.Runtime
{
    public class XSocketServer<T> where T : XSocket, new()
    {
        private string m_IP;
        private int m_Port;
        private int m_MaxConnection = 10;
        private XSocket m_Server;
        private List<T> m_Clients = new List<T>();
        public Action<T> OnClientConnectCallback { private get; set; }
        public Action<int> OnClientCloseCallback { private get; set; }
        public Action<int, string> OnClientReceiveCallback { private get; set; }

        public XSocketServer(string ip, int port, int maxConnection = 10)
        {
            m_IP = ip;
            m_Port = port;
            m_MaxConnection = maxConnection;
            m_Clients.Clear();
            for (int i = 0; i < m_MaxConnection; i++)
            {
                var client = new T();
                client.OnReceiveCallback = OnClientReceive;
                client.OnCloseCallback = OnClientClose;
                client.Index = i;
                client.Name = "Conn-" + i;
                m_Clients.Add(client);
            }
        }

        public void Start()
        {
            m_Server = new XSocket();
            m_Server.Name = "Server";
            m_Server.Listen(m_IP, m_Port, m_MaxConnection, OnAccept);
            Debug.Log("XSocketServer Start");
        }

        public void Close()
        {
            m_Server.Close();
            m_Server = null;
        }

        public void SendToClient(int clientIndex, string content)
        {
            if (clientIndex >= 0 && clientIndex < m_Clients.Count)
            {
                var client = m_Clients[clientIndex];
                client.Send(content);
            }
        }

        public void SendFileToClient(int clientIndex, string filePath)
        {
            if (clientIndex >= 0 && clientIndex < m_Clients.Count)
            {
                var client = m_Clients[clientIndex];
                client.SendFile(filePath);
            }
        }

        private int NewIndex()
        {
            if (m_Clients.Count == 0) return -1;
            for (int i = 0; i < m_MaxConnection; i++)
            {
                if (m_Clients[i] == null)
                {
                    m_Clients[i] = new T();
                    return i;
                }
                else if (!m_Clients[i].IsUse)
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnAccept(IAsyncResult result)
        {
            if (m_Server == null) return;
            try
            {
                var server = result.AsyncState as Socket;
                var socket = server.EndAccept(result);
                var index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Debug.LogError("XSocketServer OnAccept ERROR: connect max.");
                }
                else
                {
                    var conn = m_Clients[index];
                    conn.Init(socket);
                    OnClientConnectCallback?.Invoke(conn);
                    conn.BeginReceive();
                }
                m_Server.BeginAccept(OnAccept);
            }
            catch (Exception e)
            {
                Debug.LogError("XSocketServer OnAccept ERROR:" + e.Message);
            }
        }

        private void OnClientReceive(XSocket socket, byte[] buffer, int len)
        {
            var content = System.Text.Encoding.UTF8.GetString(buffer, 0, len);
            Debug.Log("[server] receive:" + socket.Index + "," + content);
            OnClientReceiveCallback?.Invoke(socket.Index, content);
        }

        private void OnClientClose(XSocket socket)
        {
            Debug.Log("[server] close:" + socket.Index);
            OnClientCloseCallback?.Invoke(socket.Index);
        }
    }
}