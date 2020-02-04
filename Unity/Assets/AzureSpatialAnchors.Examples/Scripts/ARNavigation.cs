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
            Initializing,
            CreatorMode,
            Saving,
            Searching,
            Finished, 
            Error
        }
        private AppState currentAppState = AppState.Initializing;
        #endregion Helper Variables
        #region Class References
        public AnchorExchanger anchorExchanger = new AnchorExchanger();
        private AnchorLocateCriteria anchorLocateCriteria = null;
        private CloudSpatialAnchor currentCloudAnchor;
        private CloudSpatialAnchorWatcher currentWatcher;
        private NavigationGuide guide;
        #endregion Class References
        #region Game Objects
        private GameObject spawnedObject = null;
        private Material spawnedObjectMat = null;
        public Text feedbackBox;

        public Dictionary<string, GameObject> allspawnedObjects = new Dictionary<string, GameObject>();
        private readonly List<Material> allSpawnedMaterials = new List<Material>();
        #endregion Game Objects
        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public override async void Start()
        {
            base.Start();
            guide = new NavigationGuide(this);
            anchorExchanger.getAnchors();
            feedbackBox = XRUXPicker.Instance.GetFeedbackText();
            CloudManager.AnchorLocated += AnchorLocated;
            anchorLocateCriteria = new AnchorLocateCriteria();
            await CloudManager.StartSessionAsync();
        }
        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            base.Update();
            guide.Update();
            if (currentAppState == AppState.Initializing)
            {
                if (anchorExchanger.anchorAmount == 0)
                {
                    XRUXPicker.Instance.EnableNextButton();
                    feedbackBox.text = "Please place an anchor and click save to proceed.";
                    currentAppState = AppState.CreatorMode;
                }
                if (anchorExchanger.anchorAmount > 0)
                {
                    feedbackBox.text = "Move your device to find existing anchors.";
                    anchorLocateCriteria.Strategy = LocateStrategy.AnyStrategy;
                    anchorLocateCriteria.Identifiers = new List<String>(anchorExchanger.anchorTypes.Keys).ToArray();
                    currentWatcher = CloudManager.Session.CreateWatcher(anchorLocateCriteria);
                    currentAppState = AppState.Searching;
                }
            }
        }
        public async Task PlaceObject()
        {
            if (currentAppState == AppState.CreatorMode && spawnedObject != null)
            {
                currentAppState = AppState.Saving;
                if (!CloudManager.IsSessionStarted)
                {
                    await CloudManager.StartSessionAsync();
                }
                await SaveAnchorAsync();
            }
        }
        private void AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            if (args.Status == LocateAnchorStatus.Located)
            {
                feedbackBox.text = "Please follow the dog to get to your destination";
                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    currentCloudAnchor = args.Anchor;
                    Pose anchorPose = Pose.identity;
                    #if UNITY_ANDROID || UNITY_IOS
                    anchorPose = currentCloudAnchor.GetPose();
                    #endif
                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.
                    SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);
                    spawnedObject = null;
                });
            }
        }
        /// <summary>
        /// Saves the current object anchor to the cloud.
        /// </summary>
        protected virtual async Task SaveAnchorAsync()
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
            }
            feedbackBox.text = "Saving Anchor to the ASA Service...";
            try
            {
                // Actually save
                await CloudManager.CreateAnchorAsync(cloudAnchor);
                // Store
                currentCloudAnchor = cloudAnchor;
                // Success?
                if (currentCloudAnchor != null && currentAppState != AppState.Error)
                {
                    feedbackBox.text = "Saving Anchor to Database...";
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
                    currentAppState = AppState.CreatorMode;
                    feedbackBox.text = "Success, place your next Anchor";
                }
                else
                {
                    currentAppState = AppState.Error;
                    this.feedbackBox.text = "There was an error in the ASA Service";
                }
            }
            catch (Exception ex)
            {
                currentAppState = AppState.Error;
                UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", ex.ToString()));
            }
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
                await PlaceObject();
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
            if (currentAppState == AppState.CreatorMode)
            {
        #if WINDOWS_UWP || UNITY_WSA
                Vector3 direction = new Vector3(Camera.main.transform.position.x - hitPoint.x, Camera.main.transform.position.y - hitPoint.y, Camera.main.transform.position.z - hitPoint.z);
                Quaternion rotation = Quaternion.LookRotation(direction);
                SpawnOrMoveCurrentAnchoredObject(hitPoint, rotation);
        #endif
            }
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
            if (currentAppState == AppState.CreatorMode)
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
            if (currentAppState == AppState.CreatorMode)
            {
                base.OnTouchInteraction(touch);
            }
        }
        #endregion Interaction Handlers
    }
}