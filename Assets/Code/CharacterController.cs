using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour {

	public Animator _animator = null;
	public Rigidbody _rigid = null;

	void Update () {
		float vertical = Input.GetAxis ("Vertical");
		float horizontal = Input.GetAxis ("Horizontal");
		_animator.SetFloat ("Walking", vertical);
		_animator.SetFloat ("Velocity", this._rigid.velocity.magnitude);
		this.transform.Rotate(Vector3.up, horizontal * Time.deltaTime * 20 / (this._rigid.velocity.magnitude + .1f));
	}

	void FixedUpdate() {
		float vertical = Input.GetAxis ("Vertical");
		this._rigid.AddForce(this.transform.forward * vertical * 200 / (this._rigid.velocity.magnitude + 0.1f) , ForceMode.Force);
	}
}
