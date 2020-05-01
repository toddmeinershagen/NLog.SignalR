@ECHO OFF

REM Requests the API Key
REM ====================
SET /p NuGetApiKey= Please enter the project's NuGet API Key: 
nuget.exe setApiKey %NuGetApiKey%

REM Push to Nuget 
REM =============
cd src\NLog.SignalR\bin\Release
dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json
cd ..\..\..