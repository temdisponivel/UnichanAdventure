using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour {

	public Transform _lookAt = null;
	public Transform _followPoint = null;
	public float _velocity = 1f;
	
	// Update is called once per frame
	void LateUpdate () {
		this.transform.position = Vector3.Lerp (this.transform.position, this._followPoint.position, this._velocity * Time.deltaTime);
		this.transform.LookAt (this._lookAt);
	}
}