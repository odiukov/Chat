using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sever
{
    class Server
    {
        private Socket _serverSocket;
        private int _port;
        public Server(int port) { _port = port; }
        private void SetupServerSocket()
        {
            // Получаем информацию о локальном компьютере
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint myEndpoint = new IPEndPoint(ipAddress, _port);

            // Создаем сокет, привязываем его к адресу
            // и начинаем прослушивание
            _serverSocket = new Socket(myEndpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Starting server on: " + ipAddress + ":" + _port);
            _serverSocket.Bind(myEndpoint);
            _serverSocket.Listen((int)
                SocketOptionName.MaxConnections);
        }

        private class ConnectionInfo
        {
            public Socket Socket;
            public byte[] Buffer;
        }

        private List<ConnectionInfo> _connections =
            new List<ConnectionInfo>();

        public void Start()
        {
            SetupServerSocket();
            for (int i = 0; i < 10; i++)
            {
                _serverSocket.BeginAccept(new
                    AsyncCallback(AcceptCallback), _serverSocket);
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            ConnectionInfo connection = new ConnectionInfo();
            try
            {
                // Завершение операции Accept
                Socket s = (Socket)result.AsyncState;

                connection.Socket = s.EndAccept(result);
                connection.Buffer = new byte[255];
                lock (_connections) _connections.Add(connection);
                Console.WriteLine("New connection. Count of clients = " + _connections.Count);
                // Начало операции Receive и новой операции Accept
                connection.Socket.BeginReceive(connection.Buffer,
                    0, connection.Buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback),
                    connection);
                _serverSocket.BeginAccept(new AsyncCallback(
                    AcceptCallback), result.AsyncState);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " +
                    exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            ConnectionInfo connection =
                (ConnectionInfo)result.AsyncState;
            try
            {
                int bytesRead =
                    connection.Socket.EndReceive(result);
                if (bytesRead != 0)
                {
                    lock (_connections)
                    {
                        foreach (ConnectionInfo conn in
                            _connections)
                        {
                            if (connection != conn)
                            {
                                Console.WriteLine("Send {0} bytes", bytesRead);
                                conn.Socket.Send(connection.Buffer,
                                    bytesRead, SocketFlags.None);
                            }
                        }
                    }
                    connection.Socket.BeginReceive(
                        connection.Buffer, 0,
                        connection.Buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback),
                        connection);
                }
                else CloseConnection(connection);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " +
                    exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc);
            }
        }

        private void CloseConnection(ConnectionInfo ci)
        {
            ci.Socket.Close();
            lock (_connections) _connections.Remove(ci);
            Console.WriteLine("Disconnect. Count of clients = " + _connections.Count);
        }
    }
}
