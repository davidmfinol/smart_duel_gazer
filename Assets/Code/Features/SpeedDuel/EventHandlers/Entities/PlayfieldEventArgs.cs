namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public abstract class PlayfieldEventArgs
    {
    }

    public class PlayfieldEventValue<T> : PlayfieldEventArgs
    {
        private T dataValue;

        public T Value 
        { 
            get => dataValue; 
            set => dataValue = value; 
        }
    }    
}