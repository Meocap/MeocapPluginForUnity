using System.Collections;
using System.Collections.Generic;
using NetMQ.Sockets;
using NetMQ;
using UnityEngine;
using Meocap;
using System.Text;
using System.Runtime.InteropServices;
using System;

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
        Meocap.MeoFrame frame;
        private ulong sock_ptr;
        private ulong sock_command_ptr;
        

        void Start()
        {
            AsyncIO.ForceDotNet.Force();
            string[] ip_addr = address.Split('.');
            if(ip_addr.Length != 4 ) {
                Debug.LogError("MeoSubscriber: IPAddress Format Error");
                return;
            }
            this.sock_ptr = MeocapSDK.meocap_connect_server_char(byte.Parse(ip_addr[0]), byte.Parse(ip_addr[1]), byte.Parse(ip_addr[2]), byte.Parse(ip_addr[3]), (ushort)port);
            if(this.syncBonePos)
            {
                string[] ip_command_addr = commandAddress.Split(".");
                this.sock_command_ptr = MeocapSDK.meocap_connect_command_server_char(byte.Parse(ip_command_addr[0]), byte.Parse(ip_command_addr[1]), byte.Parse(ip_command_addr[2]), byte.Parse(ip_command_addr[3]), (ushort)commandPort);
                Debug.Log(this.sock_command_ptr);
                if (this.actor && this.sock_command_ptr != 0)
                {
                    var frame = this.actor.SyncBonePosToClient();
                    var ret = MeocapSDK.meocap_command_set_skel(this.sock_command_ptr, ref frame);
                    Debug.Log(ret);

                }
            }
        }

        private void Update()
        {
            int ret = MeocapSDK.meocap_recv_frame(this.sock_ptr, out this.frame);
            if (ret == 0)
            {
                if (actor != null)
                {
                    actor.Perform(this.frame);
                }
                this.frameId = this.frame.frame_id;
            }
        }

        private void OnDestroy()
        {
            if(this.sock_ptr != 0)
            {
                MeocapSDK.meocap_clean_up(this.sock_ptr);
            }
        }
    }

}
