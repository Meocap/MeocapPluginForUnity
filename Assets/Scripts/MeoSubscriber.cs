using System.Collections;
using System.Collections.Generic;
using NetMQ.Sockets;
using NetMQ;
using UnityEngine;
using Meocap;
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
        private SubscriberSocket subscriberSocket;
        [Header("当前帧ID")]
        public int frameId = 0;
        void Start()
        {
            AsyncIO.ForceDotNet.Force();
            subscriberSocket = new SubscriberSocket();
            subscriberSocket.Options.ReceiveHighWatermark = 1000;
            subscriberSocket.Connect($"tcp://{address}:{port}");
            subscriberSocket.Subscribe("");
            Debug.Log("订阅者已连接到 " + address);

            StartCoroutine(ReceiveData());
        }

        private IEnumerator ReceiveData()
        {
            while (true)
            {
                if (subscriberSocket.TryReceiveFrameString(out string message))
                {
                    try
                    {
                        Perform.UniversalFrame frameData = JsonUtility.FromJson<Perform.UniversalFrame>(message);
                        frameId = frameData.frame_id;
                        if (actor != null)
                        {
                            actor.Perform(frameData);
                        }
                    }
                    catch
                    {

                    }


                }
                yield return null; // 确保Unity不会冻结
            }
        }

        private void OnDestroy()
        {
            subscriberSocket.Close();
            subscriberSocket.Dispose();
            NetMQConfig.Cleanup();
        }
    }

}
