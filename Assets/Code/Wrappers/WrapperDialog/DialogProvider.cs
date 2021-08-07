using Code.Core.Dialog.Entities;
#if UNITY_IOS
using Nrjwolf.Tools;
#endif
#if UNITY_ANDROID
using Nrjwolf.Tools.AndroidEasyAlerts;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Wrappers.WrapperDialog
{
    public interface IDialogProvider
    {
        void ShowDialog(DialogConfig config);
    }

    public class DialogProvider : IDialogProvider
    {
        public void ShowDialog(DialogConfig config)
        {
#if UNITY_EDITOR
            var result = EditorUtility.DisplayDialog(
                config.Title,
                config.Description,
                config.PositiveText
            );

            if (result)
            {
                config.PositiveAction.Invoke();
            }
#elif UNITY_IOS
            IOSNativeAlert.ShowAlertMessage(
                config.Title,
                config.Description,
                new IOSNativeAlert.AlertButton(config.PositiveText, config.PositiveAction)
            );
#elif UNITY_ANDROID
            AndroidEasyAlerts.ShowAlert(
                config.Title,
                config.Description,
                new AlertButton(config.PositiveText, config.PositiveAction, ButtonStyle.POSITIVE)
            );
#endif
        }
    }
}