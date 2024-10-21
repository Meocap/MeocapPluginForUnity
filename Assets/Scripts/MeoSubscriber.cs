using System.Collections;
using System.Collections.Generic;
using NetMQ.Sockets;
using NetMQ;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;
using System;
using System.Collections.Concurrent;
using System.Threading;

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
        public bool syncBonePos = false;
        [Header("Command Server IP地址")]
        public string commandAddress = "127.0.0.1";
        [Header("Command Servr 端口号")]
        public short commandPort = 15999;


        MeocapSdk.MeoFrame frame;
        private IntPtr sock_ptr = IntPtr.Zero;
        private volatile bool has_sync_skel = false;

        private ConcurrentQueue<MeocapSdk.MeoFrame> frames = new ();

        private bool running_recv_msg = true;
        private Thread messageThread;
        void Start()
        {
            AsyncIO.ForceDotNet.Force();
            string[] ip_addr = address.Split('.');
            if(ip_addr.Length != 4 ) {
                Debug.LogError("MeoSubscriber: IPAddress Format Error");
                return;
            }
            MeocapSdk.Addr addr = new() { 
                a = byte.Parse(ip_addr[0]),
                b = byte.Parse(ip_addr[1]),
                c = byte.Parse(ip_addr[2]),
                d = byte.Parse(ip_addr[3]),
                port = (ushort)port
            };

            var ret = MeocapSdk.Api.meocap_bind_listening_addr(ref addr);
            
            if(ret.err.ty == MeocapSdk.ErrorType.None)
            {
                this.sock_ptr = ret.socket;
                messageThread = new Thread(ReceiveMessages);
                messageThread.Start();
            }



        }

        private void ReceiveMessages()
        {
            while (running_recv_msg)
            {
                var frame = new MeocapSdk.MeoFrame();
                var ret = MeocapSdk.Api.meocap_recv_frame(this.sock_ptr, ref frame);
                if (ret.ty == MeocapSdk.ErrorType.None)
                {
                    frames.Enqueue(frame);
                }
                Thread.Sleep(10);
            }
        }



        private void Update()
        {
            bool has_new_data = false;
            while (frames.TryDequeue(out MeocapSdk.MeoFrame new_frame))
            {
                frame = new_frame;
                this.frameId = frame.frame_id;
                has_new_data = true;
            }
            if(has_new_data)
            {
                if (this.syncBonePos && !this.has_sync_skel)
                {
                    if (this.actor && this.sock_ptr != IntPtr.Zero)
                    {
                        var skel = this.actor.SyncBonePosToClient();
                        var ret_command = MeocapSdk.Api.meocap_command_set_skel(this.sock_ptr, ref frame.src, ref skel);
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
            running_recv_msg = false;
            if(this.sock_ptr != IntPtr.Zero)
            {
                MeocapSdk.Api.meocap_clean_up(this.sock_ptr);
            }
        }
    }

}
