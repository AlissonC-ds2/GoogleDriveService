using System;
using Google.Apis.Drive.v3;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Download;

namespace GoogleDriveTest
{
  public class Program
  {
    static void Main(string[] args)
    {
      try
      {
        var clienteNome = "Tenant1";
        var service = BuildAuth.AuthenticateServiceAccount();

        static List<string> GetFilesId(DriveService service, string clienteNome)
        {
          List<string> ids = new List<string>();

          //Encontrar a pasta com o nome do cliente
          var folder = service.Files.List();
          folder.Q = $"parents in '1Qsgg_CnXttRAGN1VpNwLqVITmHcINqBT' and name = '{clienteNome}'";
          var response = folder.Execute();


          //Encontrar os arquivos "pdf" na pasta do cliente
          var files = service.Files.List();
          files.Q = $"parents in '{response.Files[0].Id}' and mimeType = 'application/pdf'";
          var responseFiles = files.Execute();


          foreach(var file in responseFiles.Files) ids.Add(file.Id);

          return ids;
        }

        foreach(var id in GetFilesId(service, clienteNome))
        {
          UploadFile(service, id);
          DownLoadFile(service, id);
        }

        static void UploadFile(DriveService service, string fileId)
        {
          // Upload file photo.jpg on drive.
          FilesResource.CreateMediaUpload request;

          //Procura os arquivos, podendo diferenciar o mimeType para text, image....
          //URL da pasta == id
          //request1.Q = "parents in '1Qsgg_CnXttRAGN1VpNwLqVITmHcINqBT' and mimeType = 'image/png'";


          // Create a new file on drive.
          var fileMetadata = new Google.Apis.Drive.v3.Data.File()
          {
            Name = "DDD.pdf",
            Parents = new List<string>()
            {
              //Deve apenas existir 1 pasta com o nome de cada tenant, caso ao contrário isso aqui vai estourar um erro.
              fileId
            }
          };

          using (var stream = new FileStream("D:\\Imagem\\DDD.pdf", FileMode.Open))
          {
            // Create a new file, with metadata and stream.
            request = service.Files.Create(
                fileMetadata, stream, "application/pdf");

            //Fields que vão retornar no response Body
            request.Fields = "id, name, parents";
            request.Upload();
          }

          var file = request.ResponseBody;


          // Prints the uploaded file id.
          Console.WriteLine("ID do arquivo: " + file.Id + "\nNome do arquivo: " + file.Name);
          //Console.WriteLine("Nome da pasta do Cliente: " + clienteNome + "\nID da pasta do cliente: " + file.Parents[0]);
        }
    
        static void DownLoadFile(DriveService service, string fileId)
        {
          var request = service.Files.Get(fileId);
          var stream = new MemoryStream();

          // Add a handler which will be notified on progress changes.
          // It will notify on each chunk download and when the
          // download is completed or failed.
          request.MediaDownloader.ProgressChanged += progress => {
            switch (progress.Status)
            {
              case DownloadStatus.Downloading:
                {
                  Console.WriteLine(progress.BytesDownloaded);
                  break;
                }
              case DownloadStatus.Completed:
                {
                  Console.WriteLine("Download complete.");
                  break;
                }
              case DownloadStatus.Failed:
                {
                  Console.WriteLine("Download failed.");
                  break;
                }
            }
          };
          request.Download(stream);

          FileStream file1 = new FileStream($"D:\\Imagem\\{fileId}.pdf", FileMode.Create, FileAccess.Write);
          stream.WriteTo(file1);
        }

        static void DeleteFile(DriveService service, string fileId)
        {
          try
          {
            service.Files.Delete(fileId).Execute();
          }
          catch (Exception e)
          {
            Console.WriteLine("An error occurred: " + e.Message);
          }
        }

      }
      catch (Exception e)
      {
        // TODO(developer) - handle error appropriately
        if (e is AggregateException)
        {
          Console.WriteLine("Credential Not found");
        }
        else if (e is FileNotFoundException)
        {
          Console.WriteLine("File not found");
        }
        else
        {
          throw;
        }
      }
    }


  }
}
