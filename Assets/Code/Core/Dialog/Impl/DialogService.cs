using Project.Core.Dialog.Interface;

namespace Project.Core.Dialog.Impl
{
    public class DialogService : IDialogService
    {
        public void ShowToast(string message)
        {
            SSTools.ShowMessage(message, SSTools.Position.bottom, SSTools.Time.twoSecond);
        }
    }
}
