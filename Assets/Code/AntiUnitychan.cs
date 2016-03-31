using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AntiUnitychan : MonoBehaviour
{
    public GameObject[] _navPoints = null;
    public GameObject _finalTarget = null;
    public GameObject _unitChanGhost = null;
    public NavMeshAgent _navMeshAgent = null;
    public Animator _animator = null;
    public float _onPositionEpsilon = .5f;
    public float _randomPositionRange = 5f;
    public float _sightDistance = 30f;
    public float _alertDistance = 30f;
    public float _timeToUpdateTargetPosition = 2f;
    protected float _lastTimeSeen = 0f;
    protected Vector3 _lastPositionSeen = default(Vector3);
    protected int _currentNavPointIndex = -1;
    public int _sumToNavPointIndex = 1;
    public float _lastAlertTime = 0f;
    public bool _alert = false;
    public Quaternion _lastRotation;

    public bool OnAlert
    {
        get
        {
            bool alert = (this.transform.position - this._finalTarget.transform.position).magnitude <= this._alertDistance;
            if (alert && !this._alert)
            {
                this._animator.SetTrigger("Alert");
                this.UpdateNavPointPositionToNearstPoint();
            }
            else if (!alert && this._alert)
            {
                this.FadeUnitichanGhost();
            }
            this._alert = alert;
            return this._alert;
        }
    }
    public bool OnTarget { get { return (Vector3.Distance(this._navMeshAgent.destination, this.transform.position) <= .1); } }

    public void Start()
    {
        this.UpdateNavPointPosition();
    }

    protected void Update()
    {
        this.UpdateAnimation();
        if (this.IsTargetOnSight())
        {
            this.UpdateRoutineOnSight();
        }
        else
        {
            this.UpdateRoutineOffSight();
        }
    }

    protected void UpdateTargetPosition(bool playAnimation)
    {
        if (playAnimation)
        {
            this._animator.SetTrigger("ReachTarget");
            this._navMeshAgent.Stop();
            this.StartCoroutine(CallbackHelper.WaitForSecondsAndCall(2, () => this._navMeshAgent.Resume()));
        }
        this._lastTimeSeen = Time.time;
        this._lastPositionSeen = this._finalTarget.transform.position;
        this._unitChanGhost.transform.position = this._lastPositionSeen;
        this._unitChanGhost.SetActive(true);
        this._navMeshAgent.SetDestination(this._lastPositionSeen);
    }

    protected void UpdateNavPointPosition()
    {
        this._navMeshAgent.SetDestination(this.GetNextNavPoint().transform.position);
    }

    protected bool IsTargetOnSight()
    {
        RaycastHit hit;
        return Vector3.Dot(this.transform.forward, this._finalTarget.transform.position - this.transform.position) > 0
            && Physics.Raycast(this.transform.position, (this._finalTarget.transform.position - this.transform.position).normalized, out hit, this._sightDistance)
            && hit.collider.gameObject == this._finalTarget;

    }

    protected GameObject GetNextNavPoint()
    {
        if (this._currentNavPointIndex == this._navPoints.Length - 1)
        {
            this._sumToNavPointIndex = -1;
        }
        else if (this._currentNavPointIndex == 0)
        {
            this._sumToNavPointIndex = 1;
        }
        this._currentNavPointIndex += this._sumToNavPointIndex;
        Debug.Log(this._currentNavPointIndex);
        return this._navPoints[this._currentNavPointIndex];
    }

    protected void UpdateAnimation()
    {
        _animator.SetFloat("Walking", this._navMeshAgent.velocity.normalized.magnitude);
        _animator.SetFloat("Velocity", this._navMeshAgent.velocity.magnitude);
        _animator.SetFloat("Steering", (this.transform.rotation.eulerAngles - this._lastRotation.eulerAngles).y);
        this._lastRotation = this.transform.rotation;
    }

    protected void ReachFinalTarget()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    protected void UpdateRoutineOnSight()
    {
        if (this._navMeshAgent.destination == this._lastPositionSeen)
        {
            if (Time.time - this._lastTimeSeen >= this._timeToUpdateTargetPosition || this.OnTarget)
            {
                this.UpdateTargetPosition(true);
            }
        }
        else
        {
            this.UpdateTargetPosition(false);
        }
    }

    protected void UpdateRoutineOffSight()
    {
        if (this.OnTarget)
        {
            if (this.OnAlert)
            {
                this._animator.SetTrigger("ReachTarget");
                this._navMeshAgent.Stop();
                this.UpdateNavPointPositionToNearstPoint();
                this.StartCoroutine(CallbackHelper.WaitForSecondsAndCall(2, () => this._navMeshAgent.Resume()));
            }
            else
            {
                this._animator.SetTrigger("ReachTarget");
                this._navMeshAgent.Stop();
                this.UpdateNavPointPosition();
                this.StartCoroutine(CallbackHelper.WaitForSecondsAndCall(2, () => this._navMeshAgent.Resume()));
            }
        }
    }

    protected void UpdateNavPointPositionToNearstPoint()
    {
        Vector3 position;
        this._currentNavPointIndex = this.GetNearstNavPoint(out position);
        this._navMeshAgent.SetDestination(position);
        this._navMeshAgent.Resume();
    }

    protected int GetNearstNavPoint(out Vector3 nearst)
    {
        int indexOfNearst = 0;
        float minDistance = float.PositiveInfinity;
        float auxDistance = 0f;
        for (int i = 0; i < this._navPoints.Length; i++)
        {
            if ((this._navPoints[i].transform.position != this._navMeshAgent.destination) && (auxDistance = Vector3.Distance(this._lastPositionSeen, this._navPoints[i].transform.position)) < minDistance)
            {
                minDistance = auxDistance;
                indexOfNearst = i;
            }
        }
        nearst = this._navPoints[indexOfNearst].transform.position;
        return indexOfNearst;
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == this._finalTarget)
        {
            this.ReachFinalTarget();
        }
    }

    protected void FadeUnitichanGhost()
    {
        this._unitChanGhost.SetActive(false);
    }
}