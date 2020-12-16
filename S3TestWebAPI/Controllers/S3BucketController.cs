﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using S3TestWebAPI.Services;

namespace S3TestWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/S3Bucket")]
    public class S3BucketController : Controller
    {
        private readonly IS3Service _service;

        public S3BucketController(IS3Service service)
        {
            _service = service;
                 
        }

        [HttpPost("{bucketName}")]
        public async Task<IActionResult> CreateBucket([FromRoute] string bucketName)
        {
            var response = await _service.CreateBucketAsync(bucketName);
            return Ok(response);
        }

        [HttpPost]
        [Route("AddFile/{bucketName}")]
        public async Task<IActionResult> AddFile([FromRoute] string bucketName)
        {
            await _service.UploadFileAsync(bucketName);
            return Ok();
        }

        [HttpGet]
        [Route("GetFile/{bucketName}")]
        public async Task<IActionResult> GetObjectFormS3Async([FromRoute] string bucketName)
        {
            await _service.GetObjectFromS3Async(bucketName);
            return Ok();
        }
    }
}
