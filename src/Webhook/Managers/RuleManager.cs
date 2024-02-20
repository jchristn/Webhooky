namespace Webhook.Managers
{
    using System;
    using System.Collections.Generic;
    using ExpressionTree;
    using Watson.ORM;

    /// <summary>
    /// WebhookRule manager.
    /// </summary>
    public class RuleManager
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
        public RuleManager(WebhookSettings settings, WatsonORM orm)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="rule">WebhookRule.</param>
        /// <returns>WebhookRule.</returns>
        public WebhookRule Add(WebhookRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            return _ORM.Insert<WebhookRule>(rule);
        }

        /// <summary>
        /// Update.
        /// </summary>
        /// <param name="rule">WebhookRule.</param>
        /// <returns>WebhookRule.</returns>
        public WebhookRule Update(WebhookRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            return _ORM.Update<WebhookRule>(rule);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="guid">GUID.</param>
        public void Remove(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookRule>(nameof(WebhookRule.GUID)),
                OperatorEnum.Equals,
                guid);

            _ORM.DeleteMany<WebhookRule>(e);
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List of WebhookRule.</returns>
        public List<WebhookRule> All()
        {
            Expr e = new Expr(
                _ORM.GetColumnName<WebhookRule>(nameof(WebhookRule.GUID)),
                OperatorEnum.IsNotNull,
                null);

            return _ORM.SelectMany<WebhookRule>(e);
        }

        /// <summary>
        /// Verify existence.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>Boolean indicating existence.</returns>
        public bool Exists(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookRule>(nameof(WebhookRule.GUID)),
                OperatorEnum.Equals,
                guid);

            return _ORM.Exists<WebhookRule>(e);
        }

        /// <summary>
        /// Find matching entries.
        /// </summary>
        /// <param name="operationType">Operation type.</param>
        /// <param name="targetGuid">Target GUID.</param>
        /// <returns>List of WebhookRule.</returns>
        public List<WebhookRule> FindMatching(string operationType, string targetGuid = null)
        {
            if (String.IsNullOrEmpty(operationType)) throw new ArgumentNullException(nameof(operationType));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookRule>(nameof(WebhookRule.OperationType)),
                OperatorEnum.Contains,
                operationType);

            if (!String.IsNullOrEmpty(targetGuid))
            {
                e = e.PrependAnd(
                    _ORM.GetColumnName<WebhookRule>(nameof(WebhookRule.TargetGUID)),
                    OperatorEnum.Equals,
                    targetGuid);
            }

            List<WebhookRule> matching = _ORM.SelectMany<WebhookRule>(e);
            if (matching == null) return new List<WebhookRule>();
            return matching;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
