using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using S3TestWebAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace S3TestWebAPI.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _client;

        public S3Service(IAmazonS3 client)
        {
            _client = client;
        }

        public async Task<S3Response> CreateBucketAsync(string bucketName)
        {
            try
            {
               if(await AmazonS3Util.DoesS3BucketExistAsync(_client, bucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };
                    var response = await _client.PutBucketAsync(putBucketRequest);
                    return new S3Response
                    {
                        Message = response.ResponseMetadata.RequestId,
                        Status = response.HttpStatusCode
                    };
                } 
            }
            catch (AmazonS3Exception e)
            {
                return new S3Response
                {
                    Status = e.StatusCode,
                    Message = e.Message
                };
            }
            catch (Exception e)
            {
                return new S3Response
                {
                    Status = System.Net.HttpStatusCode.InternalServerError,
                    Message = e.Message
                };
            }
            return new S3Response
            {
                Status = System.Net.HttpStatusCode.InternalServerError,
                Message = "Something went wrong"
            };
        }

        private const string FilePath = "C:\\files to delete\\awstest.txt";
        private const string UploadWithKeyName = "UploadWithKeyName";
        private const string FileStreamUpload = "FileStreamUpload";
        private const string AdvancedUpload = "AdvancedUpload";

        public async Task UploadFileAsync(string bucketName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_client);

                //option 1
                await fileTransferUtility.UploadAsync(FilePath, bucketName);

                //option 2
                await fileTransferUtility.UploadAsync(FilePath, bucketName, UploadWithKeyName);

                //option 3
                using(var filetoUpload = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    await fileTransferUtility.UploadAsync(filetoUpload, bucketName, FileStreamUpload);
                }

                //option 4
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = FilePath,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = 6291456, //6mb
                    Key = AdvancedUpload,
                    CannedACL = S3CannedACL.NoACL
                };

                fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            }
            catch(AmazonS3Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task GetObjectFromS3Async(string bucketName)
        {
            const string KeyName = "awstest.txt";
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = KeyName
                };
                string responseBody;

                using (var response = await _client.GetObjectAsync(request)) 
                using (var responseStream = response.ResponseStream)
                using (var reader = new StreamReader(responseStream))
                {
                    var title = response.Metadata["xz-amz-meta-title"];
                    var contentType = response.Headers["COntent-Type"];

                    Console.WriteLine($"Object Meta, Title:{title}");
                    Console.WriteLine($"Content type: {contentType}");

                    responseBody = reader.ReadToEnd();
                }

                var pathAndFileName = $"C:\\files to delete\\{KeyName}";

                var createText = responseBody;
                File.WriteAllText(pathAndFileName, createText);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(e);


            }
            catch (Exception e)
            {
                Console.WriteLine(e);


            }
        }
    }

   
}
