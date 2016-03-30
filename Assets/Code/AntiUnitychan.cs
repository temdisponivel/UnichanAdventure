using UnityEngine;
using System.Collections;

public class AntiUnitychan : MonoBehaviour
{
    public GameObject[] _navPoints = null;
    public GameObject _targetObject = null;
    public NavMeshAgent _navMeshAgent = null;
    public Animator _animator = null;
    public float _onPositionEpsilon = .5f;
    public float _randomPositionRange = 5f;
    public float _sightDistance = 50f;
    protected Vector3 _targetPosition = default(Vector3);
    protected bool _isTargetPositionLastSeen = false;
    protected int _currentNavPoint = -1;
    protected int _sumToNavPointindex = 1;

    public void Start()
    {
        this.UpdateTargetPosition();
    }

    protected void Update()
    {
        this.UpdateAnimation();
        if (this.OnTargetPosition() && !this.OnTargetObject()) //if iam on my current position objective, but not on the target object position
        {
            if (this._isTargetPositionLastSeen)
            {
                this.ReachLastSeen();
            }
            else
            {
                if (this.IsTargetOnSight())
                {
                    this.UpdateTargetPosition();
                }
                else
                {
                    this.UpdateNavPointPosition();
                }
            }
        }
    }

    protected void UpdateTargetPosition()
    {
        this._navMeshAgent.SetDestination(this._targetObject.transform.position);
        this._isTargetPositionLastSeen = true;
    }

    protected void UpdateNavPointPosition()
    {
        this._navMeshAgent.SetDestination(this._targetObject.transform.position);
    }

    protected bool IsTargetOnSight()
    {
        RaycastHit hit;
        return Physics.Raycast(this.transform.position, this._targetObject.transform.position, out hit, this._sightDistance);
    }

    protected Vector3 GetNextNavPoint()
    {
        if (this._currentNavPoint == this._navPoints.Length)
        {
            this._sumToNavPointindex = -1;
        }
        else if (this._currentNavPoint == 0)
        {
            this._sumToNavPointindex = 1;
        }
        this._currentNavPoint += this._sumToNavPointindex;
        return this._navPoints[this._currentNavPoint].transform.position;
    }

    protected void UpdateAnimation()
    {
        _animator.SetFloat("Walking", this._navMeshAgent.velocity.normalized.magnitude);
        _animator.SetFloat("Velocity", this._navMeshAgent.velocity.magnitude);
        _animator.SetFloat("Steering", this._navMeshAgent.steeringTarget.normalized.magnitude);
    }

    protected bool OnTargetPosition()
    {
        return Vector3.Distance(this.transform.position, this._targetPosition) < this._onPositionEpsilon;
    }

    protected bool OnTargetObject()
    {
        return Vector3.Distance(this.transform.position, this._targetObject.transform.position) < this._onPositionEpsilon;
    }

    protected float DistanceToTargetObject()
    {
        return Vector3.Distance(this.transform.position, this._targetObject.transform.position);
    }

    protected void ReachLastSeen()
    {
        this._animator.SetTrigger("ReachLastSeen");
        CallbackHelper.WaitForSecondsAndCall(1, this.UpdatePositionFromLastSeen);
    }

    protected void UpdatePositionFromLastSeen()
    {
        this._isTargetPositionLastSeen = false;
    }
}
