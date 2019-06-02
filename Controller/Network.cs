// Written by Christopher Jones and John Jacobson

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// This delegate is the "bridge" between the Network Controller and the Game Controller.
    /// It is defined in the Game Controller and called when new data has arrived and needs to
    /// be processed.
    /// </summary>
    /// <param name="ss"></param>
    public delegate void callMeDelegate(SocketState ss);

    /// <summary>
    /// This class opens the sockets between the client and the server and handles sending and
    /// receiving data.
    /// </summary>
    public static class Network
    {
        public const int DEFAULT_PORT = 11000;

        /// <summary>
        /// Attempts to connect to the server via a provided hostname. 
        /// Saves the callMe function in a socket state object for use when data arrives.
        /// </summary>
        /// <param name="callMe"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static Socket ConnectToServer(callMeDelegate callMe, string hostname)
        {
            IPAddress ipAddress;
            Socket socket;

            MakeSocket(hostname, out socket, out ipAddress);
            SocketState ss = new SocketState(socket, -1, callMe);
            ss.theSocket.BeginConnect(ipAddress, DEFAULT_PORT, ConnectedCallback, ss);

            return socket;
        }

        /// <summary>
        /// Creates a Socket object for the given host string
        /// </summary>
        /// <param name="hostName">The host name or IP address</param>
        /// <param name="socket">The created Socket</param>
        /// <param name="ipAddress">The created IPAddress</param>
        public static void MakeSocket(string hostName, out Socket socket, out IPAddress ipAddress)

        {
            ipAddress = IPAddress.None;
            socket = null;

            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;

                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }

                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        throw new ArgumentException("Invalid address");
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                // Disable Nagle's algorithm - can speed things up for tiny messages,
                // such as for a game
                socket.NoDelay = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create socket. Error occured: " + e);
                throw new ArgumentException("Invalid address");
            }

        }


        /// <summary>
        /// Calls the callMe function once a connection with the server is established
        /// </summary>
        /// <param name="stateAsArObject"></param>
        private static void ConnectedCallback(IAsyncResult stateAsArObject)
        {

            SocketState ss = (SocketState)stateAsArObject.AsyncState;

            try
            {
                // Complete the connection.
                ss.theSocket.EndConnect(stateAsArObject);
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return;
            }

            // Start an event loop to receive data from the server.
            ss.callMe(ss);
        }

        /// <summary>
        /// A helper function called by the client to receive more data
        /// </summary>
        /// <param name="state"></param>
        public static void GetData(SocketState ss)
        {
            // Start listening for more parts of a message, or more new messages
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
        }

        /// <summary>
        /// Checks to see how much data has arrived. If there is no data, the connection is closed.
        /// If greater than zero, this method gets the SocketState object out of the IAsyncResult and the
        /// callMe function.
        /// </summary>
        /// <param name="stateAsArObject"></param>
        private static void ReceiveCallback(IAsyncResult stateAsArObject)
        {
            SocketState ss = (SocketState)stateAsArObject.AsyncState;

            try
            {
                int bytesRead = ss.theSocket.EndReceive(stateAsArObject);

                // If the socket is still open
                if (bytesRead > 0)
                {
                    string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);

                    // Append the received data to the growable buffer.
                    // It may be an incomplete message, so we need to start building it up piece by piece
                    ss.sb.Append(theMessage);

                    ss.callMe(ss);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client disconnected.");
            }
        }

        /// <summary>
        /// This function allows a program to send data over a socket by
        /// converting the data into bytes and then sending them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {
            // Append a newline, since that is our protocol's terminating character for a message.
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            try
            {
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, socket);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Error caught with client disconnect:" + e.Message);
            }
        }

        /// <summary>
        /// This function assists the Send function. It extracts the Socket 
        /// out of the IAsyncResult, and then call socket.EndSend.
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult stateAsArObject)
        {
            try
            {
                Socket socket = (Socket)stateAsArObject.AsyncState;

                // Conclude the send operation so the socket is happy.
                socket.EndSend(stateAsArObject);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Error caught with client disconnect:" + e.Message);
            }
        }

        public static void ServerAwaitingClientLoop(callMeDelegate callMe)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            listener.Start();

            ConnectionState cState = new ConnectionState(callMe, listener);

            listener.BeginAcceptSocket(AcceptNewClient, cState);

        }

        private static void AcceptNewClient(IAsyncResult ar)
        {
            ConnectionState cState = (ConnectionState)ar.AsyncState;

            Socket sock = cState.listener.EndAcceptSocket(ar);

            SocketState ss = new SocketState(sock, -1, cState.callMe);
            cState.callMe(ss);

            cState.listener.BeginAcceptSocket(AcceptNewClient, cState);
        }
    }
}
