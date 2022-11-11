using Google.Apis.Drive.v3;
using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.AnalyticsReporting.v4;


namespace GoogleDriveTest
{
  public class BuildAuth
  {
    public static DriveService AuthenticateServiceAccount()
    {
      string serviceAccountCredentialFilePath = "D:\\KeyDrive\\hidden-expanse-367916-18a203e82e24.json";
      string serviceAccountEmail = "alissonc@hidden-expanse-367916.iam.gserviceaccount.com";

      if (string.IsNullOrEmpty(serviceAccountCredentialFilePath))
        throw new Exception("Path to the service account credentials file is required.");
      if (!File.Exists(serviceAccountCredentialFilePath))
        throw new Exception("The service account credentials file does not exist at: " + serviceAccountCredentialFilePath);
      if (string.IsNullOrEmpty(serviceAccountEmail))
        throw new Exception("ServiceAccountEmail is required.");

      // These are the scopes of permissions you need. It is best to request only what you need and not all of them
      //string[] scopes = new string[] { AnalyticsReportingService.Scope.Analytics };             // View your Google Analytics data

      string[] scopes = new string[] { DriveService.Scope.Drive,
                                DriveService.ScopeConstants.Drive,};

      // For Json file

      GoogleCredential credential;
      using (var stream = new FileStream(serviceAccountCredentialFilePath, FileMode.Open, FileAccess.Read))
      {
        credential = GoogleCredential.FromStream(stream)
              .CreateScoped(scopes);
      }

      // Create the  Analytics service.
      return new DriveService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = "Drive Service account Authentication Sample",
      });
      
     
    }
  }
}
