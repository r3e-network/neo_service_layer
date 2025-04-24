using System;
using System.Net;
using System.Net.Sockets;

namespace NeoServiceLayer.Enclave.Enclave
{
    /// <summary>
    /// Represents a VSOCK endpoint for communication with the parent instance
    /// </summary>
    public class VsockEndPoint : EndPoint
    {
        /// <summary>
        /// The local CID (Context ID)
        /// </summary>
        public const uint LocalCid = 3;

        /// <summary>
        /// The parent CID (Context ID)
        /// </summary>
        public const uint ParentCid = 2;

        /// <summary>
        /// The CID (Context ID)
        /// </summary>
        public uint Cid { get; }

        /// <summary>
        /// The port
        /// </summary>
        public uint Port { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VsockEndPoint"/> class
        /// </summary>
        /// <param name="cid">CID (Context ID)</param>
        /// <param name="port">Port</param>
        public VsockEndPoint(uint cid, uint port)
        {
            Cid = cid;
            Port = port;
        }

        /// <summary>
        /// Creates a new <see cref="SocketAddress"/> instance from this endpoint
        /// </summary>
        /// <returns>A new <see cref="SocketAddress"/> instance</returns>
        public override SocketAddress Serialize()
        {
            // Create a new socket address with the AF_VSOCK address family (40)
            var socketAddress = new SocketAddress(AddressFamily.Unix, 8);

            // Set the CID and port
            var cidBytes = BitConverter.GetBytes(Cid);
            var portBytes = BitConverter.GetBytes(Port);

            // Copy the bytes to the socket address
            for (int i = 0; i < 4; i++)
            {
                socketAddress[i + 2] = cidBytes[i];
                socketAddress[i + 6] = portBytes[i];
            }

            return socketAddress;
        }

        /// <summary>
        /// Creates a new endpoint from a <see cref="SocketAddress"/>
        /// </summary>
        /// <param name="socketAddress">The socket address</param>
        /// <returns>A new endpoint</returns>
        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != AddressFamily.Unix)
            {
                throw new ArgumentException("Invalid address family", nameof(socketAddress));
            }

            if (socketAddress.Size < 8)
            {
                throw new ArgumentException("Invalid socket address size", nameof(socketAddress));
            }

            // Extract the CID and port bytes
            byte[] cidBytes = new byte[4];
            byte[] portBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                cidBytes[i] = socketAddress[i + 2];
                portBytes[i] = socketAddress[i + 6];
            }

            var cid = BitConverter.ToUInt32(cidBytes, 0);
            var port = BitConverter.ToUInt32(portBytes, 0);

            return new VsockEndPoint(cid, port);
        }

        /// <summary>
        /// Returns a string representation of this endpoint
        /// </summary>
        /// <returns>A string representation of this endpoint</returns>
        public override string ToString()
        {
            return $"vsock://{Cid}:{Port}";
        }
    }
}
