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
        public Dictionary<string, int> anchorTypes = new Dictionary<string, int>();
        public Dictionary<int, string> anchorOrder = new Dictionary<int, string>();
        public int anchorAmount = -1;
        public async  void GetAnchors(string exchangerUrl)
        {
            baseAddress = exchangerUrl;
            try
            {
                HttpClient client = new HttpClient();
                string rawmessage = await client.GetStringAsync(baseAddress);
                string msg2 = rawmessage.Trim(new char[] { '[', ']' });
                string msg3 = msg2.Replace("},{", "};{");
                string[] msg4 = msg3.Split(';');
                for (int i = 0; i < msg4.Length; i++)
                {
                    Anchor webAnchor = JsonUtility.FromJson<Anchor>(msg4[i]);
                    anchorTypes.Add(webAnchor.anchorKey, webAnchor.anchorType);
                    anchorOrder.Add(webAnchor.id, webAnchor.anchorKey);
                }
                anchorAmount = anchorTypes.Count;
            }
            catch (Exception)
            {
                anchorAmount = 0;
            }
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

        internal async Task<long> StoreAnchorKey(string anchorKey)
        {
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
