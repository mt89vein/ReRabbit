namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Неуспешная обработка.
    /// </summary>
    public class Reject : Acknowledgement
    {
        public bool Requeue { get; }

        public Reject(bool requeue = true)
        {
            Requeue = requeue;
        }
    }
}