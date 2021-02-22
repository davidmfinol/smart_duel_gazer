using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;

namespace AssemblyCSharp.Assets.Code.Core.Dialog.Impl
{
    public class DialogService : IDialogService
    {
        public void ShowToast(string message)
        {
            SSTools.ShowMessage(message, SSTools.Position.bottom, SSTools.Time.twoSecond);
        }
    }
}
