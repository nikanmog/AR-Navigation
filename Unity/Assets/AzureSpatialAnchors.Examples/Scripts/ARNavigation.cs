// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class ARNavigation : InputInteractionBase
    {
        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("Guide")]
        public GameObject guidePrefab = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab0 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab1 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab2 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab3 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab4 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab5 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab6 = null;
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab7 = null;
        private GameObject[] allPrefabs
        {
            get
            {
                return new GameObject[8] {
                    anchoredObjectPrefab0, anchoredObjectPrefab1, anchoredObjectPrefab2, anchoredObjectPrefab3, anchoredObjectPrefab4, anchoredObjectPrefab5, anchoredObjectPrefab6, anchoredObjectPrefab7
                };
            }
        }
        [SerializeField]
        [Tooltip("SpatialAnchorManager instance to use for this demo. This is required.")]
        private SpatialAnchorManager CloudManager = null;
        #endregion
        #region Helper Variables
        private enum AppState
        {
            Placing = 0,
            Saving,
            Initializing,
            ReadyToSearch,
            Searching,
            ReadyToNeighborQuery,
            Neighboring,
        }
        private AppState currentAppState = AppState.Initializing;
        private bool isErrorActive = false;
        private string printmsg = "";
        private readonly int numToMake = 7;
        private int locatedCount = 0;
        #endregion Helper Variables
        #region Class References
        private AnchorExchanger anchorExchanger = new AnchorExchanger();
        private AnchorLocateCriteria anchorLocateCriteria = null;
        private CloudSpatialAnchor currentCloudAnchor;
        private CloudSpatialAnchorWatcher currentWatcher;
        private NavigationGuide guide;
        #endregion Class References
        #region Game Objects
        private GameObject spawnedObject = null;
        private Material spawnedObjectMat = null;
        private Text feedbackBox;
        public Dictionary<int, string> anchorOrder = new Dictionary<int, string>();
        private Dictionary<string, int> anchorTypes = new Dictionary<string, int>();
        public Dictionary<string, GameObject> allspawnedObjects = new Dictionary<string, GameObject>();
        private readonly List<Material> allSpawnedMaterials = new List<Material>();
        #endregion Game Objects

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public override void Start()
        {
            guide = new NavigationGuide(this);
            string BaseSharingUrl = Resources.Load<SpatialAnchorSamplesConfig>("SpatialAnchorSamplesConfig").BaseSharingURL;
            Uri result;
            if (Uri.TryCreate(BaseSharingUrl, UriKind.Absolute, out result))
            {
                BaseSharingUrl = $"{result.Scheme}://{result.Host}/api/anchors";
            }
            anchorExchanger.GetAnchors(BaseSharingUrl);
            feedbackBox = XRUXPicker.Instance.GetFeedbackText();
            CloudManager.SessionUpdated += CloudManager_SessionUpdated;
            CloudManager.AnchorLocated += CloudManager_AnchorLocated;
            CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
            CloudManager.LogDebug += CloudManager_LogDebug;
            CloudManager.Error += CloudManager_Error;
            anchorLocateCriteria = new AnchorLocateCriteria();
            base.Start();
            
        }
        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            guide.Update();
            feedbackBox.text = $"Mode: {currentAppState}, AnchorIDs Count: {anchorTypes.Count}, locatedCount: {locatedCount}, print: {printmsg}";
            base.Update();
            switch (currentAppState)
            {
                case AppState.Initializing:
                    anchorTypes = anchorExchanger.anchorTypes;
                    anchorOrder = anchorExchanger.anchorOrder;
                    if (anchorExchanger.anchorAmount == 0)
                    {
                        currentAppState = AppState.Placing;
                    }
                    if (anchorTypes.Count == anchorExchanger.anchorAmount && anchorExchanger.anchorAmount > 0)
                    {
                        currentAppState = AppState.ReadyToSearch;
                    }
                    break;
                case AppState.Neighboring:
                    feedbackBox.text = $"Explore the office to find all markers. {locatedCount}/{numToMake - 1}";
                    if (locatedCount == numToMake - 1)
                    {
                        feedbackBox.text = "";
                    }
                    break;
            }

        }
        public async Task AdvanceDemoAsync()
        {
            switch (currentAppState)
            {
                case AppState.Placing:
                    printmsg = "adv1a";
                    if (spawnedObject != null)
                    {
                        currentAppState = AppState.Saving;
                        if (!CloudManager.IsSessionStarted)
                        {
                            await CloudManager.StartSessionAsync();
                        }
                        await SaveCurrentObjectAnchorToCloudAsync();
                    }
                    printmsg = "adv1b";
                    break;
                case AppState.ReadyToSearch:
                    printmsg = "adv2a";
                    await CloudManager.ResetSessionAsync();
                    await CloudManager.StartSessionAsync();
                    SetGraphEnabled(true);
                    anchorLocateCriteria.Identifiers = new List<String>(anchorTypes.Keys).ToArray();
                    locatedCount = 0;
                    currentWatcher = CloudManager.Session.CreateWatcher(anchorLocateCriteria);
                    currentAppState = AppState.Searching;
                    printmsg = "adv2b";
                    break;
                case AppState.ReadyToNeighborQuery:
                    printmsg = "adv3a";
                    SetGraphEnabled(true);
                    anchorTypes = new Dictionary<string, int>();
                    SetNearbyAnchor(currentCloudAnchor, 20, numToMake);
                    locatedCount = 0;
                    currentWatcher = CloudManager.Session.CreateWatcher(anchorLocateCriteria);
                    currentAppState = AppState.Neighboring;
                    printmsg = "adv3b";
                    break;

            }
        }
        protected void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            if (args.Status == LocateAnchorStatus.Located)
            {
                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    locatedCount++;
                    currentCloudAnchor = args.Anchor;
                    Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                    anchorPose = currentCloudAnchor.GetPose();
#endif
                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                    SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

                    spawnedObject = null;

                    if (currentAppState == AppState.Searching)
                    {
                        currentAppState = AppState.ReadyToNeighborQuery;
                    }
                });
            }
        }
        protected async Task OnSaveCloudAnchorSuccessfulAsync()
        {

            Debug.Log("Anchor created, yay!");
            anchorTypes.Add(currentCloudAnchor.Identifier, 0);

            await anchorExchanger.StoreAnchorKey(currentCloudAnchor.Identifier);

            // Sanity check that the object is still where we expect
            Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
            anchorPose = currentCloudAnchor.GetPose();
#endif
            // HoloLens: The position will be set based on the unityARUserAnchor that was located.


            SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

            spawnedObject = null;
            currentCloudAnchor = null;

            if (allspawnedObjects.Count < numToMake)
            {
                feedbackBox.text = $"Saved...Make another {allspawnedObjects.Count}/{numToMake} ";
                currentAppState = AppState.Placing;
                CloudManager.StopSession();
            }
            else
            {
                feedbackBox.text = "Saved... ready to start finding them.";
                CloudManager.StopSession();
                currentAppState = AppState.ReadyToSearch;
            }
        }
        /// <summary>
        /// Saves the current object anchor to the cloud.
        /// </summary>
        protected virtual async Task SaveCurrentObjectAnchorToCloudAsync()
        {
            // Get the cloud-native anchor behavior
            CloudNativeAnchor cna = spawnedObject.GetComponent<CloudNativeAnchor>();

            // If the cloud portion of the anchor hasn't been created yet, create it
            if (cna.CloudAnchor == null) { cna.NativeToCloud(); }

            // Get the cloud portion of the anchor
            CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;

            // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
            cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

            while (!CloudManager.IsReadyForCreate)
            {
                await Task.Delay(330);
                float createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
                feedbackBox.text = $"Move your device to capture more environment data: {createProgress:0%}";
                // scanImage.SetActive(true);
            }

            bool success = false;
            //scanImage.SetActive(false);
            feedbackBox.text = "Saving...";

            try
            {
                // Actually save
                await CloudManager.CreateAnchorAsync(cloudAnchor);

                // Store
                currentCloudAnchor = cloudAnchor;

                // Success?
                success = currentCloudAnchor != null;

                if (success && !isErrorActive)
                {
                    // Await override, which may perform additional tasks
                    // such as storing the key in the AnchorExchanger
                    await OnSaveCloudAnchorSuccessfulAsync();
                }
                else
                {
                    OnSaveCloudAnchorFailedB(new Exception("Failed to save, but no exception was thrown."));
                }
            }
            catch (Exception ex)
            {
                OnSaveCloudAnchorFailedB(ex);
            }
        }
        protected void SetNearbyAnchor(CloudSpatialAnchor nearbyAnchor, float DistanceInMeters, int MaxNearAnchorsToFind)
        {
            if (nearbyAnchor == null)
            {
                anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();
                return;
            }

            NearAnchorCriteria nac = new NearAnchorCriteria();
            nac.SourceAnchor = nearbyAnchor;
            nac.DistanceInMeters = DistanceInMeters;
            nac.MaxResultCount = MaxNearAnchorsToFind;
            anchorLocateCriteria.NearAnchor = nac;
        }
        protected void SetGraphEnabled(bool UseGraph, bool JustGraph = false)
        {
            anchorLocateCriteria.Strategy = UseGraph ?
                                            (JustGraph ? LocateStrategy.Relationship : LocateStrategy.AnyStrategy) :
                                            LocateStrategy.VisualInformation;
        }
        #region Object Handlers
        /// <summary>
        /// Spawns a new anchored object.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <param name="worldRot">The world rotation.</param>
        /// <returns><see cref="GameObject"/>.</returns>
        protected virtual GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot, int type)
        {
            // Create the prefab
            GameObject newGameObject = GameObject.Instantiate(allPrefabs[type], worldPos, worldRot);
            // Attach a cloud-native anchor behavior to help keep cloud
            // and native anchors in sync.
            newGameObject.AddComponent<CloudNativeAnchor>();
            // Return created object
            return newGameObject;
        }
        /// <summary>
        /// Spawns a new object.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <param name="worldRot">The world rotation.</param>
        /// <param name="cloudSpatialAnchor">The cloud spatial anchor.</param>
        /// <returns><see cref="GameObject"/>.</returns>
        protected virtual GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor)
        {
            // If a cloud anchor is passed, apply it to the native anchor
            if (cloudSpatialAnchor != null)
            {
                GameObject newGameObject = SpawnNewAnchoredObject(worldPos, worldRot, anchorExchanger.anchorType(cloudSpatialAnchor.Identifier));
                CloudNativeAnchor cloudNativeAnchor = newGameObject.GetComponent<CloudNativeAnchor>();
                cloudNativeAnchor.CloudToNative(cloudSpatialAnchor);
                return newGameObject;
            }
            else
            {
                return SpawnNewAnchoredObject(worldPos, worldRot, 0);
            }
        }
        protected void SpawnOrMoveCurrentAnchoredObject(Vector3 worldPos, Quaternion worldRot)
        {

            if (currentCloudAnchor != null && allspawnedObjects.ContainsKey(currentCloudAnchor.Identifier))
            {
                spawnedObject = allspawnedObjects[currentCloudAnchor.Identifier];
            }
            bool spawnedNewObject = spawnedObject == null;

            // Create the object if we need to, and attach the platform appropriate
            // Anchor behavior to the spawned object
            if (spawnedObject == null)
            {
                // Use factory method to create
                spawnedObject = SpawnNewAnchoredObject(worldPos, worldRot, currentCloudAnchor);

                // Update color
                spawnedObjectMat = spawnedObject.GetComponent<MeshRenderer>().material;
            }
            else
            {
                // Use factory method to move
                MoveAnchoredObject(spawnedObject, worldPos, worldRot, currentCloudAnchor);
            }

            if (spawnedNewObject)
            {
                allSpawnedMaterials.Add(spawnedObjectMat);

                if (currentCloudAnchor != null && allspawnedObjects.ContainsKey(currentCloudAnchor.Identifier) == false)
                {
                    allspawnedObjects.Add(currentCloudAnchor.Identifier, spawnedObject);
                }
            }

#if WINDOWS_UWP || UNITY_WSA
            if (currentCloudAnchor != null && spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier) == false)
            {
                allspawnedObjects.Add(currentCloudAnchor.Identifier, spawnedObject);
            }
#endif
        }
        /// <summary>
        /// Moves the specified anchored object.
        /// </summary>
        /// <param name="objectToMove">The anchored object to move.</param>
        /// <param name="worldPos">The world position.</param>
        /// <param name="worldRot">The world rotation.</param>
        /// <param name="cloudSpatialAnchor">The cloud spatial anchor.</param>
        protected virtual void MoveAnchoredObject(GameObject objectToMove, Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor = null)
        {
            // Get the cloud-native anchor behavior
            CloudNativeAnchor cna = spawnedObject.GetComponent<CloudNativeAnchor>();

            // Warn and exit if the behavior is missing
            if (cna == null)
            {
                Debug.LogWarning($"The object {objectToMove.name} is missing the {nameof(CloudNativeAnchor)} behavior.");
                return;
            }

            // Is there a cloud anchor to apply
            if (cloudSpatialAnchor != null)
            {
                // Yes. Apply the cloud anchor, which also sets the pose.
                cna.CloudToNative(cloudSpatialAnchor);
            }
            else
            {
                // No. Just set the pose.
                cna.SetPose(worldPos, worldRot);
            }
        }
        #endregion Object Handlers
        #region Unity Handlers
        /// <summary>
        /// This version only exists for Unity to wire up a button click to.
        /// If calling from code, please use the Async version above.
        /// </summary>
        public async void AdvanceDemo()
        {
            try
            {
                await AdvanceDemoAsync();
            }
            catch (Exception)
            {
                feedbackBox.text = $"Demo failed, check debugger output for more information";
            }
        }
        /// <summary>
        /// Destroying the attached Behaviour will result in the game or Scene
        /// receiving OnDestroy.
        /// </summary>
        /// <remarks>OnDestroy will only be called on game objects that have previously been active.</remarks>
        public override void OnDestroy()
        {
            if (CloudManager != null)
            {
                CloudManager.StopSession();
            }

            if (currentWatcher != null)
            {
                currentWatcher.Stop();
                currentWatcher = null;
            }

            CleanupSpawnedObjects();

            // Pass to base for final cleanup
            base.OnDestroy();
        }
        /// <summary>
        /// Cleans up spawned objects.
        /// </summary>
        protected virtual void CleanupSpawnedObjects()
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
                spawnedObject = null;
            }

            if (spawnedObjectMat != null)
            {
                Destroy(spawnedObjectMat);
                spawnedObjectMat = null;
            }
        }
        #endregion Unity Handlers
        #region Interaction Handlers
        /// <summary>
        /// Called when gaze interaction occurs.
        /// </summary>
        protected override void OnGazeInteraction()
        {
        #if WINDOWS_UWP || UNITY_WSA
            // HoloLens gaze interaction
            if (IsPlacingObject())
            {
                base.OnGazeInteraction();
            }
        #endif
        }
        /// <summary>
        /// Called when gaze interaction begins.
        /// </summary>
        /// <param name="hitPoint">The hit point.</param>
        /// <param name="target">The target.</param>
        protected override void OnGazeObjectInteraction(Vector3 hitPoint, Vector3 hitNormal)
        {
            base.OnGazeObjectInteraction(hitPoint, hitNormal);
            if (currentAppState == AppState.Placing)
            {
        #if WINDOWS_UWP || UNITY_WSA
                Vector3 direction = new Vector3(Camera.main.transform.position.x - hitPoint.x, Camera.main.transform.position.y - hitPoint.y, Camera.main.transform.position.z - hitPoint.z);
                Quaternion rotation = Quaternion.LookRotation(direction);
                SpawnOrMoveCurrentAnchoredObject(hitPoint, rotation);
        #endif
            }
        }
        /// <summary>
        /// Called when a cloud anchor is not saved successfully.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected virtual void OnSaveCloudAnchorFailedB(Exception exception)
        {
            // we will block the next step to show the exception message in the UI.
            isErrorActive = true;
            Debug.LogException(exception);
            Debug.Log("Failed to save anchor " + exception.ToString());
            UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", exception.ToString()));
        }
        /// <summary>
        /// Called when a select interaction occurs.
        /// </summary>
        /// <remarks>Currently only called for HoloLens.</remarks>
        protected override void OnSelectInteraction()
        {
        #if WINDOWS_UWP || UNITY_WSA
            // On HoloLens, we just advance the demo.
            UnityDispatcher.InvokeOnAppThread(() => advanceDemoTask = AdvanceDemoAsync());
        #endif
            base.OnSelectInteraction();
        }
        /// <summary>
        /// Called when a touch object interaction occurs.
        /// </summary>
        /// <param name="hitPoint">The position.</param>
        /// <param name="target">The target.</param>
        protected override void OnSelectObjectInteraction(Vector3 hitPoint, object target)
        {
            if (currentAppState == AppState.Placing)
            {
                Vector3 direction = new Vector3(Camera.main.transform.position.x - hitPoint.x, Camera.main.transform.position.y - hitPoint.y, Camera.main.transform.position.z - hitPoint.z);
                Quaternion rotation = Quaternion.LookRotation(direction);
                SpawnOrMoveCurrentAnchoredObject(hitPoint, rotation);
            }
        }
        /// <summary>
        /// Called when a touch interaction occurs.
        /// </summary>
        /// <param name="touch">The touch.</param>
        protected override void OnTouchInteraction(Touch touch)
        {
            if (currentAppState == AppState.Placing)
            {
                base.OnTouchInteraction(touch);
            }
        }
        #endregion Interaction Handlers
        #region Cloud Manager Default Handlers
        /// <summary>
        /// Called when cloud anchor location has completed.
        /// </summary>
        /// <param name="args">The <see cref="LocateAnchorsCompletedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloudLocateAnchorsCompleted(LocateAnchorsCompletedEventArgs args)
        {
            Debug.Log("Locate pass complete");
        }
        protected void OnSaveCloudAnchorFailed(Exception exception)
        {
            OnSaveCloudAnchorFailed(exception);
        }
        private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            Debug.LogFormat("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
            if (args.Status == LocateAnchorStatus.Located)
            {
                OnCloudAnchorLocated(args);
            }
        }
        private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            OnCloudLocateAnchorsCompleted(args);
        }
        private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
        }
        private void CloudManager_Error(object sender, SessionErrorEventArgs args)
        {
            isErrorActive = true;
            Debug.Log(args.ErrorMessage);
            UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", args.ErrorMessage));
        }
        private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
        {
            Debug.Log(args.Message);
        }
        #endregion Cloud Manager Default Handlers
    }
}