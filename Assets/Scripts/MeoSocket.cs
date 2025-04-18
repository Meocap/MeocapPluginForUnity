using Palmmedia.ReportGenerator.Core.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace Meocap.SDK
{
    #region 数据结构

    [Serializable]
    public struct Addr
    {
        public byte a, b, c, d;
        public ushort port;

        public IPEndPoint ToEndPoint() => new IPEndPoint(new IPAddress(new byte[] { a, b, c, d }), port);

        public static Addr FromEndPoint(IPEndPoint ep)
        {
            var ip = ep.Address.GetAddressBytes();
            return new Addr
            {
                a = ip[0],
                b = ip[1],
                c = ip[2],
                d = ip[3],
                port = (ushort)ep.Port
            };
        }
    }

    [Serializable]
    public struct Xyz
    {
        public double x, y, z;
    }

    [Serializable]
    public struct JointRot
    {
        public string name;
        public Xyz rot;
    }

    [Serializable]
    public struct BlendShape
    {
        public string name;
        public float value;
    }

    [Serializable]
    public struct ExtraCaptureResult
    {
        public bool enable;
        public List<JointRot> hands;
        public List<BlendShape> faces;
    }

    [Serializable]
    public struct UniversalFrame
    {
        public int frame_id;

        public List<float> glb_opt_pose;
        public List<float> optimized_pose;

        public List<float> translation;
        public List<float> joint_positions;

        public ExtraCaptureResult? extra_result;
    }

    [Serializable]
    public struct SkelJoint
    {
        public double[] pos; // length = 3
    }

    [Serializable]
    public struct SkelBase
    {
        public SkelJoint[] bones; // length = 24
        public double floor_y;
    }

    [Serializable]
    public struct Joint
    {
        public Vector3 pos;     // length = 3
        public Quaternion glb_rot; // length = 4
        public Quaternion loc_rot; // length = 4
    }

    [Serializable]
    public struct SetSkelPayload
    {
        public SkelBase SetSkel;
    }

    [Serializable]
    public struct RetBlendShape
    {
        public int bl_id;
        public double value;
    }

    [Serializable]
    public struct MeoFrame
    {
        public int frame_id;
        public Vector3 translation; // length = 3
        public Joint[] joints;       // length = 24
        public Addr src;
        public Joint[] left_hand_joints;  // optional
        public Joint[] right_hand_joints; // optional
        public RetBlendShape[] blendeshapes;
    }

    #endregion

    public class MeocapSocket : IDisposable
    {
        private readonly UdpClient udp;

        public MeocapSocket(Addr bindAddr)
        {
            udp = new UdpClient(bindAddr.ToEndPoint());
            udp.Client.ReceiveTimeout = 250;
            udp.Client.SendTimeout = 250;

        }

        public void SetSkeleton(SkelBase skel, Addr targetAddr)
        {
            var payload = JsonUtility.ToJson(new SetSkelPayload
            {
                SetSkel = skel
            });
            Debug.Log(payload);

            var bytes = Encoding.UTF8.GetBytes(payload);
            udp.Send(bytes, bytes.Length, targetAddr.ToEndPoint());
        }

        public MeoFrame? ReceiveFrame()
        {
            try
            {
                // 接收数据（同步阻塞）
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = udp.Receive(ref remoteEndPoint);

                // 解析成 JSON
                string json = Encoding.UTF8.GetString(buffer);
                UniversalFrame? frame;

                try
                {
                    frame = JsonUtility.FromJson<UniversalFrame>(json);
                }
                catch
                {
                    return null;
                }

                if (frame == null) return null;

                // 转换成 MeoFrame 返回
                return ConvertFrame(frame.Value, Addr.FromEndPoint(remoteEndPoint));
            }
            catch (SocketException)
            {
                return null;
            }
            catch
            {
                return null;
            }
        }

        private MeoFrame ConvertFrame(UniversalFrame frame, Addr src)
        {
            var meo = new MeoFrame
            {
                frame_id = frame.frame_id,
                translation = new Vector3(frame.translation[0], frame.translation[1], frame.translation[2]),
                src = src,
                joints = new Joint[24],
                left_hand_joints = new Joint[15],
                right_hand_joints = new Joint[15],
                blendeshapes = (frame.extra_result == null ? new RetBlendShape[7] : null),
            };

            for (int i = 0; i < 24; i++)
            {
                var pos = frame.joint_positions.GetRange(i * 3, 3).ToArray();
                meo.joints[i].pos = new Vector3(pos[0], pos[1], pos[2]);
                meo.joints[i].glb_rot = MatrixToQuaternion(frame.glb_opt_pose.GetRange(i * 9, 9));
                meo.joints[i].loc_rot = MatrixToQuaternion(frame.optimized_pose.GetRange(i * 9, 9));
            }

            if (frame.extra_result is ExtraCaptureResult extra)
            {
                foreach (var face in extra.faces)
                {
                    int id = face.name switch
                    {
                        "Blink_L" => 0,
                        "Blink_R" => 1,
                        "A" => 2,
                        "E" => 3,
                        "I" => 4,
                        "O" => 5,
                        "U" => 6,
                        _ => -1
                    };

                    if (id >= 0)
                    {
                        meo.blendeshapes[id] = new RetBlendShape
                        {
                            bl_id = id,
                            value = face.value
                        };
                    }
                }
            }

            return meo;
        }

        private Quaternion MatrixToQuaternion(List<float> m)
        {
            var mat = new Matrix4x4();
            mat.m00 = m[0]; mat.m01 = m[3]; mat.m02 = m[6];
            mat.m10 = m[1]; mat.m11 = m[4]; mat.m12 = m[7];
            mat.m20 = m[2]; mat.m21 = m[5]; mat.m22 = m[8];
            mat.m33 = 1f;
            return mat.rotation;
        }

        public void Dispose()
        {
            udp.Dispose();
        }
    }

}
