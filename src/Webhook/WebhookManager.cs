namespace Webhook
{
    using RestWrapper;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Watson.ORM;
    using Watson.ORM.Core;
    using Webhook.Managers;
    using Webhook.Services;

    /// <summary>
    /// Webhook manager.
    /// </summary>
    public class WebhookManager : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Method to invoke to send log messages.
        /// </summary>
        public Action<string> Logger { get; set; } = null;

        /// <summary>
        /// Rules.
        /// </summary>
        public RuleManager Rules
        {
            get
            {
                return _Rules;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Rules));
                _Rules = value;
            }
        }

        /// <summary>
        /// Targets.
        /// </summary>
        public TargetManager Targets
        {
            get
            {
                return _Targets;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Targets));
                _Targets = value;
            }
        }

        /// <summary>
        /// Events.
        /// </summary>
        public EventManager Events
        {
            get
            {
                return _Events;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Events));
                _Events = value;
            }
        }

        /// <summary>
        /// Request storage repository.
        /// </summary>
        public StorageService Requests
        {
            get
            {
                return _Requests;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Requests));
                _Requests = value;
            }
        }

        /// <summary>
        /// Responses storage repository.
        /// </summary>
        public StorageService Responses
        {
            get
            {
                return _Responses;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Responses));
                _Responses = value;
            }
        }

        /// <summary>
        /// Event handler for webhook events.
        /// </summary>
        public EventHandler<WebhookEventArgs> OnWebhookEvent;

        #endregion

        #region Private-Members

        private string _Header = "[WebhookManager] ";
        private WebhookSettings _Settings = null;
        private WatsonORM _ORM = null;

        private RuleManager _Rules = null;
        private TargetManager _Targets = null;
        private EventManager _Events = null;

        private StorageService _Requests = null;
        private StorageService _Responses = null;

        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private Task _ProcessingTask = null;
        private Task _CleanupTask = null;

        private List<string> _InProgress = new List<string>();
        private readonly object _InProgressLock = new object();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="settings">Settings.</param>
        public WebhookManager(WebhookSettings settings = null)
        {
            if (settings == null) settings = new WebhookSettings();

            _Settings = settings;

            _ORM = new WatsonORM(_Settings.Database);

            _ORM.InitializeDatabase();

            _ORM.InitializeTables(new List<Type>
            {
                typeof(WebhookEvent),
                typeof(WebhookRule),
                typeof(WebhookTarget)
            });

            _Rules = new RuleManager(_Settings, _ORM);
            _Targets = new TargetManager(_Settings, _ORM);
            _Events = new EventManager(_Settings, _ORM);

            _Requests = new StorageService(_Settings.RequestsDirectory);
            _Responses = new StorageService(_Settings.ResponsesDirectory);

            _ProcessingTask = Task.Run(() => ProcessingTask(), _TokenSource.Token);
            _CleanupTask = Task.Run(() => CleanupTask(), _TokenSource.Token);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _Header = null;
            _Settings = null;
            _ORM = null;
            _Rules = null;
            _Targets = null;
            _Events = null;

            _TokenSource.Cancel();
        }

        /// <summary>
        /// Add an event.
        /// </summary>
        /// <param name="operationType">Operation type.</param>
        /// <param name="data">Data.</param>
        /// <param name="targetGuid">Target GUID.</param>
        /// <returns>List of WebhookEvent.</returns>
        public async Task<List<WebhookEvent>> AddEvent(string operationType, string data, string targetGuid = null)
        {
            if (String.IsNullOrEmpty(data)) data = "";
            return await AddEvent(operationType, Encoding.UTF8.GetBytes(data), targetGuid);
        }

        /// <summary>
        /// Add an event.
        /// </summary>
        /// <param name="operationType">Operation type.</param>
        /// <param name="data">Data.</param>
        /// <param name="targetGuid">Target GUID.</param>
        /// <returns>List of WebhookEvent.</returns>
        public async Task<List<WebhookEvent>> AddEvent(string operationType, byte[] data, string targetGuid = null)
        {
            if (String.IsNullOrEmpty(operationType)) throw new ArgumentNullException(nameof(operationType));
            if (data == null) data = Array.Empty<byte>();

            List<WebhookEvent> added = new List<WebhookEvent>();

            List<WebhookRule> matching = _Rules.FindMatching(operationType, targetGuid);
            if (matching == null || matching.Count < 1)
            {
                Log("no matching rules found for operation " + operationType);
                return null;
            }

            foreach (WebhookRule rule in matching)
            {
                WebhookTarget target = _Targets.Read(rule.TargetGUID);
                if (target == null)
                {
                    Log("unable to find target GUID " + rule.TargetGUID);
                    continue;
                }

                DateTime dt = DateTime.UtcNow;

                WebhookEvent ev = new WebhookEvent
                {
                    TargetGUID = rule.TargetGUID,
                    RuleGUID = rule.GUID,
                    OperationType = operationType,
                    Url = target.Url,
                    Uri = target.Uri,
                    ContentType = target.ContentType,
                    ContentLength = data.Length,
                    Attempt = 0,
                    MaxAttempts = rule.MaxAttempts,
                    TimeoutMs = rule.TimeoutMs,
                    HttpStatus = 0,
                    AddedUtc = dt,
                    NextAttemptUtc = dt
                };

                ev = _ORM.Insert<WebhookEvent>(ev);
                added.Add(ev);

                await _Requests.Write(ev.GUID, data);

                Log("added webhook event GUID " + ev.GUID + " for operation " + ev.OperationType + " target " + ev.TargetGUID);

                OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.Created });
            }

            return added;
        }

        /// <summary>
        /// Retrieve request body for an event.
        /// </summary>
        /// <param name="eventGuid">GUID.</param>
        /// <returns>Byte array.</returns>
        public async Task<byte[]> GetRequestBody(string eventGuid)
        {
            if (String.IsNullOrEmpty(eventGuid)) throw new ArgumentNullException(nameof(eventGuid));

            return await _Requests.Read(eventGuid).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve response body for an event.
        /// </summary>
        /// <param name="eventGuid">GUID.</param>
        /// <returns>Byte array.</returns>
        public async Task<byte[]> GetResponseBody(string eventGuid)
        {
            if (String.IsNullOrEmpty(eventGuid)) throw new ArgumentNullException(nameof(eventGuid));

            return await _Responses.Read(eventGuid).ConfigureAwait(false);
        }

        #endregion

        #region Private-Methods

        private void Log(string msg)
        {
            if (String.IsNullOrEmpty(msg)) return;
            Logger?.Invoke(_Header + msg);
        }

        private async Task ProcessingTask()
        {
            bool firstRun = true;

            Log("starting processing task");

            while (!_TokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (!firstRun) await Task.Delay(_Settings.PollIntervalMs);
                    else firstRun = false;

                    List<WebhookEvent> pending = _Events.GetPending(null, null, null, _Settings.BatchSize);
                    if (pending == null || pending.Count < 1)
                    {
                        Log("no pending tasks to process");
                        continue;
                    }
                     
                    foreach (WebhookEvent ev in pending)
                    {
                        if (IsInProgress(ev.GUID)) continue;
                        else AddInProgress(ev.GUID);

                        Log("processing event " + ev.GUID + " rule " + ev.RuleGUID + " target " + ev.TargetGUID + " operation " + ev.OperationType + " " + ev.Attempt + "/" + ev.MaxAttempts);

                        if (ev.MaxAttempts <= ev.Attempt)
                        {
                            ev.FailedUtc = DateTime.UtcNow;
                            _ORM.Update<WebhookEvent>(ev);

                            OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.TaskFailed });
                            continue;
                        }

                        Task unawaited = Task.Run(async () => 
                        {
                            byte[] body = await _Requests.Read(ev.GUID).ConfigureAwait(false);

                            try
                            {
                                using (RestRequest restReq = new RestRequest(ev.Url, System.Net.Http.HttpMethod.Post))
                                {
                                    restReq.TimeoutMilliseconds = ev.TimeoutMs;
                                    restReq.ContentType = ev.ContentType;

                                    using (RestResponse restResp = await restReq.SendAsync(body, _TokenSource.Token).ConfigureAwait(false))
                                    {
                                        if (restResp == null)
                                        {
                                            #region No-Response

                                            ev.Attempt = ev.Attempt + 1;
                                            ev.HttpStatus = 0;
                                            ev.LastFailureUtc = DateTime.UtcNow;
                                            ev.NextAttemptUtc = DateTime.UtcNow.AddMilliseconds(ev.RetryIntervalMs);

                                            OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.AttemptFailed });
                                            Log("no response from " + ev.Url + " for event " + ev.GUID);

                                            #endregion
                                        }
                                        else if (restResp.StatusCode != ev.ExpectStatus)
                                        {
                                            #region Failure

                                            ev.Attempt = ev.Attempt + 1;
                                            ev.HttpStatus = restResp.StatusCode;
                                            ev.LastFailureUtc = DateTime.UtcNow;
                                            ev.NextAttemptUtc = DateTime.UtcNow.AddMilliseconds(ev.RetryIntervalMs);

                                            if (restResp.DataAsBytes != null)
                                            {
                                                if (_Responses.Exists(ev.GUID)) _Responses.Delete(ev.GUID);
                                                await _Responses.Write(ev.GUID, restResp.DataAsBytes, _TokenSource.Token).ConfigureAwait(false);
                                            }

                                            OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.AttemptFailed });
                                            Log("failure response from " + ev.Url + " for event " + ev.GUID + ": " + restResp.StatusCode);

                                            #endregion
                                        }
                                        else
                                        {
                                            #region Success

                                            ev.Attempt = ev.Attempt + 1;
                                            ev.HttpStatus = restResp.StatusCode;
                                            ev.SuccessUtc = DateTime.UtcNow;
                                            ev.NextAttemptUtc = null;

                                            if (restResp.DataAsBytes != null)
                                            {
                                                if (_Responses.Exists(ev.GUID)) _Responses.Delete(ev.GUID);
                                                await _Responses.Write(ev.GUID, restResp.DataAsBytes, _TokenSource.Token).ConfigureAwait(false);
                                            }

                                            OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.Succeeded });
                                            Log("success response from " + ev.Url + " for event " + ev.GUID + ": " + restResp.StatusCode);

                                            #endregion
                                        }

                                        WebhookEvent updated = _ORM.Update<WebhookEvent>(ev);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log("exception processing event " + ev.GUID + " to " + ev.Url + ": " + e.Message);

                                ev.Attempt = ev.Attempt + 1;
                                ev.HttpStatus = 0;
                                ev.LastFailureUtc = DateTime.UtcNow;

                                OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.AttemptFailed });
                                WebhookEvent updated = _ORM.Update<WebhookEvent>(ev);
                            }
                            finally
                            {
                                RemoveInProgress(ev.GUID);
                            }

                        }, _TokenSource.Token);

                    }
                }
                catch (Exception eOuter)
                {
                    Log("exception encountered in processing task: " + Environment.NewLine + eOuter.ToString());
                }
            }
        }

        private async Task CleanupTask()
        {
            bool firstRun = true;

            Log("starting cleanup task");

            while (!_TokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (!firstRun) await Task.Delay(_Settings.PollIntervalMs);
                    else firstRun = false;

                    DateTime eval = DateTime.UtcNow.AddMilliseconds(-1 * _Settings.ResponseRetentionMs);

                    List<WebhookEvent> expired = _Events.GetExpired(eval, null, null, null, _Settings.BatchSize);
                    if (expired == null || expired.Count < 1)
                    {
                        Log("no expired tasks to process"); 
                        continue;
                    }

                    Log("retrieved batch of " + expired.Count + " expired webhook invocations");

                    foreach (WebhookEvent ev in expired)
                    {
                        Log("removing expired event " + ev.GUID + " rule " + ev.RuleGUID + " target " + ev.TargetGUID + " operation " + ev.OperationType);

                        if (_Requests.Exists(ev.GUID)) _Requests.Delete(ev.GUID);

                        if (_Responses.Exists(ev.GUID)) _Responses.Delete(ev.GUID);

                        _Events.Remove(ev.GUID);

                        OnWebhookEvent?.Invoke(this, new WebhookEventArgs { Event = ev, Status = WebhookEventStatusEnum.Expired });
                    }
                }
                catch (Exception e)
                {
                    Log("exception encountered in cleanup task: " + Environment.NewLine + e.ToString());
                }
            }
        }

        private bool IsInProgress(string guid)
        {
            lock (_InProgressLock)
            {
                return _InProgress.Contains(guid);
            }
        }

        private void AddInProgress(string guid)
        {
            lock (_InProgressLock)
            {
                _InProgress.Add(guid);
            }
        }

        private void RemoveInProgress(string guid)
        {
            lock (_InProgressLock)
            {
                if (_InProgress.Contains(guid)) _InProgress.Remove(guid);
            }
        }

        #endregion
    }
}
