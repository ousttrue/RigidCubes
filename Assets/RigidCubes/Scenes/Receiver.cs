using System;
using System.Collections.Concurrent;
using UnityEngine;

public class Receiver : MonoBehaviour
{
    public int m_port = 54345;
    RigidCubes.BufferPool m_pool = new RigidCubes.BufferPool();
    RigidCubes.UdpSocketStream m_udp;
    ConcurrentQueue<object> m_queue = new ConcurrentQueue<object>();

    int m_received;
    int m_error;

    void OnEnable()
    {
        Debug.Log("Enable");
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
        m_queue.Enqueue(ex);
    }

    void OnGUI()
    {
        GUILayout.Label($"received: {m_received}");
    }

    void Update()
    {
        while (m_queue.TryDequeue(out var item))
        {
            switch (item)
            {
                case ArraySegment<byte> buffer:
                    ++m_received;
                    // Debug.Log($"receive: {m_received}");
                    m_pool.ReleaseBuffer(buffer.Array);
                    break;

                case Exception ex:
                    ++m_error;
                    Debug.LogException(ex);
                    break;
            }
        }
    }
}
