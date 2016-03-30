using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    public float _velocity = 200f;
    public float _angularVelocity = 200f;
    public Animator _animator = null;
    public Rigidbody _rigid = null;

    virtual protected void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        this.UpdateAnimation(horizontal, vertical, this._rigid.velocity.magnitude);
        this.transform.Rotate(Vector3.up, horizontal * Time.deltaTime * this._angularVelocity / (this._rigid.velocity.magnitude + .1f));
    }

    virtual protected void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical");
        this._rigid.AddForce(this.transform.forward * vertical * this._velocity / (this._rigid.velocity.magnitude + 0.1f), ForceMode.Force);
    }

    virtual protected void UpdateAnimation(float horizontal, float vertical, float velocity)
    {
        _animator.SetFloat("Walking", vertical);
        _animator.SetFloat("Velocity", velocity);
        _animator.SetFloat("Steering", horizontal);
    }
}
