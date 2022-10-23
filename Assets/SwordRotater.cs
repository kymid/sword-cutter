using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordRotater : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] Vector3 offsetPos;
    [SerializeField] float scale,speed,rotateScale,rotateSpeed;
    void Start()
    {
        offsetPos = transform.localPosition;
        joystick.DeadZone = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Moved();
        Rotated();
    }
    void Moved()
    {
        Vector3 joyDirection = new Vector3(joystick.Direction.x, joystick.Direction.y, 0);
        Vector3 neededPos = offsetPos + joyDirection * scale;
        transform.localPosition = Vector3.Lerp(transform.localPosition, neededPos, speed * Time.deltaTime);
    }

    float x, y, z;
    void Rotated()
    {
        x = -joystick.Direction.y * rotateScale;
        y = joystick.Direction.x * rotateScale;
        z = Mathf.Atan2(joystick.Rotation.y - offsetPos.y, joystick.Rotation.x - offsetPos.x);
        z *= Mathf.Rad2Deg; 
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(x, y, z),rotateSpeed * Time.deltaTime);


    }
}
