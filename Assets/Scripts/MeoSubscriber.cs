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

        Meocap.MeoFrame frame;
        private ulong sock_ptr;

        void Start()
        {
            AsyncIO.ForceDotNet.Force();
            string[] ip_addr = address.Split('.');
            if(ip_addr.Length != 4 ) {
                Debug.LogError("MeoSubscriber: IPAddress Format Error");
                return;
            }
            this.sock_ptr = MeocapSDK.meocap_connect_server_char(byte.Parse(ip_addr[0]), byte.Parse(ip_addr[1]), byte.Parse(ip_addr[2]), byte.Parse(ip_addr[3]), (ushort)port);
            StartCoroutine(ReceiveData());
        }

        private IEnumerator ReceiveData()
        {
            while (true)
            {
                int ret = MeocapSDK.meocap_recv_frame(this.sock_ptr, out this.frame);
                if (ret==0)
                {
                    if(actor != null)
                    {
                        actor.Perform(this.frame);
                    }
                }
                yield return null; // 确保Unity不会冻结
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
