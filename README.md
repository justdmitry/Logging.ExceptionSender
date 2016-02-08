# Logging.ExceptionSender

`ExceptionSenderMiddleware` watches for unhandled exceptions. When found - details are saved to text files in specific folder.

`ExceptionSenderTask` monitors that folder and sends exception details to you by mail (using [MailGun.com][3])

Written for **ASP.NET vNext** (ASP.NET 5, ASP.NET Core 1).

## Main features

* Exception message and stacktrace are saved;
* Last N log records saved ([Logging.Memory][1] is used);
* Files are saved in `logs` subdirectory for later processing;
* Task (based on [RecurrentTasks][2]) is used for checking new exception data;
* Every single exception - one email to you;
* [MailGun][3] is used to send emails (free quota 10K emails/month);
* When new exception is caught - tries to send immediately
* When message sucessfully sent - files are deleted from disk

## Usage

*In progress*

## Installation

Use NuGet package [Logger.ExceptionSender
](https://www.nuget.org/packages/Logger.ExceptionSender
/)

### Dependencies

* Microsoft.AspNet.Http.Abstractions
* Microsoft.Extensions.OptionsModel
* Microsoft.Extensions.PlatformAbstractions
* [Logging.Memory][1]
* [RecurrentTasks][2]
* [RestSharp](http://restsharp.org/)


## Version history

* 1.0.0 (Feb 8, 2016)
  * Initial release

[1]: https://github.com/iflight/Logging
[2]: https://github.com/justdmitry/RecurrentTasks
[3]: https://www.mailgun.com