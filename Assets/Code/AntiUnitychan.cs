using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AntiUnitychan : MonoBehaviour
{
    public GameObject[] _navPoints = null;
    public GameObject _finalTarget = null;
    public GameObject _unitChanGhost = null;
    public NavMeshAgent _navMeshAgent = null;
    public Animator _animator = null;
    public AudioSource _audioSource = null;
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
    public Quaternion _lastRotation;
    public Vector3 _currentTargetPosition;

    public bool OnAlert { get; set; }
    public bool OnTarget { get; set; }

    public void Start()
    {
        this._lastPositionSeen = this._finalTarget.transform.position;
        this.UpdateNavPointPosition();
    }

    protected void Update()
    {
        this.UpdateAnimation();
        this.UpdateOnAlert();
        this.UpdateOnTarget();
        this.UpdateNextTarget();
    }

    protected void UpdateTargetPosition()
    {
        this._lastTimeSeen = Time.time;
        this._lastPositionSeen = this._finalTarget.transform.position;
        this._unitChanGhost.transform.position = this._lastPositionSeen;
        this._unitChanGhost.SetActive(true);
        this._navMeshAgent.SetDestination(this._currentTargetPosition = this._lastPositionSeen);
    }

    protected void UpdateNavPointPosition()
    {
        this._navMeshAgent.SetDestination(this._currentTargetPosition = this.GetNextNavPoint().transform.position);
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
        return this._navPoints[this._currentNavPointIndex];
    }

    protected void UpdateAnimation()
    {
        _animator.SetFloat("Walking", this._navMeshAgent.velocity.normalized.magnitude);
        _animator.SetFloat("Velocity", this._navMeshAgent.velocity.magnitude);
        _animator.SetFloat("Steering", (this.transform.rotation.eulerAngles - this._lastRotation.eulerAngles).y);
        this._lastRotation = this.transform.rotation;
        this._audioSource.volume = Mathf.SmoothStep(0, 1, Mathf.Min(this._navMeshAgent.velocity.magnitude, this._navMeshAgent.speed) / this._navMeshAgent.speed);
    }

    protected void ReachFinalTarget()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    protected void UpdateNextPositionOnSight()
    {
        Debug.Log("ON SIGHT");
        if (this._currentTargetPosition == this._lastPositionSeen)
        {
            if (Time.time - this._lastTimeSeen >= this._timeToUpdateTargetPosition || this.OnTarget)
            {
                Debug.Log("CHANGING A");
                this.UpdateTargetPosition();
            }
        }
        else
        {
            Debug.Log("CHANGING B");
            this.UpdateTargetPosition();
        }
    }

    protected void UpdateNextPositionOffSight()
    {
        if (this.OnAlert)
        {
            if (this.OnTarget)
            {
                this.UpdateNavPointPositionToNearstPoint();
            }
        }
        else if (this.OnTarget)
        {
            this.UpdateNavPointPosition();
        }
    }

    protected void UpdateNavPointPositionToNearstPoint()
    {
        Vector3 nearst;
        this._currentNavPointIndex = this.GetNearstNavPoint(out nearst);
        this._navMeshAgent.SetDestination(this._currentTargetPosition = nearst);
    }

    protected int GetNearstNavPoint(out Vector3 nearst)
    {
        int indexOfNearst = 0;
        float minDistance = float.PositiveInfinity;
        float auxDistance = 0f;
        for (int i = 0; i < this._navPoints.Length; i++)
        {
            if ((i != this._currentNavPointIndex || !this.OnTarget) && (auxDistance = Vector3.Distance(this._lastPositionSeen, this._navPoints[i].transform.position)) < minDistance)
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

    protected void UpdateNextTarget()
    {
        if (this.IsTargetOnSight())
        {
            this.UpdateNextPositionOnSight();
        }
        else
        {
            this.UpdateNextPositionOffSight();
        }
    }

    protected void UpdateOnAlert()
    {
        bool alert = Vector3.Distance(this._finalTarget.transform.position, this.transform.position) <= Mathf.Max(this._alertDistance / 7, (this._alertDistance * this._finalTarget.GetComponent<AudioSource>().volume));
        if (alert && !this.OnAlert)
        {
            this._animator.SetTrigger("Alert");
            this._lastPositionSeen = this._finalTarget.transform.position;
            this.UpdateNavPointPositionToNearstPoint();
            this._navMeshAgent.Stop();
            this.StartCoroutine(CallbackHelper.WaitForSecondsAndCall(3, () => this._navMeshAgent.Resume()));
        }
        else if (!alert && this.OnAlert)
        {
            this.FadeUnitichanGhost();
        }
        this.OnAlert = alert;
    }

    protected void UpdateOnTarget()
    {
        bool target = Vector3.Distance(this._navMeshAgent.destination, this.transform.position) <= 1;
        if (target && !this.OnTarget)
        {
            if (!this.IsTargetOnSight())
            {
                this._animator.SetTrigger("ReachTarget");
                this._navMeshAgent.Stop();
                this.StartCoroutine(CallbackHelper.WaitForSecondsAndCall(3, () => this._navMeshAgent.Resume()));
            }
        }
        this.OnTarget = target;
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, Mathf.Max(this._alertDistance / 7, (this._alertDistance * this._finalTarget.GetComponent<AudioSource>().volume)));
        Gizmos.color = Color.blue;
        Quaternion aux = this.transform.rotation;
        for (int i = 0; i < 180; i++)
        {
            RaycastHit ray;
            Vector3 directionA = Vector3.RotateTowards(this.transform.forward, this.transform.right, Mathf.Deg2Rad * i, 1);
            Vector3 directionB = Vector3.RotateTowards(this.transform.forward, -this.transform.right, Mathf.Deg2Rad * i, 1);
            if (Physics.Raycast(this.transform.position, directionA, out ray, this._sightDistance))
            {
                Gizmos.DrawLine(this.transform.position, this.transform.position+directionA * ray.distance);
            }
            else
            {
                Gizmos.DrawLine(this.transform.position, this.transform.position+directionA * this._sightDistance);
            }
            if (Physics.Raycast(this.transform.position, directionB, out ray, this._sightDistance))
            {
                Gizmos.DrawLine(this.transform.position, this.transform.position + directionB * ray.distance);
            }
            else
            {
                Gizmos.DrawLine(this.transform.position, this.transform.position+directionB * this._sightDistance);
            }
        }
    }
}