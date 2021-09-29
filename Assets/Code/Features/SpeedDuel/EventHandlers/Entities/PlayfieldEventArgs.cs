namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public class PlayfieldEventValue<T> : PlayfieldEventArgs
    {
        private T data;

        public T Value 
        { 
            get => data; 
            set => data = value; 
        }
    }

    public abstract class PlayfieldEventArgs
    {
    }
}