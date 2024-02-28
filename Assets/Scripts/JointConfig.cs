using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointConfig : MonoBehaviour
{
	[Tooltip("If the T-pose is not zero rotations, check this on and let the character begin with T-pose (including root rotation)")]
	public bool readZeroPoseOnStart = false;
	[Tooltip("All rotational joints in the root-to-leaf order. The parent of the first joint is assumed a translational joint.")]
	public Transform[] joints;

	public Transform TranslationJoint { get { return joints[0].parent; } }
	public Quaternion[] Tpose { get; set; }
	public Vector3 BeginPosition { get; set; }
	public Quaternion BaseRotation { get; set; }


	private void Start() {
		Tpose = new Quaternion[joints.Length];
		for (int i = 0; i < joints.Length; i++) {
			Tpose[i] = readZeroPoseOnStart ? joints[i].localRotation : Quaternion.identity;
		}
		BaseRotation = readZeroPoseOnStart ? joints[0].parent.rotation : Quaternion.identity;
		BeginPosition = TranslationJoint.localPosition;
	}
}
