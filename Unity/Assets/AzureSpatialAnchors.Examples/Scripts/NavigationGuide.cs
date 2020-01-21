using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using UnityEngine;

public class NavigationGuide : MonoBehaviour
{
    #region Object Members
    private ARNavigation arnavigation;
    private GameObject guide = null;
    private int originId = 1;
    private int destinationId = 2;
    #endregion Object Members
    #region Control Functions
    public NavigationGuide(ARNavigation arNavigation)
    {
        this.arnavigation = arNavigation;
    }
    public void Update()
    {
        if(arnavigation.allspawnedObjects.Count >= 2)
        {
            spawnGuide();
            updatePosition();
        }
    }
    private void spawnGuide()
    {
        if(guide == null)
        {
            guide = GameObject.Instantiate(arnavigation.guidePrefab,guidePosition(), guideDirection());
        }
    }
    private void updatePosition()
    {
        guide.transform.position = guidePosition();
        guide.transform.rotation = guideDirection();
    }
    #endregion Control Functions

    private GameObject origin() {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[originId]];
        
    }
    private GameObject destination()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[destinationId]];
    }
    private Vector3 guidePosition()
    {

        float multiplicator = 0.0f;
        float newDistance = 999999.0f;
        float previousDistance = 99999999.0f;
        while (newDistance < previousDistance && multiplicator <= 1.0f)
        {
            previousDistance = newDistance;
            newDistance = Vector3.Distance(origin().transform.position + path() * multiplicator, Camera.main.transform.position);
            multiplicator += 0.01f;
        }
        multiplicator *= 1.2f;
        if (multiplicator >= 1)
        {
            moveToNextObject();
        }
        float originDistance = Vector3.Distance(origin().transform.position, Camera.main.transform.position);
        float destinationDistance = Vector3.Distance(destination().transform.position, Camera.main.transform.position);
        arnavigation.printmsg = "MP:" + multiplicator + " OR:" + originId +"DistO:"+ originDistance + "DS" + destinationId + "DistT:" + destinationDistance;
        return origin().transform.position + path() * multiplicator;
    }
    private void moveToNextObject()
    {
        if(destinationId < arnavigation.anchorOrder.Count)
        {
            originId += 1;
            destinationId += 1;
        }
    }
    private Quaternion guideDirection()
    {
        return Quaternion.LookRotation(path());
    }
    private Vector3 path()
    {
        return destination().transform.position - origin().transform.position;
    }
}