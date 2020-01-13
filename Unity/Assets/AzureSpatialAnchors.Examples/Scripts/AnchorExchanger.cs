// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class AnchorExchanger
    {

        private string baseAddress = "";

        public Dictionary<string, int> anchorkeys = new Dictionary<string, int>();

        public Dictionary<string, int> AnchorKeys
        {
            get
            {
                lock (anchorkeys)
                {
                    return new Dictionary<string, int>(anchorkeys);
                }
            }
        }

        public void WatchKeys(string exchangerUrl)
        {
            baseAddress = exchangerUrl;
            Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        for (int i = 1; 1<5; i++)
                        {
                            string currentKey = await RetrieveAnchorKey(i);
                            int currentType = await RetrieveAnchorType(i);
                            if (!string.IsNullOrWhiteSpace(currentKey))
                            {
                                Debug.Log("Found key " + currentKey);
                                lock (anchorkeys)
                                {
                                    anchorkeys.Add(currentKey, currentType);
                                }
                            }
                        }
                    }
                }, TaskCreationOptions.LongRunning);
        }
        public int anchorType(string anchorKey)
        {
            int defaultType = 1;
            
            if (anchorkeys.ContainsKey(anchorKey))
            {
                return anchorkeys[anchorKey];
            }
            return defaultType;
        }

        public async Task<string> RetrieveAnchorKey(long anchorNumber)
        {
            try
            {
                HttpClient client = new HttpClient();
                return await client.GetStringAsync(baseAddress + "/" + anchorNumber.ToString());
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
                return int.Parse(await client.GetStringAsync(baseAddress + "/type/" + anchorNumber.ToString()));

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Failed to retrieve anchor key for anchor number: {anchorNumber}.");
                return 0;
            }
        }


        public async Task<string> RetrieveLastAnchorKey()
        {
            try
            {
                HttpClient client = new HttpClient();
                return await client.GetStringAsync(baseAddress + "/last");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Failed to retrieve last anchor key.");
                return null;
            }
        }

        public async Task<int> RetrieveLastAnchorType()
        {
            try
            {
                HttpClient client = new HttpClient();
                
                return int.Parse(await client.GetStringAsync(baseAddress + "/last/type"));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Failed to retrieve last anchor key.");
                return 0;
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
