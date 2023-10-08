namespace TurnupAPI.Exceptions
{
    /// <summary>
    /// Exception personnalisée pour gérer les erreurs liées aux services externes (API tierces).
    /// </summary>
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException(string message) : base(message)
        {
        }
    }
}
