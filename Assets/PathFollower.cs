using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class PathFollower : MonoBehaviour
{
    SplineFollower follower;
    [SerializeField] float _speed;
    public float Speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value;
            follower.followSpeed = _speed;
        }
    }
    void Start()
    {
        follower = GetComponent<SplineFollower>();
        follower.followSpeed = 0;
    }

    public void ChangeSpeed(float value)
    {
        Speed += value;

        if(Speed < 0)
            Speed = 0;
    }

    bool canStart = true;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) & canStart)
        {
            canStart = false;
            follower.followSpeed = Speed;
        }
    }


}
