using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Interfaces
{
    public abstract class NetworkInterface
    {
        public const bool DEBUG = true;
        public const bool VERBOSE = false;

        private TcpClient client;
        private Socket socket;
        private String name;

        private List<INetworkObserver> observers;

        public NetworkInterface(String name)
        {
            this.name = name;
            this.observers = new List<INetworkObserver>();
        }

        #region Observers
        public void RegisterObserver(INetworkObserver observer) {
            observers.Add(observer);
        }
        #endregion

		public bool Connected {
			get { return client != null && client.Client != null && client.Connected; }
		}

        #region Connectivity
		/// <summary>
		/// Connect to a local socket on the given port number.
		/// </summary>
		/// 
		/// <param name="port">Port to try and connect to.</param>
		/// 
		/// <returns>True if connected successfully, false if otherwise.</returns>
		public bool Connect(int port)
		{
			return Connect("localhost", port);
        }

        /// <summary>
        /// Connect to a TCP server with the given hostname and port number.
        /// </summary>
        /// 
		/// <param name="host">Hostname of tcp server.</param>
        /// <param name="port">Port to try and connect to.</param>
        /// 
        /// <returns>True if connected successfully, false if otherwise.</returns>
        public bool Connect(String host, int port)
        {
            try
            {
                client = new TcpClient(host, port);
                socket = client.Client;
            }
            catch (SocketException)
            {
                client = null;
            }

            if (client == null || !client.Connected)
            {
                Log(String.Format("Unable to connect to port {0}", port));
                return false;
            }
            else
            {
                (new Thread(Read)).Start();
                Log(String.Format("Connected on port {0}", port));
                return true;
            }
        }

        /// <summary>
        /// Disconnect from the socket and close the TCP client connection.
        /// </summary>
        public void Disconnect()
        {
            if (socket != null)
                socket.Close();

            if (client != null)
                client.Close();

            Log("Disconnected");
        }
        #endregion

        #region Messages
        /// <summary>
        /// Send a message over the socket, encoded as UTF-8. Adds a newline
        /// to the end of the message.
        /// </summary>
        public void Send(String message)
        {
            message += "\n";

            try {
                if (Connected)
                {
                    socket.Send(Encoding.UTF8.GetBytes(message), message.Length, SocketFlags.None);

                    if (DEBUG)
                        Log(String.Format("Sent: \"{0}\"", message.Trim()));
                }
            } catch (System.ObjectDisposedException) {

            } catch (System.Net.Sockets.SocketException) {

            }
        }

        /// <summary>
        /// Blocking reads from the TCP stream.
        /// </summary>
        private void Read()
        {
            // Buffer to store the response bytes.
            Byte[] data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Socket end point
            EndPoint endPoint = socket.RemoteEndPoint;

            while (true) {
                int bytes = 0;

                try {
                    bytes = socket.ReceiveFrom(data, ref endPoint);
                } catch (System.Net.Sockets.SocketException) {
                    break;
                }

                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes).Trim();    

                if (responseData.Equals("Quit") || responseData.Length == 0)
                    break;

                foreach (INetworkObserver observer in observers) {
                    observer.MessageReceived(responseData);
                }
            }
        }
        #endregion

        #region Utility
        private void Log(string message)
        {
            if (DEBUG)
            {
                if (VERBOSE)
                    Console.WriteLine("NetworkInterface: {0}", message);

                lock (this)
                {
                    FileManager.Append(String.Format("{0}-Log.txt", name), message);
                }
            }
        }
        #endregion
    }
}
