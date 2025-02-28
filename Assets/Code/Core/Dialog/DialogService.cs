﻿using Code.Core.Dialog.Entities;
using Code.Wrappers.WrapperDialog;
using Code.Wrappers.WrapperToast;

namespace Code.Core.Dialog
{
    public interface IDialogService
    {
        void ShowDialog(DialogConfig config);
        void ShowToast(string message);
    }

    public class DialogService : IDialogService
    {
        private readonly IDialogProvider _dialogProvider;
        private readonly IToastProvider _toastProvider;

        public DialogService(
            IDialogProvider dialogProvider,
            IToastProvider toastProvider)
        {
            _dialogProvider = dialogProvider;
            _toastProvider = toastProvider;
        }

        public void ShowDialog(DialogConfig config)
        {
            _dialogProvider.ShowDialog(config);
        }

        public void ShowToast(string message)
        {
            _toastProvider.ShowToast(message);
        }
    }
}