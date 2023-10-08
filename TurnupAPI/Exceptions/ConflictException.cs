namespace TurnupAPI.Exceptions
{
    /// <summary>
    /// Exception personnalisée pour signaler des conflits entre ressources.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
