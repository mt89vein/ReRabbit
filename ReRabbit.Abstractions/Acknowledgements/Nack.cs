namespace ReRabbit.Abstractions.Acknowledgements
{
    /// <summary>
    /// Неуспешная обработка.
    /// </summary>
    public class Nack : Acknowledgement
    {
        public bool Requeue { get; }

        public Nack(bool requeue = true)
        {
            Requeue = requeue;
        }
    }
}