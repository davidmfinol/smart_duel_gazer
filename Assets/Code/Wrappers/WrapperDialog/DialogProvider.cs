using Code.Core.Dialog.Entities;
using UnityEditor;

namespace Code.Wrappers.WrapperDialog
{
    public interface IDialogProvider
    {
        bool ShowDialog(DialogConfig config);
    }

    public class DialogProvider : IDialogProvider
    {
        public bool ShowDialog(DialogConfig config)
        {
            return EditorUtility.DisplayDialog(config.Title, config.Description, config.PositiveText, config.NegativeText);
        }
    }
}