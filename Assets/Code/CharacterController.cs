using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
        
        float multiplier = this._angularVelocity * horizontal * (Input.GetButton("Boost") ? this._boostMultiplier : 1) * Time.deltaTime;
        this.transform.Rotate(Vector3.up * multiplier);

        multiplier = this._velocity * vertical * (Input.GetButton("Boost") ? this._boostMultiplier : 1) * Time.deltaTime;
        this.transform.position += this.transform.forward * multiplier;

        this.UpdateAnimation(horizontal, vertical, multiplier / Time.deltaTime);
    }

    virtual protected void UpdateAnimation(float horizontal, float vertical, float velocity)
    {
        _animator.SetFloat("Walking", vertical);
        _animator.SetFloat("Velocity", velocity);
        _animator.SetFloat("Steering", horizontal);
    }

    protected void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "FinishPoint")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
