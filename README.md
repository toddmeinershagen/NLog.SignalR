NLog.SignalR
============

NLog.SignalR is a [SignalR](https://github.com/SignalR/SignalR) target for [NLog](https://github.com/NLog/NLog), allowing you to send log messages straight to a SignalR hub in real-time.

[![NLog.SignalR](https://badge.fury.io/nu/NLog.SignalR.svg)](https://badge.fury.io/nu/NLog.SignalR)
[![AppVeyor](https://img.shields.io/appveyor/ci/toddmeinershagen/nlog-signalr/master.svg)](https://ci.appveyor.com/project/toddmeinershagen/nlog-signalr/branch/master)

## Getting started

See the included Sample at /src/NLog.Signalr.Sample.Web and /src/NLog.SignalR.Sample.Command for an example of running two clients (web and console) at the same time and having log messages appear on the web log page from both sources.

To add to your own projects do the following.

#### Add NLog.SignalR.dll to your project(s) via [NuGet](http://www.nuget.org/packages/NLog.SignalR/)

  > install-package NLog.SignalR

#### Configure NLog

Add the assembly and new target to NLog.config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <extensions>
    <add assembly="NLog.SignalR" />
  </extensions>

  <targets async="true">
    <target xsi:type="SignalR"
            name="signalr"
            uri="http://localhost:1860"
            hubName="LoggingHub"
            methodName="Log"
            layout="${message}"
            />
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="signalr" />
  </rules>
</nlog>
```

Note:  the only required property for the target is the uri.  The target will default hubName, methodName, and layout properties.

#### Set up Hub in server project to receive log events

There are two methods you can use for setting up the Hub in your web project.  One is to set up a strongly-typed hub, using the interface provided by the NLog.SignalR component.

```c#
public class LoggingHub : Hub<ILoggingHub>
{
    public void Log(LogEvent logEvent)
    {
          Clients.Others.Log(logEvent);
    }
}
```

The other way is to simply set up a Hub in your web project using dynamic types.

```c#
public class LoggingHub : Hub
{
    public void Log(dynamic logEvent)
    {
          Clients.Others.Log(logEvent);
    }
}
```

#### Set up client-side JavaScript to listen for log events

```html
<script>
  
$(function() {
        
 var nlog = $.connection.loggingHub;
 nlog.client.log = function(logEvent) {
   //Put code here to handle the logEvent that is sent.
 };

 $.connection.hub.start().done(function() {
   //Put code here that you want to execute after connecting to the Hub.
 });
})    

<script>
```

## Feedback

Feel free to tweet [@tmeinershagen](http://twitter.com/tmeinershagen) for questions or comments on the code.  You can also submit a GitHub issue [here](https://github.com/toddmeinershagen/NLog.SignalR/issues).

## License

https://github.com/toddmeinershagen/NLog.SignalR/blob/master/LICENSE
