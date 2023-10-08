namespace TurnupAPI.Exceptions
{
    /// <summary>
    /// Exception personnalisée pour signaler qu'une ressource n'a pas été trouvée.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() { }
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
