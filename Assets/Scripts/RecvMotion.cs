using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecvMotion : MonoBehaviour {
	Client client;
	MotionController motionController;
	string data = "";

	void Start() {
		client = FindObjectOfType<Client>();
		motionController = FindObjectOfType<MotionController>();
	}

	void Update() {
		string[] motions, pose_and_tran;
		do {
			data += client.Receive(256);
			motions = data.Split(new char[] { '$' }, System.StringSplitOptions.RemoveEmptyEntries);
		} while (motions.Length <= 1);
		data = motions[1];
		pose_and_tran = motions[0].Split(new char[] { '#' }, System.StringSplitOptions.RemoveEmptyEntries);
		motionController.SetPose(pose_and_tran[0]);
		motionController.SetRootPosition(pose_and_tran[1]);
	}
}
