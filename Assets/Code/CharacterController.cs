using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    public float _boostMultiplier = 1.5f;
    public float _velocity = 200f;
    public float _angularVelocity = 200f;
    public Animator _animator = null;
    public Rigidbody _rigid = null;

    virtual protected void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        this.UpdateAnimation(horizontal, vertical, this._rigid.velocity.magnitude);
    }

    virtual protected void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        float multiplier = this._angularVelocity * horizontal * (Input.GetButton("Boost") ? this._boostMultiplier : 1);
        this.transform.Rotate(Vector3.up * multiplier);

        multiplier = this._velocity * vertical * (Input.GetButton("Boost") ? this._boostMultiplier : 1);
        this._rigid.AddForce(this.transform.forward * multiplier / (this._rigid.velocity.magnitude + 0.1f), ForceMode.Impulse);
    }

    virtual protected void UpdateAnimation(float horizontal, float vertical, float velocity)
    {
        _animator.SetFloat("Walking", vertical);
        _animator.SetFloat("Velocity", velocity);
        _animator.SetFloat("Steering", horizontal);
    }
}
