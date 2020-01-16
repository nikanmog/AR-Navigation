using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using UnityEngine;

public class NavigationGuide : MonoBehaviour
{
    #region Object Members
    private ARNavigation arnavigation;
    private GameObject guide = null;
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
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[1]]; //TODO change number
        
    }
    private GameObject destination()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[2]]; //TODO change number
    }
    private Vector3 guidePosition()
    {
        float multiplicator = 0.0f;
        float newDistance = 999999.0f;
        float previousDistance = 99999999.0f;
        while (newDistance < previousDistance && multiplicator <= 1.0f)
        {
            previousDistance = newDistance;
            newDistance = Vector3.Distance(path() * multiplicator, Camera.main.transform.position);
            multiplicator += 0.01f;
        }
        return origin().transform.position + path() * multiplicator;
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