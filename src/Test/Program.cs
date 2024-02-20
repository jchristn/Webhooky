namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using WatsonWebserver;
    using WatsonWebserver.Core;
    using Webhook;

    internal class Program
    {
        static string _Hostname = "localhost";
        static int _Port = 3000;
        static WebserverSettings _WebserverSettings = new WebserverSettings();
        static Webserver _Webserver = null;

        static WebhookSettings _Settings = new WebhookSettings();
        static WebhookManager _Webhook = null;

        static async Task Main(string[] args)
        {
            _WebserverSettings.Hostname = _Hostname;
            _WebserverSettings.Port = _Port;
            _Webserver = new Webserver(_WebserverSettings, DefaultRoute);
            // _Webserver.Start();

            Console.WriteLine("Webserver started on " + _WebserverSettings.Prefix);

            _Settings.ResponseRetentionMs = (1000 * 60 * 10); // 10 minutes
            _Settings.PollIntervalMs = 2500;
            _Webhook = new WebhookManager(_Settings);
            _Webhook.Logger = Console.WriteLine;
            _Webhook.OnWebhookEvent += WebhookEventHandler;

            WebhookTarget target = _Webhook.Targets.Add(new WebhookTarget
            {
                Url = "http://" + _Hostname + ":" + _Port + "/",
                ContentType = "text/plain",
                ExpectStatus = 200
            });

            Console.WriteLine("Using webhook target " + target.GUID);

            WebhookRule rule = _Webhook.Rules.Add(new WebhookRule
            {
                TargetGUID = target.GUID,
                Name = "My webhook rule",
                OperationType = "test",
                MaxAttempts = 3,
                RetryIntervalMs = 2500
            });

            Console.WriteLine("Using webhook rule " + rule.GUID);

            for (int i = 0; i < 1; i++)
            {
                string val = "Test " + i.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(val);

                List<WebhookEvent> events = await _Webhook.AddEvent("test", bytes);
            }

            await Task.Delay(10000);

            /*
            _Webserver.Stop();

            for (int i = 10; i < 20; i++)
            {
                string val = "Test " + i.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(val);

                Console.WriteLine("Adding event " + i);
                List<WebhookEvent> events = await _Webhook.AddEvent("test", bytes);
            }

            Task.Delay(10000).Wait();

            _Webserver.Start();
            */

            Console.WriteLine("");
            Console.WriteLine("Press ENTER to exit");
            Console.WriteLine("");
            Console.ReadLine();
        }

        private static void WebhookEventHandler(object sender, WebhookEventArgs e)
        {
            Console.WriteLine("*** Event " + e.Event.GUID + " status: " + e.Status.ToString());
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            Console.WriteLine(ctx.Request.Source.IpAddress + ":" + ctx.Request.Source.Port + " " + ctx.Request.Method.ToString() + " " + ctx.Request.Url.RawWithQuery + ": 200");
            await ctx.Response.Send(ctx.Request.DataAsBytes);
        }
    }
}
