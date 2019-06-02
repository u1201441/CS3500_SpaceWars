// Written by Christopher Jones and John Jacobson

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// This class is used to store information and instructions in the Network
    /// Controller static class.
    /// </summary>
    public class SocketState
    {

        public callMeDelegate callMe;
        public Socket theSocket;
        public int id;

        // This is the buffer where we will receive data from the socket
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        /// <summary>
        /// The socket state constructor that initializes what socket, id, and delegate
        /// will be passed around in the Network Controller and Game Controller.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="_id"></param>
        /// <param name="callMeParameter"></param>
        public SocketState(Socket s, int _id, callMeDelegate callMeParameter)
        {
            theSocket = s;
            id = _id;
            callMe = callMeParameter;
            sb = new StringBuilder();
            messageBuffer = new byte[1024];
        }
    }
}