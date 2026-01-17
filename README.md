# Logging.ExceptionSender

Watches for unhandled exceptions, saves details to text files in specific folder, starts background task that sends exception info to you (MailGun and Telegram are supported, add your own if needed).

Written for console/worker projects that use [RecurrentTasks][2] and for ASP.NET projects (via `Logging.ExceptionSender.Web`).

[![NuGet](https://img.shields.io/nuget/v/Logging.ExceptionSender.svg)](https://www.nuget.org/packages/Logging.ExceptionSender/) 
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)
![MIT License](https://img.shields.io/github/license/justdmitry/RecurrentTasks)

## Main features

* Exception message and stacktrace are captured;
* Last _N_ log records captured (using [Logging.Memory][1] package);
* Captured data saved in `log` subdirectory for later processing;
* Background task (based on [RecurrentTasks][2]) checks new exception data;
* Every single exception - one message to you;
* Uses [**MailGun**][3] to send emails (free quota 10K emails/month) or **Telegram**
* Add your own provider by inheriting from `ExceptionSenderTask`;
* When new exception is caught - tries to send immediately;
* When message sucessfully sent - files are deleted from disk;
* Can send emails to multiple recipients (multiple `To`).

## Installation

Use NuGet package [Logging.ExceptionSender](https://www.nuget.org/packages/Logging.ExceptionSender) for console/worker projects, or [Logging.ExceptionSender.Web](https://www.nuget.org/packages/Logging.ExceptionSender.Web) for web projects.


## Usage

See **Sample** project.
 
### 1. Register at MailGun.com or create a Telegram bot

Put values into `appsettings` file:

```json
{
  "ExceptionSender": {
    // For Mailgun
    "MailgunDomain": "example.com",
    "MailgunApiKey": "key-*************",
    "From": "myapp@example.com",
    "To": [ "admin1@example.com", "admin2@example.com" ],

    // For Telegram
    "ChatId": "",
    "BotToken": ""
  }
}
```

### 2. Register sender service

```csharp
services.AddMailgunExceptionSender(Configuration.GetSection("ExceptionSender"));
// or
services.AddTelegramExceptionSender(Configuration.GetSection("ExceptionSender"));
```

### 3. Attach catch service

Append `.WithExceptionSender()` to task configuration:

```csharp
services.AddTask<SomeTask>(o => o.AutoStart(...).WithExceptionSender());
```

Append `.UseExceptionSender()` to aspnet request pipeline:

```csharp
app.UseExceptionSender();
```

### 4: Enable MemoryLogger with desired log level

```csharp
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
loggerFactory.AddMemory(LogLevel.Debug);
```

### Dependencies

* [Logging.Memory][1]
* [RecurrentTasks][2]
* `Microsoft.Extensions.Http` (for HttpClient)

[1]: https://github.com/iflight/Logging.Memory
[2]: https://github.com/justdmitry/RecurrentTasks
[3]: https://www.mailgun.com
