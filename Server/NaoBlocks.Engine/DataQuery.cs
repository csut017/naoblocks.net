namespace NaoBlocks.Engine
{
    /// <summary>
    /// A base class for all data queries to inherit from.
    /// </summary>
    /// <remarks>
    /// This class will ensure the query is correctly wired to the database session.
    /// </remarks>
    public abstract class DataQuery
    {
        private IDatabaseSession? session;

        /// <summary>
        /// Gets the session to use.
        /// </summary>
        public IDatabaseSession Session
        {
            get
            {
                if (session == null) throw new InvalidOperationException("Session has not been initialised");
                return session;
            }
        }

        /// <summary>
        /// Initialises the database session for this query.
        /// </summary>
        /// <param name="session">The session to use.</param>
        public void InitialiseSession(IDatabaseSession session)
        {
            this.session = session;
        }
    }
}
