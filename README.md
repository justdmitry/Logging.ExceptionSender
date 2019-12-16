# Logging.ExceptionSender

`ExceptionSenderMiddleware` watches for unhandled exceptions. When found - details are saved to text files in specific folder.

`ExceptionSenderTask` monitors that folder and sends exception info to you by email (using [MailGun.com][3], but you can add your own)

Written for **ASP.NET Core** (ASP.NET 5, ASP.NET vNext) projects.

[![NuGet](https://img.shields.io/nuget/v/Logging.ExceptionSender.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Logging.ExceptionSender/)

## Main features

* Exception message and stacktrace are captured;
* Last _N_ log records captured ([Logging.Memory][1] is used);
* Captured data saved in `logs` subdirectory for later processing;
* Task (based on [RecurrentTasks][2]) is used for checking new exception data;
* Every single exception - one email to you;
* [MailGun][3] is used to send emails (free quota 10K emails/month), you can add new mail providers (inherit from `ExceptionSenderTask`);
* When new exception is caught - tries to send immediately;
* When message sucessfully sent - files are deleted from disk;
* Can send message to multiple recipients (multiple `To`)

## Installation

Use NuGet package [Logging.ExceptionSender](https://www.nuget.org/packages/Logging.ExceptionSender)

### Dependencies

* [Logging.Memory][1]
* [RecurrentTasks][2]


## Usage

### 1. Register at MailGun.com (if needed)

If you wish to use `MailGun` for sending mail, you need to create account (or use existing one, if any).

Register your site at [MailGun.com][3] and write down your domain name and api key:

!["sample](docs/mailgun.png)

### 2. Configure/initialize in `Startup.cs`

Sample (minimum) configuration in `config.json` (aka `appsettings.json`):

```json
{
    ...
    "ExceptionSender": {
        "MailgunDomain": "example.com",
        "MailgunApiKey": "key-*************",
        "From": "myapp@example.com",
        "To": ["admin1@example.com", "admin2@example.com"]
    }
    ...
}
```

In `ConfigureServices` method of your `Startup.cs`:

```csharp
services.AddMailgunExceptionSender(Configuration.GetSection("ExceptionSender"));
```

In `Configure` method of your `Statup.cs`:

```csharp
// Enable in-memory logging
loggerFactory.AddMemory();

// Activate ExceptionSender middleware to catch ASP exceptions
app.UseMiddleware<ExceptionSenderMiddleware>();

```


[1]: https://github.com/iflight/Logging.Memory
[2]: https://github.com/justdmitry/RecurrentTasks
[3]: https://www.mailgun.com
