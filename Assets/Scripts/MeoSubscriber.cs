using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;

namespace Meocap.DataSource
{
    
  
    public class MeoSubscriber : MonoBehaviour
    {
        // Start is called before the first frame update
        [Header("设置该数据源绑定的Actor实例")]
        public Perform.MeoActor actor;
        [Header("发布端IP地址")]
        public string address = "127.0.0.1";
        [Header("发布端端口号")]
        public short port = 14999;
        [Header("当前帧ID")]
        public int frameId = 0;
        [Header("将Actor骨架同步至客户端")]
        public bool syncBonePos = true;
        [Header("启动时自动连接")]
        public bool connectOnStart = true;


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

        async private void StartReceivingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (this.sock == null)
                {
                    continue;
                }
                var frame = await sock.ReceiveFrameAsync();
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
