
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace XCommon.Runtime
{
    public class XSocket
    {
        private const int BUF_SIZE = 1024 * 64;
        private string m_Name;
        private Socket m_Socket;
        private string m_IP;
        private int m_Port;
        private byte[] m_Buffer = new byte[BUF_SIZE];
        public Action<bool, string> OnConnectCallback { private get; set; }
        public Action<XSocket> OnSendCallback { private get; set; }
        public Action<XSocket> OnCloseCallback { private get; set; }
        public Action<XSocket, byte[], int> OnReceiveCallback { private get; set; }
        public Action<XSocket> OnAcceptCallback { private get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public bool IsUse { get; private set; }
        public int BufferCount { get; private set; }
        public int BufferRemain { get { return BUF_SIZE - BufferCount; } }
        public DateTime ConnectTime { get; private set; }

        public XSocket()
        {
        }

        public void Init(Socket socket)
        {
            m_Socket = socket;
            IsUse = true;
            ConnectTime = DateTime.Now;
        }

        public void Listen(string ip, int port, int maxConn, AsyncCallback onAccept = null)
        {
            m_IP = ip;
            m_Port = port;
            IsUse = true;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var addr = IPAddress.Parse(m_IP);
            var endPoint = new IPEndPoint(addr, m_Port);
            m_Socket.Bind(endPoint);
            m_Socket.Listen(maxConn);
            m_Socket.SendBufferSize = BUF_SIZE;
            m_Socket.ReceiveBufferSize = BUF_SIZE;
            m_Socket.BeginAccept(onAccept == null ? OnAccept : onAccept, m_Socket);
        }

        public void Connect(string ip, int port)
        {
            m_IP = ip;
            m_Port = port;
            IsUse = true;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.SendBufferSize = BUF_SIZE;
            m_Socket.ReceiveBufferSize = BUF_SIZE;
            var addr = IPAddress.Parse(m_IP);
            var endPoint = new IPEndPoint(addr, m_Port);
            try
            {
                m_Socket.BeginConnect(endPoint, OnConnect, null);
            }
            catch (Exception e)
            {
                OnConnectCallback?.Invoke(false, e.Message);
            }
        }

        public void BeginAccept(AsyncCallback onAccept)
        {
            m_Socket.BeginAccept(onAccept, m_Socket);
        }

        public void BeginReceive()
        {
            m_Socket.BeginReceive(m_Buffer, BufferCount, BufferRemain, SocketFlags.None, OnReceive, m_Socket);
        }

        public void BeginSend(byte[] buf)
        {
            try
            {
                SocketLog("Send:" + buf.Length);
                m_Socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, OnSend, m_Socket);
            }
            catch (Exception e)
            {
                SocketLogError("Send ERROR:" + e.Message);
            }
        }

        public bool Send(byte[] buf)
        {
            try
            {
                m_Socket.Send(buf);
                return true;
            }
            catch (SocketException e)
            {
                SocketLogError("Send ERROR:" + e.Message);
                return false;
            }
        }

        public bool Send(string text)
        {
            SocketLog("Send:" + text);
            byte[] tex = System.Text.Encoding.UTF8.GetBytes(text);
            return Send(tex);
        }

        public int Send(byte[] buf, int timeoutMicroSeconds)
        {
            var flag = 0;
            try
            {
                var left = buf.Length;
                var send = 0;
                while (true)
                {
                    if (m_Socket.Poll(timeoutMicroSeconds, SelectMode.SelectWrite) == true)
                    {
                        send = m_Socket.Send(buf, send, left, SocketFlags.None);
                        left -= send;
                        if (left == 0)
                        {
                            flag = 0;
                            break;
                        }
                        else
                        {
                            if (send > 0) continue;
                            else
                            {
                                flag = -2;
                                break;
                            }
                        }
                    }
                    else
                    {
                        flag = -1;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                SocketLogError("Send ERROR:" + e.Message);
                flag = -3;
            }
            return flag;
        }

        public bool SendFile(string filePath, int maxBufferSize = 512, int timeoutMicroSeconds = -1)
        {
            if (!File.Exists(filePath)) return false;
            if (maxBufferSize <= 0) return false;
            bool flag = true;
            SocketLog("SendFile:" + filePath);
            try
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileLen = fs.Length;
                var leftLen = fileLen;
                var readLen = 0;
                byte[] buffer;

                if (fileLen < maxBufferSize)
                {
                    buffer = new byte[fileLen];
                    readLen = fs.Read(buffer, 0, (int)fileLen);
                    flag = Send(buffer);
                }
                else
                {
                    while (leftLen != 0)
                    {
                        if (leftLen < maxBufferSize)
                        {
                            buffer = new byte[leftLen];
                            readLen = fs.Read(buffer, 0, (int)leftLen);
                        }
                        else
                        {
                            buffer = new byte[maxBufferSize];
                            readLen = fs.Read(buffer, 0, maxBufferSize);
                        }
                        flag = Send(buffer);
                        if (!flag)
                        {
                            break;
                        }
                        leftLen -= readLen;
                    }
                }
                fs.Flush();
                fs.Close();
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        public void Close()
        {
            if (IsUse)
            {
                m_Socket.Close();
                IsUse = false;
                SocketLogError("Socket Close");
                OnCloseCallback?.Invoke(this);
            }
        }

        protected virtual void OnConnect(IAsyncResult result)
        {
            SocketLog("OnConnect SUCCESS!");
            ConnectTime = DateTime.Now;
            OnConnectCallback?.Invoke(true, null);
        }

        protected virtual void OnSend(IAsyncResult result)
        {
            SocketLog("OnSend SUCCESS!");
            OnSendCallback?.Invoke(this);
        }

        private void OnReceive(IAsyncResult result)
        {
            var socket = result.AsyncState as Socket;
            if (!socket.Connected || !IsUse) return;
            try
            {
                var count = socket.EndReceive(result);
                if (count <= 0)
                {
                    // SocketLog("Close");
                    // Close();
                    // return;
                }
                else
                {
                    OnReceiveCallback?.Invoke(this, m_Buffer, count);
                }
                m_Socket.BeginReceive(m_Buffer, BufferCount, BufferRemain, SocketFlags.None, OnReceive, m_Socket);
            }
            catch (Exception e)
            {
                SocketLogError(e);
                Close();
            }
        }

        private void OnAccept(IAsyncResult result)
        {
            try
            {
                var server = result.AsyncState as Socket;
                var socket = server.EndAccept(result);
                var conn = new XSocket();
                conn.Init(socket);
                conn.BeginReceive();
                OnAcceptCallback?.Invoke(conn);
                m_Socket.BeginAccept(OnAccept, m_Socket);
            }
            catch (Exception e)
            {
                SocketLogError("XSocketServer OnAccept ERROR:" + e.Message);
            }
        }

        private void SocketLog(string log)
        {
            Debug.LogFormat("[{0}] {1}", Name, log);
        }
        private void SocketLog(object log)
        {
            Debug.LogFormat("[{0}] {1}", Name, log.ToString());
        }

        private void SocketLogError(string log)
        {
            Debug.LogErrorFormat("[{0}] {1}", Name, log);
        }
        private void SocketLogError(object log)
        {
            Debug.LogErrorFormat("[{0}] {1}", Name, log.ToString());
        }
    }
}