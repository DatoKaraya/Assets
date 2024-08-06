using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject playerCamera;
    public float cameraXSensitivity = 3;
    public float cameraYSensitivity = 3;
    public float cameraDistanceDefault = 3;
    public float cameraXRotationDefault = 50;
    public float lockOnRange = 10;
    public float lockOnRetargetDelta = .2f;
    public int lockOnRetargetAngle = 30;

    float cameraYRotation, cameraXRotation, cameraDistance, cameraFinalDistance;
    RaycastHit cameraRay;
    Camera playerViewport;
    [NonSerialized] public GameObject targetLockOn;
    bool canTarget = true;

    // Start is called before the first frame update
    void Start()
    {
        playerViewport = playerCamera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        cameraYRotation += Input.GetAxis("Mouse X") * cameraXSensitivity;

        if(Mathf.Abs(cameraYRotation) > 180)
        {
            cameraYRotation -= 360 * Mathf.Sign(cameraYRotation);
        }


        // Lock On

        if (Input.GetButtonDown("Lock On"))
        {
            if (targetLockOn == null && canTarget)
            {
                StartCoroutine("lockOnAction");
            }

            else
            {
                if (targetLockOn != null) targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Default");

                targetLockOn = null;
            }
        }

        if (targetLockOn != null)
        {
            if (Vector2.Distance(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")), Vector2.zero) > lockOnRetargetDelta && canTarget)
            {
                StartCoroutine("lockOnRetarget");
            }

            cameraXRotation = Mathf.Rad2Deg * -Mathf.Asin((targetLockOn.transform.position.y - transform.position.y) / Vector3.Distance(transform.position, targetLockOn.transform.position));
            cameraYRotation = Mathf.Rad2Deg * Mathf.Atan2(targetLockOn.transform.position.x - transform.position.x, targetLockOn.transform.position.z - transform.position.z);

            if (Vector3.Distance(transform.position, targetLockOn.transform.position) >= lockOnRange * 1.1f)
            {
                targetLockOn = null;
            }
        }


        // Zoom

        if (targetLockOn == null)
        {
            if (cameraXRotation != cameraXRotationDefault || cameraDistance != cameraDistanceDefault)
            {
                cameraXRotation = Mathf.MoveTowards(cameraXRotation, cameraXRotationDefault, 3);
                cameraDistance = Mathf.MoveTowards(cameraDistance, cameraDistanceDefault, .2f);
            }
        }
        else
        {
            cameraDistance = 10;
        }

        cameraFinalDistance = cameraDistance;

        if (Physics.SphereCast(transform.position, .3f, playerCamera.transform.position - transform.position, out cameraRay, cameraDistance, LayerMask.GetMask("Default")))
        {
            cameraFinalDistance = cameraRay.distance - .2f;
        }

        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(cameraXRotation, cameraYRotation, 0f));
        playerCamera.transform.localPosition = new Vector3(0, 0, -cameraFinalDistance);
    }



    IEnumerator lockOnRetarget()
    {
        canTarget = false;

        if (targetLockOn != null) targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Default");

        Vector2 direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Lock-On Target");
        GameObject currentTarget = targetLockOn;
        Vector3 currentLocation;
        float targetDist = 1f;

        Vector3 currentTargetLocation = playerViewport.WorldToViewportPoint(currentTarget.transform.position);

        foreach (GameObject obj in allTargets)
        {
            currentLocation = playerViewport.WorldToViewportPoint(obj.transform.position);

            if (obj != targetLockOn && Vector2.Angle(direction, new Vector2(currentLocation.x - currentTargetLocation.x, currentLocation.y - currentTargetLocation.y)) <= lockOnRetargetAngle)
            {

                if (Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f)) < targetDist && currentLocation.z <= lockOnRange)
                {
                    if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerViewport), obj.GetComponent<Collider>().bounds))
                    {
                        targetDist = Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f));
                        currentTarget = obj;
                    }
                }
            }

        }
        targetLockOn = currentTarget;

        if (targetLockOn != null)
        {
            targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Targeted");
        }

        yield return new WaitForSeconds(.2f);

        canTarget = true;
    }

    IEnumerator lockOnAction()
    {
        canTarget = false;

        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Lock-On Target");

        Vector3 currentLocation;
        float targetDist = 1f;

        foreach (GameObject obj in allTargets)
        {
            currentLocation = playerViewport.WorldToViewportPoint(obj.transform.position);

            if (Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f)) < targetDist && currentLocation.z <= lockOnRange)
            {
                if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerViewport), obj.GetComponent<Collider>().bounds))
                {
                    targetDist = Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f));
                    targetLockOn = obj;
                }
            }
        }

        if (targetLockOn != null)
        {
            targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Targeted");
        }

        yield return new WaitForSeconds(.2f);
        
        canTarget = true;
    }
}
