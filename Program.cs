using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BlobV12Net6
{
    class Program
    {
        //- Create a unique name for the container
        const string containerName = "quickstartblobs" + "-001";
        //- Create a local file in the ./data/ directory for uploading and downloading
        const string localPath = "./data";
        //- Set file path in blob container (eg. equivalent to a matter id)
        const string filePath = "02105-0002" + "/";

        static async Task Main(string[] args)
        {
            // Build a config object, using env vars and JSON providers.
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.dev.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            var connectionString = config.GetValue<string>("AZURE_STORAGE_CONNECTION");
            // - variables
            BlobContainerClient containerClient = null;
            bool isContainerCreated = false;            
            //- Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);  

            // - Show menu
            ShowMenu();
            isContainerCreated = isExistsContainer(blobServiceClient);
            Console.WriteLine("Is container created = " + isContainerCreated);
            if (isContainerCreated)
            {
                containerClient = GetBlobContainer(blobServiceClient);
            }
            // - Process keys
            while (Console.ReadKey(true).Key != ConsoleKey.X)
            {
                ConsoleKeyInfo currentKey = Console.ReadKey(true);
                string blobName = "";
                string newContainerName = "";
                string sourceContainerName;
                string destContainerName;
                string folderName;

                switch (currentKey.Key)
                {
                    case ConsoleKey.A:
                        if (!isContainerCreated)
                        {
                            containerClient = await CreateBlobContainer(blobServiceClient);
                            isContainerCreated = isExistsContainer(blobServiceClient);
                            Console.WriteLine($"Creating blob container.. | container created = {isContainerCreated}");
                        }
                        else 
                        {
                            Console.WriteLine($"Blob container already exists! | container created = {isContainerCreated}");
                        }
                        break;

                    case ConsoleKey.B:
                        await UploadBlob(containerClient);
                        break;

                    case ConsoleKey.C:
                        await ListBlobs(containerClient);
                        break;

                    case ConsoleKey.D:
                        Console.WriteLine("Enter blob name:");
                        blobName = Console.ReadLine();
                        await DeleteBlob(containerClient, blobName);
                        break;

                    case ConsoleKey.E:
                        Console.WriteLine("Enter blob name:");
                        blobName = Console.ReadLine();
                        await DownloadBlob(containerClient, blobName);
                        break;                    

                    case ConsoleKey.G:
                        if (isContainerCreated)
                        {
                            GetBlobContainer(blobServiceClient);
                        }
                        else
                        {
                            Console.WriteLine("You must create a container first!");                            
                        }
                        break;

                    case ConsoleKey.H:
                        GetServiceSasUriForContainer(containerClient);
                        break;

                    case ConsoleKey.I:
                        Console.WriteLine("Enter blob name:");
                        blobName = Console.ReadLine();
                        GetServiceSasUriForBlob(containerClient, blobName);
                        break;

                    case ConsoleKey.J:
                        Console.WriteLine("Enter container name:");
                        newContainerName = Console.ReadLine();
                        CreateNewContainer(blobServiceClient, newContainerName);
                        break;

                    case ConsoleKey.K:
                        await UploadBlobNoFolder(containerClient);
                        break;

                    case ConsoleKey.M:
                        Console.WriteLine("Enter source container name:");
                        sourceContainerName = Console.ReadLine();
                        Console.WriteLine("Enter destination container name:");
                        destContainerName = Console.ReadLine();
                        await MoveBlobs(sourceContainerName, destContainerName, blobServiceClient);
                        break;

                    case ConsoleKey.N:
                        Console.WriteLine("Enter source container name:");
                        sourceContainerName = Console.ReadLine();
                        Console.WriteLine("Enter destination container name:");
                        destContainerName = Console.ReadLine();
                        Console.WriteLine("Enter folder name:");
                        folderName = Console.ReadLine();
                        await MoveBlobsFolder(sourceContainerName, destContainerName, folderName, blobServiceClient);
                        break;

                    case ConsoleKey.Y:
                        // Write the values to the console.
                        Console.WriteLine($"connection = {connectionString}");
                        break;
                        
                    case ConsoleKey.Z:
                        ShowMenu();
                        break;

                    default:
                        break;
                }
            }                        
        }

        private static void ShowMenu()
        {
            Console.WriteLine("MENU - Press key of selected action");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("A > Create test blob container");
            Console.WriteLine("B > Upload blob to container");
            Console.WriteLine("C > List blobs");
            Console.WriteLine("D > Delete blob");
            Console.WriteLine("E > Download blob");            
            Console.WriteLine("F > Delete container");
            Console.WriteLine("G > Get container - " + containerName);
            Console.WriteLine("H > SAS Uri container");
            Console.WriteLine("I > SAS Uri blob");
            Console.WriteLine("J > Create new blob container");
            Console.WriteLine("K > Upload blob with no folder to container");
            Console.WriteLine("M > Move all blobs to a different container");
            Console.WriteLine("N > Move blobs on folder to a different container");
            Console.WriteLine("Y > Display connection");
            Console.WriteLine("Z > Show menu");
            Console.WriteLine("X > Exit");
        }

        private static bool isExistsContainer(BlobServiceClient blobServiceClient)
        {
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            if (container.Exists())
            {
                return true;
            }
            return false;
        }

        private static async Task<BlobContainerClient> CreateBlobContainer(BlobServiceClient blobServiceClient)
        {
            // - Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

            return containerClient;
        }

        private static async Task UploadBlob(BlobContainerClient containerClient)
        {
            string fileName = "Quickstart" + Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);
            //- Write text to the file
            await File.WriteAllTextAsync(localFilePath, "Hello, Azure Blob!");
            //- Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(filePath + fileName);
            //- Display
            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);
            //- Upload data from the local file
            await blobClient.UploadAsync(localFilePath, overwrite: true);
        }

        private static async Task ListBlobs(BlobContainerClient containerClient)
        {
            //- List blobs in a container
            Console.WriteLine("Listing blobs ...");
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name + " | contains 02105- " + blobItem.Name.Contains("02105-"));
            }
        }

        private static BlobContainerClient GetBlobContainer(BlobServiceClient blobServiceClient)
        {
            //- Get existing blob container
            var blobContainer = blobServiceClient.GetBlobContainerClient(containerName);

            return blobContainer;
        }

        private static async Task DownloadBlob(BlobContainerClient containerClient, string blobName)
        {
            //- Download a blob to a local file
            //- Append the string "DOWNLOADED" before the .txt extension so you can compare files in the data dir
            string localFilePath = Path.Combine(localPath, blobName);
            string downloadFilePath = localFilePath.Replace(".txt", "-DOWNLOADED.txt");
            //- Display
            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);
            //- Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            //- Download the blob's contents and save it to a file            
            await blobClient.DownloadToAsync(downloadFilePath);
            //- Display uri
            Console.WriteLine(blobClient.Uri);
        }

        private static async Task DeleteBlob(BlobContainerClient containerClient, string blobName)
        {
            //- Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            //- Display uri
            Console.WriteLine("\nDeleting blob \n\t{0}\n", blobClient.Uri);
            //- Delete blob in azure immediately, using DeleteAsync will soft delete until garbage collected
            await blobClient.DeleteIfExistsAsync();
            //- Delete local file (original and downloaded)
            string localFilePath = Path.Combine(localPath, blobName);
            string originalFilePath = localFilePath.Replace(filePath, "");
            string downloadFilePath = localFilePath.Replace(".txt", "-DOWNLOADED.txt");
            File.Delete(originalFilePath);
            File.Delete(downloadFilePath);
        }

        private static void GetServiceSasUriForContainer(
            BlobContainerClient containerClient,
            string storedPolicyName = null
        )
        {
            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (containerClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for twenty minutes.
                BlobSasBuilder sas = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c",
                };
                if (storedPolicyName == null)
                {
                    sas.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(2);
                    sas.SetPermissions(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List);
                    // sas.SetPermissions(BlobAccountSasPermissions.List);
                }
                else
                {
                    sas.Identifier = storedPolicyName;
                }
                Uri sasUri = containerClient.GenerateSasUri(sas);
                Console.WriteLine("SAS URI for container is: {0}", sasUri);
                //- accessing the container thru browser is not supported, but listing all blobs is possible
                Console.WriteLine("SAS URI for container with list: {0}", sasUri + "&restype=container&comp=list");
            }
            else
            {
                Console.WriteLine(@"BlobContainerClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
            }
        }

        private static void GetServiceSasUriForBlob(
            BlobContainerClient containerClient,
            string blobName,
            string storedPolicyName = null
        )
        {
            //- Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);            
            // Check whether this BlobClient object has been authorized with Shared Key.
            if(blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for twenty minutes.
                BlobSasBuilder sas = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b",
                };
                if (storedPolicyName == null)
                {
                    sas.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(20);
                    sas.SetPermissions(
                        BlobSasPermissions.Read |
                        BlobSasPermissions.Write
                    );
                }
                else
                {
                    sas.Identifier = storedPolicyName;
                }
                Uri sasUri = blobClient.GenerateSasUri(sas);
                Console.WriteLine("SAS URI for blob is: {0}", sasUri);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
            }
        }
    
        private static async Task<BlobContainerClient> CreateNewContainer(BlobServiceClient blobServiceClient, string newContainerName)
        {
            // - Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(newContainerName);

            return containerClient;
        }

        private static async Task<string> MoveBlobs(
            string sourceContainer,
            string destContainer,
            BlobServiceClient blobServiceClient)
        {
            // - get containers (source and destination), this sample is for same storage
            BlobContainerClient sourceBlobContainer = blobServiceClient.GetBlobContainerClient(sourceContainer);
            BlobContainerClient destBlobContainer = blobServiceClient.GetBlobContainerClient(destContainer);
            destBlobContainer.CreateIfNotExists();
            // - get all blobs from source
            IEnumerable<BlobClient> sourceBlobRefs = await FindMatchingBlobsAsync(sourceBlobContainer);
            // - Move matching blobs to the destination container
            await MoveMatchingBlobsAsync(sourceBlobRefs, sourceBlobContainer, destBlobContainer);

            return "Blobs moved successfully.";
        }

        private static async Task<IEnumerable<BlobClient>> FindMatchingBlobsAsync(BlobContainerClient blobContainer)
        {
            List<BlobClient> blobList = new List<BlobClient>();

            // Iterate through the blobs in the source container
            List<BlobItem> segment = await blobContainer.GetBlobsAsync(prefix: "").ToListAsync();
            foreach (BlobItem blobItem in segment)
            {
                BlobClient blob = blobContainer.GetBlobClient(blobItem.Name);

                // Check the source file's metadata
                Response<BlobProperties> propertiesResponse = await blob.GetPropertiesAsync();
                BlobProperties properties = propertiesResponse.Value;
                
                // Check the last modified date and time
                // Add the blob to the list if has been modified since the specified date and time
                // if (DateTimeOffset.Compare(properties.LastModified.ToUniversalTime(), transferBlobsModifiedSince.ToUniversalTime()) > 0)
                // {
                //     blobList.Add(blob);
                // }
                blobList.Add(blob);
            }

            // Return the list of blobs to be transferred
            return blobList;
        }

        private static async Task MoveMatchingBlobsAsync(IEnumerable<BlobClient> sourceBlobRefs, BlobContainerClient sourceContainer, BlobContainerClient destContainer)
        {
            foreach (BlobClient sourceBlobRef in sourceBlobRefs)
            {
                // Copy the source blob
                BlobClient sourceBlob = sourceContainer.GetBlobClient(sourceBlobRef.Name);

                // Check the source file's metadata
                Response<BlobProperties> propertiesResponse = await sourceBlob.GetPropertiesAsync();
                BlobProperties properties = propertiesResponse.Value;

                BlobClient destBlob = destContainer.GetBlobClient(sourceBlobRef.Name);
                CopyFromUriOperation ops = await destBlob.StartCopyFromUriAsync(GetSharedAccessUri(sourceBlobRef.Name, sourceContainer));

                // Display the status of the blob as it is copied
                while(ops.HasCompleted == false)
                {
                    long copied = await ops.WaitForCompletionAsync();

                    Console.WriteLine($"Blob: {destBlob.Name}, Copied: {copied} of {properties.ContentLength}");
                    await Task.Delay(500);
                }

                Console.WriteLine($"Blob: {destBlob.Name} Complete");

                // Remove the source blob
                bool blobExisted = await sourceBlobRef.DeleteIfExistsAsync();
            }
        }

        private static Uri GetSharedAccessUri(string blobName, BlobContainerClient container)
        {
            DateTimeOffset expiredOn = DateTimeOffset.UtcNow.AddMinutes(10);

            BlobClient blob = container.GetBlobClient(blobName);
            Uri sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, expiredOn);

            return sasUri;
        }

        private static async Task UploadBlobNoFolder(BlobContainerClient containerClient)
        {
            string fileName = "Quickstart" + Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);
            //- Write text to the file
            await File.WriteAllTextAsync(localFilePath, "Hello, Azure Blob!");
            //- Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            //- Display
            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);
            //- Upload data from the local file
            await blobClient.UploadAsync(localFilePath, overwrite: true);
        }

        private static async Task<string> MoveBlobsFolder(
            string sourceContainer,
            string destContainer,
            string folder,
            BlobServiceClient blobServiceClient)
        {
            // - get containers (source and destination), this sample is for same storage
            BlobContainerClient sourceBlobContainer = blobServiceClient.GetBlobContainerClient(sourceContainer);
            BlobContainerClient destBlobContainer = blobServiceClient.GetBlobContainerClient(destContainer);
            destBlobContainer.CreateIfNotExists();
            // - get all blobs from source
            IEnumerable<BlobClient> sourceBlobRefs = await FindBlobsInFolderAsync(sourceBlobContainer, folder);
            // - Move matching blobs to the destination container
            await MoveMatchingBlobsAsync(sourceBlobRefs, sourceBlobContainer, destBlobContainer);

            return "Blobs in folder moved successfully.";
        }

        private static async Task<IEnumerable<BlobClient>> FindBlobsInFolderAsync(
            BlobContainerClient blobContainer,
            string folder)
        {
            List<BlobClient> blobList = new List<BlobClient>();

            // Iterate through the blobs in the source container
            List<BlobItem> segment = await blobContainer.GetBlobsAsync(prefix: "").ToListAsync();
            foreach (BlobItem blobItem in segment)
            {
                BlobClient blob = blobContainer.GetBlobClient(blobItem.Name);

                // Check the source file's metadata
                Response<BlobProperties> propertiesResponse = await blob.GetPropertiesAsync();
                BlobProperties properties = propertiesResponse.Value;
                
                // Check the last modified date and time
                // Add the blob to the list if has been modified since the specified date and time
                // if (DateTimeOffset.Compare(properties.LastModified.ToUniversalTime(), transferBlobsModifiedSince.ToUniversalTime()) > 0)
                // {
                //     blobList.Add(blob);
                // }
                if(blobItem.Name.Contains(folder))
                {
                    blobList.Add(blob);
                }
            }

            // Return the list of blobs to be transferred
            return blobList;
        }


    }
}