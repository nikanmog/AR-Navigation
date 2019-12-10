// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class AzureSpatialAnchorsNearbyDemoScript : DemoScriptBase
    {
        //App States
        internal enum AppState
        {
            Placing = 0,
            Saving,
            Initializing,
            ReadyToSearch,
            Searching,
            ReadyToNeighborQuery,
            Neighboring,
            ModeCount
        }
        AppState currentAppState
        {
            get
            {
                return _currentAppState;
            }
            set
            {
                if (_currentAppState != value)
                {
                    Debug.LogFormat("State from {0} to {1}", _currentAppState, value);
                    _currentAppState = value;

                }
            }
        }
        private AppState _currentAppState = AppState.Initializing;
        // Cosmos Connection
        public string BaseSharingUrl { 
            get => baseSharingUrl; 
            set => baseSharingUrl = value; 
        }
        private string baseSharingUrl = "";
#if !UNITY_EDITOR
            public AnchorExchanger anchorExchanger = new AnchorExchanger();
#endif
        private string _anchorKeyToFind = null;
        private readonly int numToMake = 8;

        readonly List<string> anchorIds = new List<string>();
        readonly Dictionary<AppState, Dictionary<string, GameObject>> spawnedObjectsPerAppState = new Dictionary<AppState, Dictionary<string, GameObject>>();
        private readonly List<GameObject> allSpawnedObjects = new List<GameObject>();
        private readonly List<Material> allSpawnedMaterials = new List<Material>();

        Dictionary<string, GameObject> spawnedObjectsInCurrentAppState
        {
            get
            {
                if (spawnedObjectsPerAppState.ContainsKey(_currentAppState) == false)
                {
                    spawnedObjectsPerAppState.Add(_currentAppState, new Dictionary<string, GameObject>());
                }

                return spawnedObjectsPerAppState[_currentAppState];
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public async override void Start()
        {

            SpatialAnchorSamplesConfig samplesConfig = Resources.Load<SpatialAnchorSamplesConfig>("SpatialAnchorSamplesConfig");
            if (string.IsNullOrWhiteSpace(BaseSharingUrl) && samplesConfig != null)
            {
                BaseSharingUrl = samplesConfig.BaseSharingURL;
            }

            if (string.IsNullOrEmpty(BaseSharingUrl))
            {
                feedbackBox.text = $"Need to set {nameof(BaseSharingUrl)}.";
                XRUXPickerForSharedAnchorDemo.Instance.GetDemoButtons()[1].gameObject.SetActive(false);
                XRUXPickerForSharedAnchorDemo.Instance.GetDemoButtons()[0].gameObject.SetActive(false);
                XRUXPickerForSharedAnchorDemo.Instance.GetDemoInputField().gameObject.SetActive(false);
                return;
            }
            else
            {
                Uri result;
                if (!Uri.TryCreate(BaseSharingUrl, UriKind.Absolute, out result))
                {
                    feedbackBox.text = $"{nameof(BaseSharingUrl)} is not a valid url";
                    return;
                }
                else
                {
                    BaseSharingUrl = $"{result.Scheme}://{result.Host}/api/anchors";
                }
            }

#if !UNITY_EDITOR
                anchorExchanger.WatchKeys(BaseSharingUrl);
#endif

            await setMode();
            base.Start();
            
            if (!SanityCheckAccessConfiguration())
            {
                return;
            }

        }


        private async Task setMode()
        {
           await AdvanceDemoAsync();

            {
#if !UNITY_EDITOR
                            _anchorKeyToFind = await anchorExchanger.RetrieveAnchorKey(1); 
#endif
                if (_anchorKeyToFind != null)
                {
                    currentAppState = AppState.ReadyToSearch;
                    anchorIds.Add(_anchorKeyToFind);
                }
                else
                {
                    _currentAppState = AppState.Placing;
                }
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            base.Update();
            switch (currentAppState)
            {
                case AppState.Searching:
                    feedbackBox.text = "Please go to the starting point and look around.";
                    break;
                case AppState.Initializing:
                    feedbackBox.text = "Initializing...";
                    break;
                case AppState.ReadyToNeighborQuery:
                    feedbackBox.text = "Tap to continue";
                    break;
                case AppState.Neighboring:
                    // We should find all anchors except for the anchor we are using as the source anchor.
                    feedbackBox.text = $"Explore the office to find all markers. {locatedCount}/{numToMake - 1}";
                    
                    if (locatedCount == numToMake - 1)
                    {
                        feedbackBox.text = "";
                    }
                    break;
            }
        }

        private int locatedCount = 0;

        protected override void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            base.OnCloudAnchorLocated(args);

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

        protected override void SpawnOrMoveCurrentAnchoredObject(Vector3 worldPos, Quaternion worldRot)
        {
            if (currentCloudAnchor != null && spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier))
            {
                spawnedObject = spawnedObjectsInCurrentAppState[currentCloudAnchor.Identifier];
            }

            bool spawnedNewObject = spawnedObject == null;

            base.SpawnOrMoveCurrentAnchoredObject(worldPos, worldRot);

            if (spawnedNewObject)
            {
                allSpawnedObjects.Add(spawnedObject);
                allSpawnedMaterials.Add(spawnedObjectMat);

                if (currentCloudAnchor != null && spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier) == false)
                {
                    spawnedObjectsInCurrentAppState.Add(currentCloudAnchor.Identifier, spawnedObject);
                }
            }

            #if WINDOWS_UWP || UNITY_WSA
            if (currentCloudAnchor != null && spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier) == false)
            {
                spawnedObjectsInCurrentAppState.Add(currentCloudAnchor.Identifier, spawnedObject);
            }
            #endif
        }

        public async override Task AdvanceDemoAsync()
        {
            switch (currentAppState)
            {
                case AppState.Placing:
                    if (spawnedObject != null)
                    {
                        currentAppState = AppState.Saving;
                        if (!CloudManager.IsSessionStarted)
                        {
                            await CloudManager.StartSessionAsync();
                        }
                        await SaveCurrentObjectAnchorToCloudAsync();
                    }
                    break;
                case AppState.ReadyToSearch:
                    if (!CloudManager.IsSessionStarted)
                    {
                        await CloudManager.StartSessionAsync();
                    }
                    await CloudManager.ResetSessionAsync();
                    await CloudManager.StartSessionAsync();
                    SetGraphEnabled(false);
                    IEnumerable<string> anchorsToFind = new[] { anchorIds[0] };
                    SetAnchorIdsToLocate(anchorsToFind);
                    locatedCount = 0;
                    currentWatcher = CreateWatcher();
                    currentAppState = AppState.Searching;
                    break;
                case AppState.ReadyToNeighborQuery:
                    SetGraphEnabled(true);
                    ResetAnchorIdsToLocate();
                    SetNearbyAnchor(currentCloudAnchor, 20, numToMake);
                    locatedCount = 0;
                    currentWatcher = CreateWatcher();
                    currentAppState = AppState.Neighboring;
                    break;
                    
            }
        }

        protected override async Task OnSaveCloudAnchorSuccessfulAsync()
        {
            await base.OnSaveCloudAnchorSuccessfulAsync();

            Debug.Log("Anchor created, yay!");

            anchorIds.Add(currentCloudAnchor.Identifier);

            long anchorNumber = -1;

            anchorIds.Add(currentCloudAnchor.Identifier);

            #if !UNITY_EDITOR
                        anchorNumber = (await anchorExchanger.StoreAnchorKey(currentCloudAnchor.Identifier));
            #endif

            // Sanity check that the object is still where we expect
            Pose anchorPose = Pose.identity;

            #if UNITY_ANDROID || UNITY_IOS
            anchorPose = currentCloudAnchor.GetPose();
            #endif
            // HoloLens: The position will be set based on the unityARUserAnchor that was located.

            
            SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

            spawnedObject = null;
            currentCloudAnchor = null;
            if (allSpawnedObjects.Count < numToMake)
            {
                feedbackBox.text = $"Saved...Make another {allSpawnedObjects.Count}/{numToMake} ";
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

        protected override bool IsPlacingObject()
        {
            return currentAppState == AppState.Placing;
        }
        protected override void OnSaveCloudAnchorFailed(Exception exception)
        {
            base.OnSaveCloudAnchorFailed(exception);
        }
        protected override Color GetStepColor()
        {
            return Color.red;
        }
    }
}
