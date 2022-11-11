using System;
using Google.Apis.Drive.v3;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Download;
using System.Linq;

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
        var parentFolderId = "1Qsgg_CnXttRAGN1VpNwLqVITmHcINqBT";
        Console.WriteLine($"Nome da pasta do Cliente: {clienteNome}\n");

        static string GetClienteFolderId(DriveService service, string clienteNome, string parentFolderId)
        {
          var folder = service.Files.List();
          folder.Q = $"parents in '{parentFolderId}' and name = '{clienteNome}'";
          var response = folder.Execute();

          return response.Files[0].Id;
        }
        
        static List<Arquivo> GetFilesIdDrive(DriveService service, string clienteNome, string parentFolderId)
        {
          var arquivos = new List<Arquivo>();

          //Encontrar a pasta com o nome do cliente
          var folder = service.Files.List();
          folder.Q = $"parents in '{parentFolderId}' and name = '{clienteNome}'";
          var response = folder.Execute();


          //Encontrar os arquivos "pdf" na pasta do cliente
          var files = service.Files.List();
          files.Q = $"parents in '{response.Files[0].Id}' and mimeType = 'application/pdf'";
          var responseFiles = files.Execute();


          foreach(var file in responseFiles.Files)
          {                     
            arquivos.Add(new Arquivo { Nome = file.Name, Id = file.Id});
          }
            
          return arquivos;
        }

        static void UploadFile(DriveService service, string clienteNome, string parentFolderId, string arquivoNome)
        {
          // Upload file DDD.pdf on drive.
          FilesResource.CreateMediaUpload request;

          //Procura os arquivos, podendo diferenciar o mimeType para text, image....
          //URL da pasta == id
          //request1.Q = "parents in '1Qsgg_CnXttRAGN1VpNwLqVITmHcINqBT' and mimeType = 'image/png'";


          // Create a new file on drive.
          var fileMetadata = new Google.Apis.Drive.v3.Data.File()
          {
            Name = arquivoNome.Split('\\').Last(),
            Parents = new List<string>()
            {
              //Deve apenas existir 1 pasta com o nome de cada cliente, caso ao contrário isso aqui vai estourar um erro.
              GetClienteFolderId(service, clienteNome, parentFolderId)
            }
          };

          using (var stream = new FileStream(arquivoNome, FileMode.Open))
          {
            // Create a new file, with metadata and stream.
            request = service.Files.Create(
                fileMetadata, stream, "application/pdf");

            //Fields que vão retornar no response Body
            request.Fields = "id, name, parents";
            request.Upload();
          }

          var file = request.ResponseBody;

     
          Console.WriteLine($"Arquivo Criado: \nID do arquivo: {file.Id} \nNome do arquivo:  { file.Name } \n");         
        }
    
        static void DownLoadFile(DriveService service, string fileId, string arquivoNome)
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

          FileStream file1 = new FileStream($"D:\\DocumentosPdf\\{arquivoNome}.pdf", FileMode.Create, FileAccess.Write);
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

        static string[] GetAllFilesFromDirectory()
        {
          string[] fileArray = Directory.GetFiles("D:\\DocumentosPdf", "*.pdf");

          return fileArray;
        }


        switch (args[0])
        {
          case "upload":
            foreach (var arquivoNome in GetAllFilesFromDirectory()) 
              UploadFile(service, clienteNome, parentFolderId, arquivoNome);
            break;
          case "download":
            foreach (var arquivo in GetFilesIdDrive(service, clienteNome, parentFolderId)) 
              DownLoadFile(service, arquivo.Id, arquivo.Nome);
            break;
          case "delete":
            foreach (var arquivo in GetFilesIdDrive(service, clienteNome, parentFolderId))
            {
              DeleteFile(service, arquivo.Id);
              Console.WriteLine($"Arquivo Deletado: \nID do arquivo: {arquivo.Id} \nNome do arquivo:  { arquivo.Nome } \n");
            };
            break;
          default:
            Console.Write("Comando Invalido", args[0]);
            break;
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
