namespace Webhook.Managers
{
    using ExpressionTree;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Watson.ORM;

    /// <summary>
    /// WebhookTarget manager.
    /// </summary>
    public class TargetManager
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
        public TargetManager(WebhookSettings settings, WatsonORM orm)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ORM = orm ?? throw new ArgumentNullException(nameof(orm));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="target">WebhookTarget.</param>
        /// <returns>WebhookTarget.</returns>
        public WebhookTarget Add(WebhookTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return _ORM.Insert<WebhookTarget>(target);
        }

        /// <summary>
        /// Add.
        /// </summary>
        /// <param name="target">WebhookTarget.</param>
        /// <returns>WebhookTarget.</returns>
        public WebhookTarget Update(WebhookTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return _ORM.Update<WebhookTarget>(target);
        }

        /// <summary>
        /// Remove.
        /// </summary>
        /// <param name="guid">GUID.</param>
        public void Remove(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookTarget>(nameof(WebhookTarget.GUID)),
                OperatorEnum.Equals,
                guid);

            _ORM.DeleteMany<WebhookTarget>(e);
        }

        /// <summary>
        /// Retrieve all.
        /// </summary>
        /// <returns>List of WebhookTarget.</returns>
        public List<WebhookTarget> All()
        {
            Expr e = new Expr(
                _ORM.GetColumnName<WebhookTarget>(nameof(WebhookTarget.GUID)),
                OperatorEnum.IsNotNull,
                null);

            return _ORM.SelectMany<WebhookTarget>(e);
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
                _ORM.GetColumnName<WebhookTarget>(nameof(WebhookTarget.GUID)),
                OperatorEnum.Equals,
                guid);

            return _ORM.Exists<WebhookTarget>(e);
        }

        /// <summary>
        /// Read.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <returns>WebhookTarget.</returns>
        public WebhookTarget Read(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));

            Expr e = new Expr(
                _ORM.GetColumnName<WebhookTarget>(nameof(WebhookTarget.GUID)),
                OperatorEnum.Equals,
                guid);

            return _ORM.SelectFirst<WebhookTarget>(e);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
