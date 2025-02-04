using UnityEngine;

public class BillboardSprite: MonoBehaviour {

	private Transform MyCameraTransform;
	private Transform MyTransform;
	private bool alignNotLook = true;

	// Use this for initialization
	private void Start () {
		MyTransform = this.transform;
		MyCameraTransform = Camera.main.transform;
	}
	
	// Update is called once per frame
	private void LateUpdate () {
		if (alignNotLook)
			MyTransform.forward = MyCameraTransform.forward;
		else
			MyTransform.LookAt (MyCameraTransform, Vector3.up);
	}
}