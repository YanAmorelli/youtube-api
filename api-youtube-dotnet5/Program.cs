using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

/*
 * External dependencies, OAuth 2.0 support, and core client libraries are at:
 *   https://developers.google.com/api-client-library/dotnet/apis/
 * Also see the Samples.zip file for the Google.Apis.Samples.Helper classes at:
 *   https://github.com/youtube/api-samples/tree/master/dotnet
 */

using DotNetOpenAuth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Upload;

namespace api_youtube_dotnet5
{
    internal class UploadVideo
    {
        [STAThread]
        static void Main(string[] args)
        {
            {
                Console.WriteLine("YouTube Data API: Upload Video");
                Console.WriteLine("==============================");

                try
                {
                    new UploadVideo().Run().Wait();
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("C:/Users/3CON-RJ/Documents/client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = "Teste música Lofi";
            video.Snippet.Description = "Teste";
            video.Snippet.Tags = new string[] { "music", "lofi" };
            
            // TODO: Pesquisar categorias por id
            video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "private"; // or "private" or "public"
            var filePath = @"C:\Users\3CON-RJ\Downloads\Chão De Giz - Zé Ramalho [lofi remix].mp4"; // Replace with path to actual movie file.

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                await videosInsertRequest.UploadAsync();
            }
        }

        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
        }
    }    
}
