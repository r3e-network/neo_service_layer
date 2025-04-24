using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;

namespace NeoServiceLayer.Enclave.Host
{
    /// <summary>
    /// Client for VSOCK communication with the enclave
    /// </summary>
    public class VsockClient
    {
        private readonly ILogger<VsockClient> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsockClient"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public VsockClient(ILogger<VsockClient> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends a message to the enclave
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Response from the enclave</returns>
        public async Task<byte[]> SendMessageAsync(byte[] message)
        {
            try
            {
                using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                var endpoint = new VsockEndPoint(Constants.VsockConfig.EnclaveCid, Constants.VsockConfig.EnclavePort);

                await socket.ConnectAsync(endpoint);

                // Send message length
                var lengthBytes = BitConverter.GetBytes(message.Length);
                await socket.SendAsync(lengthBytes, SocketFlags.None);

                // Send message
                await socket.SendAsync(message, SocketFlags.None);

                // Receive response length
                var responseLengthBytes = new byte[4];
                await socket.ReceiveAsync(responseLengthBytes, SocketFlags.None);
                var responseLength = BitConverter.ToInt32(responseLengthBytes);

                // Receive response
                var response = new byte[responseLength];
                var totalBytesReceived = 0;

                while (totalBytesReceived < responseLength)
                {
                    var bytesReceived = await socket.ReceiveAsync(
                        response.AsMemory(totalBytesReceived, responseLength - totalBytesReceived),
                        SocketFlags.None);

                    totalBytesReceived += bytesReceived;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to enclave");
                throw new EnclaveException("Error sending message to enclave", ex);
            }
        }
    }

    /// <summary>
    /// VSOCK endpoint for communication with the enclave
    /// </summary>
    public class VsockEndPoint : EndPoint
    {
        /// <summary>
        /// VSOCK address family
        /// </summary>
        public const int AF_VSOCK = 40;

        /// <summary>
        /// Local CID (Context ID)
        /// </summary>
        public const int LocalCid = 3;

        /// <summary>
        /// Context ID
        /// </summary>
        public int Cid { get; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VsockEndPoint"/> class
        /// </summary>
        /// <param name="cid">Context ID</param>
        /// <param name="port">Port</param>
        public VsockEndPoint(int cid, int port)
        {
            Cid = cid;
            Port = port;
        }

        /// <summary>
        /// Creates a new <see cref="SocketAddress"/> for this endpoint
        /// </summary>
        /// <returns>Socket address</returns>
        public override SocketAddress Serialize()
        {
            var socketAddress = new SocketAddress((AddressFamily)AF_VSOCK, 16);

            // Set address family
            socketAddress[0] = (byte)(AF_VSOCK & 0xFF);
            socketAddress[1] = (byte)((AF_VSOCK >> 8) & 0xFF);

            // Set port
            socketAddress[2] = (byte)(Port & 0xFF);
            socketAddress[3] = (byte)((Port >> 8) & 0xFF);
            socketAddress[4] = (byte)((Port >> 16) & 0xFF);
            socketAddress[5] = (byte)((Port >> 24) & 0xFF);

            // Set CID
            socketAddress[6] = (byte)(Cid & 0xFF);
            socketAddress[7] = (byte)((Cid >> 8) & 0xFF);
            socketAddress[8] = (byte)((Cid >> 16) & 0xFF);
            socketAddress[9] = (byte)((Cid >> 24) & 0xFF);

            return socketAddress;
        }

        /// <summary>
        /// Creates a new <see cref="VsockEndPoint"/> from a <see cref="SocketAddress"/>
        /// </summary>
        /// <param name="socketAddress">Socket address</param>
        /// <returns>VSOCK endpoint</returns>
        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != (AddressFamily)AF_VSOCK)
                throw new ArgumentException("Invalid address family", nameof(socketAddress));

            if (socketAddress.Size < 16)
                throw new ArgumentException("Invalid socket address size", nameof(socketAddress));

            var port = socketAddress[2] |
                      (socketAddress[3] << 8) |
                      (socketAddress[4] << 16) |
                      (socketAddress[5] << 24);

            var cid = socketAddress[6] |
                     (socketAddress[7] << 8) |
                     (socketAddress[8] << 16) |
                     (socketAddress[9] << 24);

            return new VsockEndPoint(cid, port);
        }

        /// <summary>
        /// Returns a string representation of this endpoint
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"vsock://{Cid}:{Port}";
        }
    }
}
