﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using Lean.Touch;
using Touchblocks.ARTransformation;

public class PlacementController : MonoBehaviour {

    public float maxRayDistance = 30.0f;
    public LayerMask collisionLayer = 1 << 10;  //ARKitPlane layer
    public GameObject m_placementObject;
    public bool allowPlacement = true;

    public void SetPlacementObject(GameObject placementObject){
        m_placementObject = placementObject;
        allowPlacement = true;
    }

    bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
    {
        List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
        if (hitResults.Count > 0)
        {
            foreach (var hitResult in hitResults)
            {
                if (allowPlacement)
                {
                    GameObject myob = Instantiate(m_placementObject, UnityARMatrixOps.GetPosition(hitResult.worldTransform), UnityARMatrixOps.GetRotation(hitResult.worldTransform));
                    allowPlacement = false;

                    DemoControls demoControls = GameObject.FindObjectOfType<DemoControls>();
                    if(demoControls){
                        ARTouchTransformation arTouchTransformation = myob.GetComponent<ARTouchTransformation>();
                        arTouchTransformation.GetComponent<LeanSelectable>().OnSelect.AddListener((LeanFinger finger) => demoControls.OnSelect(arTouchTransformation.gameObject));
                        arTouchTransformation.GetComponent<LeanSelectable>().OnDeselect.AddListener(demoControls.OnDeSelect);
                    }
                }
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
            //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
            if (Physics.Raycast(ray, out hit, maxRayDistance, collisionLayer))
            {
                if (allowPlacement)
                {
                    GameObject myob = Instantiate(m_placementObject, hit.point, hit.transform.rotation);
                    allowPlacement = false;
                    DemoControls demoControls = GameObject.FindObjectOfType<DemoControls>();
                    if(demoControls){
                        ARTouchTransformation arTouchTransformation = myob.GetComponent<ARTouchTransformation>();
                        arTouchTransformation.GetComponent<LeanSelectable>().OnSelect.AddListener((LeanFinger finger) => demoControls.OnSelect(arTouchTransformation.gameObject));
                        arTouchTransformation.GetComponent<LeanSelectable>().OnDeselect.AddListener(demoControls.OnDeSelect);
                    }
                }
            }
        }
#else
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                    ARPoint point = new ARPoint {
                        x = screenPosition.x,
                        y = screenPosition.y
                    };

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
                        //ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    }; 
                    
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
                }
            }
#endif

    }
}
