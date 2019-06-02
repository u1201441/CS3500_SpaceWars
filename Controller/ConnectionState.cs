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
    /// This class is used to store information and instructions in the Network
    /// Controller static class.
    /// </summary>
    public class ConnectionState
    {

        public callMeDelegate callMe;
        public TcpListener listener;

        /// <summary>
        /// The socket state constructor that initializes what socket, id, and delegate
        /// will be passed around in the Network Controller and Game Controller.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="_id"></param>
        /// <param name="callMeParameter"></param>
        public ConnectionState(callMeDelegate callMeParameter, TcpListener _listener)
        {
            callMe = callMeParameter;
            listener = _listener;
        }
    }
}