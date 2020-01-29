// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using Microsoft.AspNetCore.Mvc;
using SharingService.Data;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SharingService.Controllers
{

    [Route("api/anchors")]
    [ApiController]
    public class AnchorsController : ControllerBase
    {
        private readonly CosmosDbCache anchorKeyCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorsController"/> class.
        /// </summary>
        /// <param name="anchorKeyCache">The anchor key cache.</param>
        public AnchorsController(CosmosDbCache anchorKeyCache)
        {
            this.anchorKeyCache = anchorKeyCache;
        }




        // GET api/anchors/5
        [HttpGet("{anchorNumber}/key")]
        public async Task<ActionResult<string>> GetAsync(long anchorNumber)
        {
            // Get the key if present
            try
            {
                return await this.anchorKeyCache.GetAnchorKeyAsync(anchorNumber);
            }
            catch(KeyNotFoundException)
            {
                return this.NotFound();
            }
        }


        // GET api/type/5
        [HttpGet("{anchorNumber}/type")]
        public async Task<ActionResult<int>> GetTypeAsync(long anchorNumber)
        {
            // Get the key if present
            try
            {
                return await this.anchorKeyCache.GetAnchorTypeAsync(anchorNumber);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
        }

        // GET api/anchors/last
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCountAsync()
        {
            // Get the last anchor
            AnchorCacheEntity anchor = await this.anchorKeyCache.GetLastAnchorAsync();

            if (anchor == null)
            {
                return 0;
            }

            return int.Parse(anchor.RowKey);
        }

        // POST api/anchors
        [HttpPost]
        public async Task<ActionResult<long>> PostAsync()
        {
            string anchorKey;
            using (StreamReader reader = new StreamReader(this.Request.Body, Encoding.UTF8))
            {
                anchorKey = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(anchorKey))
            {
                return this.BadRequest();
            }

            // Set the key and return the anchor number
            return await this.anchorKeyCache.SetAnchorKeyAsync(anchorKey);
        }


        // POST api/delete
        [HttpPost("delete")]
        public void DeleteAsync()
        {
            
            this.anchorKeyCache.DeleteTable();
        }

        // POST api/update
        [HttpPost("update")]
        public void UpdateAsync()
        {
            this.anchorKeyCache.UpdateTable();
        }

    }
}
