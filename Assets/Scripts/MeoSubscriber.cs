using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Meocap.DataSource
{
    
  
    public class MeoSubscriber : MonoBehaviour
    {
        // Start is called before the first frame update
        [Header("设置要驱动的MeoActor实例")]
        public Perform.MeoActor actor;
        [Header("数据来源IP地址")]
        public string address = "127.0.0.1";
        [Header("数据端口号")]
        public short port = 14999;
        [Header("Frame ID")]
        public int frameId = 0;
        [Header("将骨架同步到客户端")]
        public bool syncBonePos = true;
        [Header("在启动时连接")]
        public bool connectOnStart = true;

        private Thread _receiveThread;
        Meocap.SDK.MeoFrame frame;
        private volatile bool has_sync_skel = false;
        private Meocap.SDK.MeocapSocket sock = null;
        private CancellationTokenSource cancelSource;
        private ConcurrentQueue<SDK.MeoFrame> frames = new();

        void Start()
        {
            if (this.connectOnStart)
            {
                this.Connect();
            }

        }

        void Connect()
        {
            string[] ip_addr = address.Split('.');
            if (ip_addr.Length != 4)
            {
                Debug.LogError("MeoSubscriber: IPAddress Format Error");
                return;
            }

            Meocap.SDK.Addr addr = new()
            {
                a = byte.Parse(ip_addr[0]),
                b = byte.Parse(ip_addr[1]),
                c = byte.Parse(ip_addr[2]),
                d = byte.Parse(ip_addr[3]),
                port = (ushort)port
            };

            this.sock = new SDK.MeocapSocket(addr);
            cancelSource = new CancellationTokenSource();
            StartReceivingLoop(cancelSource.Token);
        }

        private void StartReceivingLoop(CancellationToken token)
        {
            _receiveThread = new Thread(() => ReceivingLoop(token));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void ReceivingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (this.sock == null)
                {
                    continue;
                }
                var frame = sock.ReceiveFrame();
                if (frame.HasValue)
                {
                    frames.Enqueue(frame.Value);
                }
            }
        }

        private void Update()
        {
            bool has_new_data = false;
            while (frames.TryDequeue(out SDK.MeoFrame new_frame))
            {
                frame = new_frame;
                this.frameId = frame.frame_id;
                has_new_data = true;
            }
            if (has_new_data)
            {
                if (this.syncBonePos && !this.has_sync_skel)
                {
                    if (this.actor && this.sock != null)
                    {
                        var skel = this.actor.SyncBonePosToClient();
                        sock.SetSkeleton(skel, frame.src);
                        Debug.Log("Set Skel to MeocapClient");
                        this.has_sync_skel = true;
                    }
                }
                if (this.actor != null)
                {
                    this.actor.Perform(frame);
                }
            }
        }

        private void OnDestroy()
        {
            cancelSource.Cancel();
            if (this.sock != null)
            {
                sock.Dispose();
            }
        }
    }

}
