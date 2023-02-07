using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Profiling;

namespace RigidCubes
{
    public delegate void OnReceive(ArraySegment<byte> buffer);
    public delegate void OnError(Exception ex);

    public class BufferPool
    {
        int m_bufferSize;
        ConcurrentQueue<byte[]> m_bufferPool = new ConcurrentQueue<byte[]>();

        public BufferPool(int bufferSize = 1024)
        {
            m_bufferSize = bufferSize;
        }

        public byte[] GetOrCreate()
        {
            if (m_bufferPool.TryDequeue(out var buffer))
            {
                return buffer;
            }
            else
            {
                UnityEngine.Debug.Log($"new byte[{m_bufferSize}]");
                return new byte[m_bufferSize];
            }
        }

        public void ReleaseBuffer(byte[] buffer)
        {
            m_bufferPool.Enqueue(buffer);
        }
    }

    public class UdpState
    {
        public readonly BufferPool BufferPool;
        public readonly OnReceive OnReceive;
        public readonly OnError OnError;
        bool m_canceled = false;
        public bool IsCanceled => m_canceled;
        public UdpState(BufferPool pool, OnReceive onReceive, OnError onError)
        {
            BufferPool = pool;
            OnReceive = onReceive;
            OnError = onError;
        }
        public void Cancel()
        {
            m_canceled = true;
        }
    }

    public class UdpSocketStream : IDisposable
    {
        Socket m_socket;
        EndPoint m_senderRemote;
        UdpState m_udpState;
        int m_count;

        struct State
        {
            readonly OnReceive OnReceive;
            public byte[] Buffer;

            public State(OnReceive onReceive, byte[] buffer)
            {
                OnReceive = onReceive;
                Buffer = buffer;
            }

            public void InvokeOnReceive(int readSize)
            {
                OnReceive(new ArraySegment<byte>(Buffer, 0, readSize));
            }
        }

        public UdpSocketStream(int port, UdpState state)
        {
            m_udpState = state;

            try
            {
                var ep = new IPEndPoint(IPAddress.Any, port);
                m_socket = new Socket(ep.Address.AddressFamily,
                    SocketType.Dgram,
                    ProtocolType.Udp);

                // Creates an IPEndPoint to capture the identity of the sending host.
                var sender = new IPEndPoint(IPAddress.Any, 0);
                m_senderRemote = (EndPoint)sender;

                // Binding is required with ReceiveFrom calls.
                m_socket.Bind(ep);

                BeginRead();
            }
            catch (Exception ex)
            {
                state.OnError(ex);
            }
        }

        public void Dispose()
        {
            var socket = m_socket;
            m_socket = null;
            if (socket != null)
            {
                socket.Close();
            }
        }

        Socket GetSocket()
        {
            if (m_udpState.IsCanceled)
            {
                return null;
            }
            return m_socket;
        }

        void OnRead(IAsyncResult ar)
        {
            Profiler.BeginSample("SocketStream AsyncCallback");
            var socket = GetSocket();
            if (socket == null)
            {
                // disposed
                return;
            }
            var s = (State)ar.AsyncState;
            try
            {
                var readSize = socket.EndReceiveFrom(ar, ref m_senderRemote);
                if (readSize == 0)
                {
                    m_udpState.OnError(new ArgumentNullException());
                    m_udpState.BufferPool.ReleaseBuffer(s.Buffer);
                    return;
                }
                s.InvokeOnReceive(readSize);
                // next
                BeginRead();
            }
            catch (Exception ex)
            {
                m_udpState.BufferPool.ReleaseBuffer(s.Buffer);
                m_udpState.OnError(ex);
            }
            Profiler.EndSample();
        }

        void BeginRead()
        {
            var socket = GetSocket();
            if (socket == null)
            {
                // disposed
                return;
            }

            // get or create buffer
            byte[] buffer = m_udpState.BufferPool.GetOrCreate();

            socket.BeginReceiveFrom(buffer, 0, buffer.Length, default, ref m_senderRemote, OnRead,
                new State(m_udpState.OnReceive, buffer));
        }
    }
}
