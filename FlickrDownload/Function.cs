using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Newtonsoft.Json;

namespace FlickrDownload
{
    public class Function
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        private readonly IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USWest2);

        /// <summary>
        /// This function downloads all photos from a particular user tagged with a particular tag, and saves medium and large copies of them in an S3 bucket.
        /// </summary>
        public async Task FunctionHandler(Stream input)
        {
            string flickrApiKey = Environment.GetEnvironmentVariable("FLICKR_API_KEY");
            var result = await client.GetStringAsync($"https://www.flickr.com/services/rest/?api_key={flickrApiKey}&method=flickr.photos.search&user_id=93665003%40N05&format=json&nojsoncallback=1&extras=url_s%2Curl_m%2Curl_l");

            var deserializedPhotoResponse = JsonConvert.DeserializeObject<PhotoApiResponse>(result);
            var numPhotos = deserializedPhotoResponse.Photos.Photo.Count;

            var allDownloadTasks = new List<Task>();
            for (var i = 0; i < numPhotos; i++)
            {
                var curPhoto = deserializedPhotoResponse.Photos.Photo[i];
                allDownloadTasks.Add(DownloadPhoto(curPhoto.Url_m, $"medium/{i}.jpg"));
                allDownloadTasks.Add(DownloadPhoto(curPhoto.Url_l, $"large/{i}.jpg"));
            }

            await Task.WhenAll(allDownloadTasks);
        }

        /// <summary>
        /// Downloads a single photo, converts to a memory stream, and saves to S3 bucket.
        /// </summary>
        /// <param name="url">URL from which to download photo</param>
        /// <param name="fileKey">Object key for the photo in the bucket</param>
        /// <returns></returns>
        private async Task DownloadPhoto(string url, string fileKey)
        {
            using(var fileTransferUtility = new TransferUtility(s3Client))
            using (var result = await client.GetStreamAsync(url))
            using (var memoryStream = new MemoryStream())
            {
                await result.CopyToAsync(memoryStream);
                await fileTransferUtility.UploadAsync(memoryStream, bucketName, fileKey);
            }
        }
    }
}
