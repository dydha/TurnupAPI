namespace TurnupAPI.Exceptions
{
    /// <summary>
    /// Exception personnalisée pour signaler qu'une tentative de création d'une ressource en doublon a été détectée.
    /// </summary>
    public class DuplicateException : Exception
    {
        public DuplicateException() { }
        public DuplicateException(string message) : base(message)
        {
        }
    }
}
