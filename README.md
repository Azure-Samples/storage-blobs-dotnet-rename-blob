---
services: blobs
platforms: c#
author: msonecode
---

# storage-blobs-dotnet-rename-blob
A code snippet to demonstrate how to rename windows azure blob files.
## Building this sample
To install Microsoft Azure Storage, run the following command in the Package Manager Console
PM> Install-Package WindowsAzure.Storage
Ensure your Visual Studio version is 2012 or above.
Before using the code snippet, you need to set your account name and key first:
```cs
StorageCredentials cred = new StorageCredentials("[Your storage account name]", "[Your storage account key]"); 
CloudBlobContainer container = new CloudBlobContainer(new Uri("http://[Your storage account name].blob.core.windows.net/[Your container name] /"), cred);
```
## Using the Code
```cs
/// <summary>  
/// 1. Copy the file and name it to a new name  
/// 2. Delete the old file  
/// </summary> 
StorageCredentials cred = new StorageCredentials("[Your?storage?account?name]", "[Your?storage?account?key]");  
CloudBlobContainer container = new CloudBlobContainer(new Uri("http://[Your?storage?account?name].blob.core.windows.net/[Your container name] /"), cred);  

string fileName = "OldFileName";  
string newFileName = "NewFileName";  
await container.CreateIfNotExistsAsync();  
CloudBlockBlob blobCopy = container.GetBlockBlobReference(newFileName);  
if (!await blobCopy.ExistsAsync())  
{  
    CloudBlockBlob blob = container.GetBlockBlobReference(fileName);  
 
    if (await blob.ExistsAsync())  
    {  
           await blobCopy.StartCopyAsync(blob);  
           await blob.DeleteIfExistsAsync();  
    } 
} 
```
## About the code
There’s no API that can rename the blob file on Azure.
This code snippet demonstrates how to rename a blob file in Microsoft Azure Blob Storage.
## More information
1 [https://code.msdn.microsoft.com/How-to-rename-a-blob-file-58aae8c9](https://code.msdn.microsoft.com/How-to-rename-a-blob-file-58aae8c9)
