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
    private bool reachedDestination = false;
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
            guide = GameObject.Instantiate(arnavigation.guidePrefab,guidePosition(), Quaternion.LookRotation(path()));
            anim = guide.GetComponent<Animation>();
        }
    }
    #endregion Control Functions
    #region Helper Functions
    private GameObject origin()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorExchanger.anchorOrder[originId]];

    }
    private GameObject destination()
    {
        return arnavigation.allspawnedObjects[arnavigation.anchorExchanger.anchorOrder[destinationId]];
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
        if (originId == 1 && guideProgress() <= 0.03f && !reachedDestination)
        { // At start
            guide.transform.position = guidePosition();
            guide.transform.rotation = guideToCamera();
            anim.Play("0|shake_0");
        } else if ((destinationId == arnavigation.anchorExchanger.anchorOrder.Count && guideProgress() >= 0.90f )|| reachedDestination)
        { // At destination
            reachedDestination = true;
            guide.transform.rotation = guideToCamera();
            arnavigation.feedbackBox.text = "Reached Destination";
            anim.Play("0|rollover_0");
        } else
        { // On the move
            guide.transform.position = guidePosition();
            guide.transform.rotation = Quaternion.LookRotation(path());
            anim.Play("0|standing_0");
        }

    }
    private float guideProgress()
    {
        float multiplicator = 0.0f;
        float newDistance = 999999.0f;
        float previousDistance = 99999999.0f;
        for (multiplicator = 0.0f; newDistance < previousDistance && multiplicator <= 1.0f; multiplicator += 0.002f)
        {
            previousDistance = newDistance;
            newDistance = Vector3.Distance(origin().transform.position + path() * multiplicator, Camera.main.transform.position);
        }
        return multiplicator;
    }
    private Vector3 guidePosition()
    {
        float multiplicator = guideProgress() * 1.4f;
        if (multiplicator >= 1)
        {
            if (destinationId < arnavigation.anchorExchanger.anchorOrder.Count)
            {
                originId += 1;
                destinationId += 1;
            }
            return origin().transform.position + path() * 1.0f;
        }
        //arnavigation.printmsg = "MP:" + multiplicator + " OR:" + originId + "DS" + destinationId;
        return origin().transform.position + path() * multiplicator;
    }

}