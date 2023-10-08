namespace TurnupAPI.Exceptions
{
    /// <summary>
    /// Exception personnalisée pour encapsuler toutes les erreurs liées à l'accès aux données, telles que les erreurs de base de données ou de connexion.
    /// </summary>
    public class DataAccessException : Exception
    {
        public DataAccessException() { }

        public DataAccessException(string message) : base(message) { }

        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
}
