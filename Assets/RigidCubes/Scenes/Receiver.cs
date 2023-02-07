using System;
using System.Collections.Concurrent;
using UnityEngine;

public class Receiver : MonoBehaviour
{
    public int m_bufferSize = 2048;
    public int m_port = 54345;
    public UnityEngine.Events.UnityEvent<ArraySegment<byte>> m_onMessage;

    RigidCubes.BufferPool m_pool;
    RigidCubes.UdpSocketStream m_udp;
    ConcurrentQueue<ArraySegment<byte>> m_queue = new ConcurrentQueue<ArraySegment<byte>>();
    Exception m_ex;

    int m_received;
    int m_error;

    void OnEnable()
    {
        Debug.Log("Enable");
        m_pool = new RigidCubes.BufferPool(m_bufferSize);
        m_udp = new RigidCubes.UdpSocketStream(m_port, new RigidCubes.UdpState(m_pool, OnReceive, OnError));
    }

    void OnDisable()
    {
        m_udp.Dispose();
        Debug.Log("Disable");
    }

    void OnReceive(ArraySegment<byte> buffer)
    {
        m_queue.Enqueue(buffer);
    }

    void OnError(Exception ex)
    {
        m_ex = ex;
    }

    void OnGUI()
    {
        // GUILayout.Label($"received: {m_received}");
        if (m_ex != null)
        {
            GUILayout.Label($"exception: {m_ex}");
        }
    }

    void Update()
    {
        while (m_queue.TryDequeue(out var buffer))
        {
            ++m_received;
            // Debug.Log($"receive: {m_received}");
            m_onMessage.Invoke(buffer);

            m_pool.ReleaseBuffer(buffer.Array);
        }
    }
}
