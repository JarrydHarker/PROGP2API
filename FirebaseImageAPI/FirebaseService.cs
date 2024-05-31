using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace FirebaseImageAPI
{
    public class FirebaseService
    {
        private readonly StorageClient _storageClient;
        private static readonly string _bucketName = "progpoep2.appspot.com";

        public FirebaseService()
        {
            GoogleCredential credential = GoogleCredential.FromFile("C:/Users/Jarryd/source/repos/FirebaseImageAPI/Secrets/progpoep2Key.json");
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadImageAsync(string localPath, string remotePath)
        {
            using (var fileStream = File.OpenRead(localPath))
            {
                _storageClient.UploadObject(_bucketName, remotePath, null, fileStream);
                Console.WriteLine($"File {localPath} uploaded to {remotePath}.");

                // Make the uploaded file public
                MakeFilePublic(_storageClient, remotePath);
            }

            return _storageClient.GetObject(_bucketName ,remotePath).SelfLink.ToString();
        }

        public void DownloadImageAsync(string localPath, string remotePath)
        {
            using (var outputFile = File.OpenWrite(localPath))
            {
                _storageClient.DownloadObject(_bucketName, remotePath, outputFile);
                Console.WriteLine($"File {remotePath} downloaded to {localPath}.");
            }
        }

        public void DeleteImageAsync(string bucketName, string remotePath)
        {
            _storageClient.DeleteObject(bucketName, remotePath);
            Console.WriteLine($"File {remotePath} deleted.");
        }

        static void MakeFilePublic(StorageClient storageClient, string objectName)
        {
            var storageObject = storageClient.GetObject(_bucketName, objectName);
            storageObject.Acl = storageObject.Acl ?? new List<Google.Apis.Storage.v1.Data.ObjectAccessControl>();
            storageObject.Acl.Add(new Google.Apis.Storage.v1.Data.ObjectAccessControl()
            {
                Entity = "allUsers",
                Role = "READER"
            });
            storageClient.UpdateObject(storageObject);
            Console.WriteLine($"File {objectName} is now public.");
        }

        public async Task<string> GetFileUrlAsync(string filePath)
        {
            var storageObject = await _storageClient.GetObjectAsync(_bucketName,filePath);
            return storageObject.MediaLink;
        }

        public async Task<string> GeneratePublicUrl(string filePath)
        {
            var storageObject = _storageClient.GetObject(_bucketName, filePath);
            storageObject.Acl = storageObject.Acl ?? new List<Google.Apis.Storage.v1.Data.ObjectAccessControl>();
            storageObject.Acl.Add(new Google.Apis.Storage.v1.Data.ObjectAccessControl()
            {
                Entity = "allUsers",
                Role = "READER"
            });
            _storageClient.UpdateObject(storageObject);
            return $"https://storage.googleapis.com/{_bucketName}/{filePath}";
        }
    }
}
