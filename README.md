![alt tag](https://github.com/jchristn/Webhooky/raw/main/Assets/icon.ico)

# Webhooky

## Easily add webhooks to your app

[![NuGet Version](https://img.shields.io/nuget/v/webhooky.svg?style=flat)](https://www.nuget.org/packages/webhooky/) [![NuGet](https://img.shields.io/nuget/dt/webhooky.svg)](https://www.nuget.org/packages/webhooky)    

Webhooky helps you add webhooks to your app, with simple classes and methods for managing rules, targets, events, retries, and status. 
 
## New in v1.0.x

- Initial release

## Help or Feedback

Need help or have feedback?  Please file an issue here!

## Special Thanks

Thanks to community members that have helped improve this library!

## Simple Example

Start a node.
```csharp
using Webhook;

// Instantiate
Webhook webhook = new Webhook();

// Define your target
WebhookTarget target = webhook.Targets.Add(new WebhookTarget
{
  Url = "http://localhost:8000/mywebhook/",
  ContentType = "text/plain",
  ExpectStatus = 200
});

// Define a rule
WebhookRule rule = webhook.Rules.Add(new WebhookRule
{
  TargetGUID = target.GUID,
  Name = "My webhook rule",
  OperationType = "test",
  MaxAttempts = 3,
  RetryIntervalMs = 10000
});

// Fire an event!
webhook.AddEvent("test", "Here is my webhook data!");

// See events
List<WebhookEvent> events;

events = webhook.Events.GetSucceeded();  // successful events
events = webhook.Events.GetFailed();     // failed events
events = webhook.Events.GetPending();    // pending events
```

## Version History

Please refer to CHANGELOG.md.
