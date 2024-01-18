# recover-large-files-icloud

This script sends a POST request to the iCloud Data Recovery ```recoverDeletedFiles``` endpoint and attempts to recover a large number of files. Apple allows only 250 files per request before their backend API times out and shits itself. When you are recovering an accidental Desktop folder deletion, 250 files won't cut it.

## Requirements to run
1. Login to https://icloud.com, click on Drive then "&#x2630;" icon and go to Data Recovery

2. Using Google Chrome Developer Tools -> Network tab, get the following from **any** ```POST``` request the browser is making to the iCloud backend endpoints:

| Variable | Description |
| --- | ----------|
| clientId | The app client Id. This is found in the query string. |
| dsid | Domain service Id. This is also found in the query string. | 
| cookie | Get any ```icloud.com``` cookie from any ```POST``` request and save it in ```cookie.txt```. The most important cookie is ```X-APPLE-WEBAUTH-TOKEN``` |
| ```files.json``` | You can get the content of this Json file by making a recovery attempt after you "Select all" files using the web UI. The POST request to the ```recoverDeletedFiles``` endpoint will contain in its raw body the files that need to be recovered. It is okay to recover files twice. |
| clientBuildNumber | Check this to make sure it did not change |
| clientMasteringNumber | Should be the same as ```clientBuildNumber``` |
| iCloudBackend | This is the host name of the POST requests you are seeing in the Developer Tools |
| maxConcurrent | 20 concurrent requests at a time seems to be a sane number. Change to your preference |

3. Keep session alive by keeping a browser window open on the Data Recovery page. Session cookie time out is approx. 15 minutes.
4. &#9785; &#xF8FF;.

## Config and Run
with the latest .NET SDK installed locally, run the command: ```dotnet run``` after filling out the details in ```Program.cs```. I used .NET 8.0 SDK so YMMV.