using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(JointConfig))]
public class MotionController : MonoBehaviour
{
	JointConfig jointConfig;

	void Start() {
		jointConfig = GetComponent<JointConfig>();
	}

	/// <summary>
	/// Set pose with a string, which should contain joint_num * 3 float numbers separated by a specific char.
	/// The pose is defined as joint rotations in axis-angle format in a right-handed system (x=left, y=up, z=forward).
	/// </summary>
	/// <param name="pose">The pose string that contains joint_num * 3 float numbers.</param>
	/// <param name="split">The split character between each number.</param>
	public void SetPose(string pose, char split=',') {
		string[] ps = pose.Split(split);
		float[,] pf = new float[jointConfig.joints.Length, 3];
		for (int i = 0; i < jointConfig.joints.Length; ++i) {
			for (int j = 0; j < 3; ++j) {
				pf[i, j] = float.Parse(ps[i * 3 + j]);
			}
		}
		SetPose(pf);
	}

	/// <summary>
	/// Set pose with a float array in shape [joint_num, 3].
	/// The pose is defined as joint rotations in axis-angle format in a right-handed system (x=left, y=up, z=forward).
	/// </summary>
	/// <param name="pose">Pose array in shape [joint_num, 3]</param>
	public void SetPose(float[,] pose) {
		Quaternion save = jointConfig.joints[0].parent.rotation;
		jointConfig.joints[0].parent.rotation = jointConfig.BaseRotation;
		ClearPose();
		for (int i = jointConfig.joints.Length - 1; i >= 0; --i) {
			Vector3 aa = new Vector3(pose[i, 0], -pose[i, 1], -pose[i, 2]);  // to unity coordinate
			jointConfig.joints[i].rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * aa.magnitude, aa.normalized) * jointConfig.joints[i].rotation;
		}
		jointConfig.joints[0].parent.rotation = save;
	}

	/// <summary>
	/// Clear pose (or set T-pose, set zero pose)
	/// </summary>
	public void ClearPose() {
		for (int i = 0; i < jointConfig.joints.Length; ++i) {
			jointConfig.joints[i].localRotation = jointConfig.Tpose[i];
		}
	}

	/// <summary>
	/// Set the root position with a string, which should contain 3 float numbers separated by a specific char.
	/// The position is defined in a right-handed system (x=left, y=up, z=forward). 
	/// </summary>
	/// <param name="position">The position string that contains 3 float numbers.</param>
	/// <param name="split">The split character between each number.</param>
	public void SetRootPosition(string position, char split = ',') {
		string[] ps = position.Split(split);
		Vector3 p = new Vector3(-float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]));  // to unity coordinate
		SetRootPosition(p);
	}

	/// <summary>
	/// Set the root position with a float array in shape [3].
	/// The position is defined in a right-handed system (x=left, y=up, z=forward). 
	/// </summary>
	/// <param name="position">Position array in shape [3]</param>
	public void SetRootPosition(float[] position) {
		Vector3 p = new Vector3(-position[0], position[1], position[2]);  // to unity coordinate
		SetRootPosition(p);
	}

	/// <summary>
	/// Set the root position with a Vector3.
	/// The position is defined in unity coordinate, i.e., a left-handed system (x=right, y=up, z=forward). 
	/// </summary>
	/// <param name="position">Root position Vector3 in unity coordinate</param>
	public void SetRootPosition(Vector3 position) {
		jointConfig.TranslationJoint.localPosition = position;
	}

	/// <summary>
	/// Get the root local position (the position in the motion system).
	/// The position is defined in unity coordinate, i.e., a left-handed system (x=right, y=up, z=forward). 
	/// </summary>
	/// <returns>Root position in unity coordinate</returns>
	public Vector3 GetRootPosition() {
		return jointConfig.TranslationJoint.localPosition;
	}

	/// <summary>
	/// Get the root world position (the real position in the scene).
	/// The position is defined in unity coordinate, i.e., a left-handed system (x=right, y=up, z=forward). 
	/// </summary>
	/// <returns>Root position in unity coordinate</returns>
	public Vector3 GetRootWorldPosition() {
		return jointConfig.TranslationJoint.position;
	}

	/// <summary>
	/// Clear root position (move to the position of the beginning).
	/// </summary>
	public void ClearRootPosition() {
		jointConfig.TranslationJoint.localPosition = jointConfig.BeginPosition;
	}

	/// <summary>
	/// Set the root translation with a string, which should contain 3 float numbers separated by a specific char.
	/// The translation is defined in a right-handed system (x=left, y=up, z=forward). 
	/// </summary>
	/// <param name="translation">The translation string that contains 3 float numbers.</param>
	/// <param name="split">The split character between each number.</param>
	public void SetRootTranslation(string translation, char split = ',') {
		string[] ps = translation.Split(split);
		Vector3 p = new Vector3(-float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]));  // to unity coordinate
		SetRootTranslation(p);
	}

	/// <summary>
	/// Set the root translation with a float array in shape [3].
	/// The translation is defined in a right-handed system (x=left, y=up, z=forward). 
	/// </summary>
	/// <param name="translation">Translation array in shape [3]</param>
	public void SetRootTranslation(float[] translation) {
		Vector3 p = new Vector3(-translation[0], translation[1], translation[2]);  // to unity coordinate
		SetRootTranslation(p);
	}

	/// <summary>
	/// Set the root translation with a Vector3.
	/// The translation is defined in unity coordinate, i.e., a left-handed system (x=right, y=up, z=forward). 
	/// </summary>
	/// <param name="translation">Root translation in unity coordinate</param>
	public void SetRootTranslation(Vector3 translation) {
		jointConfig.TranslationJoint.Translate(translation, Space.World);
	}
}
