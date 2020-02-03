// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    [Serializable]
    public class Anchor
    {
        public int id;
        public string demo;
        public string anchorKey;
        public int anchorType;
    }
    public class AnchorExchanger
    {
        private string baseAddress = "";
        public string aemsg = "X";
        private List<Anchor> anchors;
        public Dictionary<string, int> anchorTypes = new Dictionary<string, int>();
        public Dictionary<int, string> anchorOrder = new Dictionary<int, string>();
        public int anchorAmount = -1;
        public void GetAnchors(string exchangerUrl)
        {
            baseAddress = exchangerUrl;
            getAnchors(exchangerUrl);
            
            /*Task.Factory.StartNew(async () =>
            {
                anchorAmount = await RetrieveAnchorAmount();
                for (int i = 1; 1 <= anchorAmount ; i++)
                {
                    string currentKey = await RetrieveAnchorKey(i);
                    int currentType = await RetrieveAnchorType(i);
                    if (!string.IsNullOrWhiteSpace(currentKey))
                    {
                        Debug.Log("Found key " + currentKey);
                        lock (anchorTypes)
                        {
                            anchorTypes.Add(currentKey, currentType);
                        }
                        lock (anchorOrder)
                        {
                            anchorOrder.Add(i, currentKey);
                        }
                        
                    }
                }
            }, TaskCreationOptions.LongRunning);
            */
        }
        public int anchorType(string anchorKey)
        {
            int defaultType = 1;

            if (anchorTypes.ContainsKey(anchorKey))
            {
                return anchorTypes[anchorKey];
            }
            return defaultType;
        }
        public async Task<string> RetrieveAnchorKey(long anchorNumber)
        {
            try
            {
                HttpClient client = new HttpClient();
                return await client.GetStringAsync(baseAddress + "/" + anchorNumber.ToString() + "/key");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Failed to retrieve anchor key for anchor number: {anchorNumber}.");
                return null;
            }
        }
        public async Task<int> RetrieveAnchorType(long anchorNumber)
        {
            try
            {
                HttpClient client = new HttpClient();
                return int.Parse(await client.GetStringAsync(baseAddress + "/" + anchorNumber.ToString() + "/type"));

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Failed to retrieve anchor key for anchor number: {anchorNumber}.");
                return 0;
            }
        }
        public async Task<int> RetrieveAnchorAmount()
        {
            try
            {
                HttpClient client = new HttpClient();

                return int.Parse(await client.GetStringAsync(baseAddress + "/count"));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Failed to retrieve last anchor key.");
                return 0;
            }
        }
        private async void getAnchors(string exchangeURL)
        {
            string rawmessage = "[]";
            try
            {
                HttpClient client = new HttpClient();
                rawmessage = await client.GetStringAsync(baseAddress);
            }
            catch (Exception)
            {
                rawmessage = "[]";
            }
            if (rawmessage == "[]")
            {
                anchorAmount = 0;
            }
            else
            {
                string[] splitmessage = rawmessage.Trim(new char[] { '[', ']' }).Replace("},{", "};{").Split(';');
                Anchor anchorObject = JsonUtility.FromJson<Anchor>(splitmessage[0]);
                anchorTypes.Add(anchorObject.anchorKey, anchorObject.anchorType);
                anchorOrder.Add(anchorObject.id, anchorObject.anchorKey);
                anchors.Add(anchorObject);
                anchorAmount = 1;
                aemsg = anchorObject.anchorKey;
                /*
                foreach (String singlemessage in splitmessage)
                {
                    aemsg = singlemessage;
                    Anchor anchorObject = JsonUtility.FromJson<Anchor>(singlemessage);
                    anchors.Add(anchorObject);
                    lock (anchorTypes)
                    {
                        anchorTypes.Add(anchorObject.anchorKey, anchorObject.anchorType);
                    }
                    lock (anchorOrder)
                    {
                        anchorOrder.Add(anchorObject.id, anchorObject.anchorKey);
                    }
                }

                anchorAmount = anchors.Count;
                */
            }

        }
        internal async Task<long> StoreAnchorKey(string anchorKey)
        {
            if (string.IsNullOrWhiteSpace(anchorKey))
            {
                return -1;
            }

            try
            {
                HttpClient client = new HttpClient();
                var response = await client.PostAsync(baseAddress, new StringContent(anchorKey));
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    long ret;
                    if (long.TryParse(responseBody, out ret))
                    {
                        Debug.Log("Key " + ret.ToString());
                        return ret;
                    }
                    else
                    {
                        Debug.LogError($"Failed to store the anchor key. Failed to parse the response body to a long: {responseBody}.");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to store the anchor key: {response.StatusCode} {response.ReasonPhrase}.");
                }

                Debug.LogError($"Failed to store the anchor key: {anchorKey}.");
                return -1;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Failed to store the anchor key: {anchorKey}.");
                return -1;
            }
        }
    }
}
