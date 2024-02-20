namespace Webhook.Managers
{
    using DatabaseWrapper.Core;
    using ExpressionTree;
    using System;
    using System.Collections.Generic;
    using Watson.ORM;

    /// <summary>
    /// WebhookEvent manager.
    /// </summary>
    public class EventManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private WebhookSettings _Settings = null;
        private WatsonORM _ORM = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="orm">ORM.</param>
        public EventManager(WebhookSettings settings, WatsonORM orm)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve all by rule GUID.
        /// </summary>
        /// <param name="guid">Rule GUID.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> AllByRule(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.RuleGUID)),
                OperatorEnum.Equals,
                guid);

            return _ORM.SelectMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Retrieve all by target GUID.
        /// </summary>
        /// <param name="guid">Target GUID.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> AllByTarget(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.TargetGUID)),
                OperatorEnum.Equals,
                guid);

            return _ORM.SelectMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Retrieve all by operation type.
        /// </summary>
        /// <param name="operationType">Operation type.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> AllByOperation(string operationType)
        {
            if (String.IsNullOrEmpty(operationType)) throw new ArgumentNullException(nameof(operationType));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.OperationType)),
                OperatorEnum.Equals,
                operationType);

            return _ORM.SelectMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="guid">GUID.</param>
        public void Remove(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.GUID)),
                OperatorEnum.Equals,
                guid);

            _ORM.DeleteMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Remove all by target GUID.
        /// </summary>
        /// <param name="guid">Target GUID.</param>
        public void RemoveAllByTarget(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.TargetGUID)),
                OperatorEnum.Equals,
                guid);

            _ORM.DeleteMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Remove all by rule GUID.
        /// </summary>
        /// <param name="guid">Rule GUID.</param>
        public void RemoveAllByRule(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.RuleGUID)),
                OperatorEnum.Equals,
                guid);

            _ORM.DeleteMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Remove all by operation type.
        /// </summary>
        /// <param name="operationType">Operation type.</param>
        public void RemoveAllByOperation(string operationType)
        {
            if (String.IsNullOrEmpty(operationType)) throw new ArgumentNullException(nameof(operationType));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.OperationType)),
                OperatorEnum.Equals,
                operationType);

            _ORM.DeleteMany<WebhookEvent>(e);
        }

        /// <summary>
        /// Retrieve pending events.
        /// </summary>
        /// <param name="targetGuid">Target GUID.</param>
        /// <param name="ruleGuid">Rule GUID.</param>
        /// <param name="operationType">Operation type.</param>
        /// <param name="maxResults">Maximum number of results.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> GetPending(string targetGuid = null, string ruleGuid = null, string operationType = null, int maxResults = 10)
        {
            if (maxResults < 1) throw new ArgumentOutOfRangeException(nameof(maxResults));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.IsNotNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.LessThan,
                DateTime.UtcNow);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.SuccessUtc)),
                OperatorEnum.IsNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.FailedUtc)),
                OperatorEnum.IsNull,
                null);

            if (!String.IsNullOrEmpty(targetGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.TargetGUID)),
                    OperatorEnum.Equals,
                    targetGuid);
            }

            if (!String.IsNullOrEmpty(ruleGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.RuleGUID)),
                    OperatorEnum.Equals,
                    ruleGuid);
            }

            if (!String.IsNullOrEmpty(operationType))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.OperationType)),
                    OperatorEnum.Equals,
                    operationType);
            }

            ResultOrder[] ro = new ResultOrder[1];
            ro[0] = new ResultOrder(_ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)), OrderDirectionEnum.Ascending);

            return _ORM.SelectMany<WebhookEvent>(0, maxResults, e, ro);
        }

        /// <summary>
        /// Retrieve failed events.
        /// </summary>
        /// <param name="targetGuid">Target GUID.</param>
        /// <param name="ruleGuid">Rule GUID.</param>
        /// <param name="operationType">Operation type.</param>
        /// <param name="maxResults">Maximum number of results.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> GetFailed(string targetGuid = null, string ruleGuid = null, string operationType = null, int maxResults = 10)
        {
            if (maxResults < 1) throw new ArgumentOutOfRangeException(nameof(maxResults));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.IsNotNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.LessThan,
                DateTime.UtcNow);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.SuccessUtc)),
                OperatorEnum.IsNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.FailedUtc)),
                OperatorEnum.IsNotNull,
                null);

            if (!String.IsNullOrEmpty(targetGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.TargetGUID)),
                    OperatorEnum.Equals,
                    targetGuid);
            }

            if (!String.IsNullOrEmpty(ruleGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.RuleGUID)),
                    OperatorEnum.Equals,
                    ruleGuid);
            }

            if (!String.IsNullOrEmpty(operationType))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.OperationType)),
                    OperatorEnum.Equals,
                    operationType);
            }

            ResultOrder[] ro = new ResultOrder[1];
            ro[0] = new ResultOrder(_ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)), OrderDirectionEnum.Ascending);

            return _ORM.SelectMany<WebhookEvent>(0, maxResults, e, ro);
        }

        /// <summary>
        /// Retrieve successful events.
        /// </summary>
        /// <param name="targetGuid">Target GUID.</param>
        /// <param name="ruleGuid">Rule GUID.</param>
        /// <param name="operationType">Operation type.</param>
        /// <param name="maxResults">Maximum number of results.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> GetSucceeded(string targetGuid = null, string ruleGuid = null, string operationType = null, int maxResults = 10)
        {
            if (maxResults < 1) throw new ArgumentOutOfRangeException(nameof(maxResults));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.IsNotNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.LessThan,
                DateTime.UtcNow);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.SuccessUtc)),
                OperatorEnum.IsNotNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.FailedUtc)),
                OperatorEnum.IsNull,
                null);

            if (!String.IsNullOrEmpty(targetGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.TargetGUID)),
                    OperatorEnum.Equals,
                    targetGuid);
            }

            if (!String.IsNullOrEmpty(ruleGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.RuleGUID)),
                    OperatorEnum.Equals,
                    ruleGuid);
            }

            if (!String.IsNullOrEmpty(operationType))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.OperationType)),
                    OperatorEnum.Equals,
                    operationType);
            }

            ResultOrder[] ro = new ResultOrder[1];
            ro[0] = new ResultOrder(_ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)), OrderDirectionEnum.Ascending);

            return _ORM.SelectMany<WebhookEvent>(0, maxResults, e, ro);
        }

        /// <summary>
        /// Retrieve expired events.
        /// </summary>
        /// <param name="expiration">Expiration timestamp, UTC.</param>
        /// <param name="targetGuid">Target GUID.</param>
        /// <param name="ruleGuid">Rule GUID.</param>
        /// <param name="operationType">Operation type.</param>
        /// <param name="maxResults">Maximum number of results.</param>
        /// <returns>List of WebhookEvent.</returns>
        public List<WebhookEvent> GetExpired(DateTime expiration, string targetGuid = null, string ruleGuid = null, string operationType = null, int maxResults = 10)
        {
            if (maxResults < 1) throw new ArgumentOutOfRangeException(nameof(maxResults));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.IsNotNull,
                null);

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)),
                OperatorEnum.LessThan,
                DateTime.UtcNow);

            e = e.PrependAnd(
                new Expr(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.SuccessUtc)),
                    OperatorEnum.IsNotNull,
                    null),
                OperatorEnum.Or,
                new Expr(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.FailedUtc)),
                    OperatorEnum.IsNotNull,
                    null));

            e = e.PrependAnd(
                _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.AddedUtc)),
                OperatorEnum.LessThan,
                expiration);

            if (!String.IsNullOrEmpty(targetGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.TargetGUID)),
                    OperatorEnum.Equals,
                    targetGuid);
            }

            if (!String.IsNullOrEmpty(ruleGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.RuleGUID)),
                    OperatorEnum.Equals,
                    ruleGuid);
            }

            if (!String.IsNullOrEmpty(operationType))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.OperationType)),
                    OperatorEnum.Equals,
                    operationType);
            }

            ResultOrder[] ro = new ResultOrder[1];
            ro[0] = new ResultOrder(_ORM.GetColumnName<WebhookEvent>(nameof(WebhookEvent.NextAttemptUtc)), OrderDirectionEnum.Ascending);

            return _ORM.SelectMany<WebhookEvent>(0, maxResults, e, ro);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
