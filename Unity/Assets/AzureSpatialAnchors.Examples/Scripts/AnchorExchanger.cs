// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    [Serializable]
    public class Anchor
    {
        public Anchor(int id, string demo, string anchorKey, int anchorType)
        {
            this.id = id;
            this.demo = demo;
            this.anchorKey = anchorKey;
            this.anchorType = anchorType;
        }
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
        public async void getAnchors()
        {
            
            baseAddress = PlayerPrefs.GetString("Webservice");
            Uri result;
            if (Uri.TryCreate(baseAddress, UriKind.Absolute, out result))
            {
                baseAddress = $"{result.Scheme}://{result.Host}/api/anchors";
            }
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
        internal async Task<string> StoreAnchorKey(string anchorKey)
        {
            try
            {
                HttpClient client = new HttpClient();
                Anchor anchor = new Anchor(0, "Standard", anchorKey, 0);
                string jsoncontent = JsonUtility.ToJson(anchor);
                var buffer = System.Text.Encoding.UTF8.GetBytes(jsoncontent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var result = await client.PostAsync(baseAddress, byteContent);
                if (result.IsSuccessStatusCode){
                    anchorTypes.Add(anchorKey, 0);
                    return "";
                }
                else
                {
                    return result.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
