namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public abstract class PlayfieldEventArgs
    {
    }

    public class PlayfieldEventValue<T> : PlayfieldEventArgs
    {
        private T _dataValue;

        public T Value 
        { 
            get => _dataValue; 
            set => _dataValue = value; 
        }
    }    
}