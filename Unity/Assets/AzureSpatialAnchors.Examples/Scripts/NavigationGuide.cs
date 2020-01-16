using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NavigationGuide : MonoBehaviour
{
    private ARNavigation arnavigation;
    private GameObject guide = null;
    public NavigationGuide(ARNavigation arNavigation)
    {
        this.arnavigation = arNavigation;
        this.Start();
    }
    // Start is called before the first frame update
    void Start()
    {
        
        //guide.transform.position.x;
    }

    // Update is called once per frame
    public void Update()
    {
        if(arnavigation.allspawnedObjects.Count >= 2)
        {
            spawnGuide();
            moveGuide();
        }

    }

    private void spawnGuide()
    {
        if(guide == null)
        {
            guide = GameObject.Instantiate(arnavigation.guidePrefab,currentOrigin().transform.position, guideDirection());
        }
        
    }
    private void moveGuide()
    {

    }
    private GameObject currentOrigin() {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[1]]; //TODO change number
    }
    private GameObject currentDestination()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[2]]; //TODO change number
    }
    private Quaternion guideDirection()
    {
        return Quaternion.Euler(direction().x, direction().y, direction().z);
    }
    private Vector3 direction()
    {
        return new Vector3(
            currentDestination().transform.position.x - currentOrigin().transform.position.x,
            currentDestination().transform.position.y - currentOrigin().transform.position.y,
            currentDestination().transform.position.z - currentOrigin().transform.position.z
            );

    }
}
