using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using UnityEngine;

public class NavigationGuide : MonoBehaviour
{
    #region Object Members
    private ARNavigation arnavigation;
    private GameObject guide = null;
    private Animation anim;
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
            updateBehaviour();
        }
    }
    private void spawnGuide()
    {
        if(guide == null)
        {
            guide = GameObject.Instantiate(arnavigation.guidePrefab,guidePosition(), guideDirection());
            anim = guide.GetComponent<Animation>();
        }
    }
    #endregion Control Functions
    #region Helper Functions
    private GameObject origin()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[originId]];

    }
    private GameObject destination()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorOrder[destinationId]];
    }
    private void moveToNextObject()
    {
        if (destinationId < arnavigation.anchorOrder.Count)
        {
            originId += 1;
            destinationId += 1;
        }
    }
    private Quaternion guideDirection()
    {
        return Quaternion.LookRotation(path());
    }
    private Quaternion guideToCamera()
    {
        
        return Quaternion.LookRotation(new Vector3(Camera.main.transform.position.x - guide.transform.position.x, 0, Camera.main.transform.position.z - guide.transform.position.z));
    }
    private Vector3 path()
    {
        return destination().transform.position - origin().transform.position;
    }
    #endregion Helper Functions
    private void updateBehaviour()
    {
        guide.transform.position = guidePosition();
        if (originId == 1 && guideProgression() <= 0.1f )
        { // At start
            guide.transform.rotation = guideToCamera();
            anim.Play("0|shake_0");
        } else if (destinationId == arnavigation.anchorOrder.Count && guideProgression() >= 0.9f)
        { // At destination
            guide.transform.rotation = guideToCamera();
            anim.Play("0|rollover_0");
        } else
        { // On the move
            guide.transform.rotation = guideDirection();
            anim.Play("0|standing_0");
        }

    }

    private float guideProgression()
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
        return multiplicator;
    }
    private Vector3 guidePosition()
    {
        float multiplicator = guideProgression() * 1.3f;
        if (multiplicator >= 1)
        {
            moveToNextObject();
            return origin().transform.position + path() * 1.0f;
        }
        //arnavigation.printmsg = "MP:" + multiplicator + " OR:" + originId + "DS" + destinationId;
        return origin().transform.position + path() * multiplicator;
    }

}