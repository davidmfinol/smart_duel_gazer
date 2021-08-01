namespace Code.Wrappers.WrapperToast
{
    public interface IToastProvider
    {
        void ShowToast(string message);
    }
    
    public class ToastProvider : IToastProvider
    {
        public void ShowToast(string message)
        {
            SSTools.ShowMessage(message, SSTools.Position.bottom, SSTools.Time.threeSecond);
        }
    }
}