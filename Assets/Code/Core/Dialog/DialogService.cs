namespace Code.Core.Dialog
{
    public interface IDialogService
    {
        void ShowToast(string message);
    }
    
    public class DialogService : IDialogService
    {
        public void ShowToast(string message)
        {
            SSTools.ShowMessage(message, SSTools.Position.bottom, SSTools.Time.twoSecond);
        }
    }
}
