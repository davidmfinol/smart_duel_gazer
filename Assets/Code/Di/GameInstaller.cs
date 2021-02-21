using Project.Core.Dialog.Impl;
using Project.Core.Dialog.Interface;
using Zenject;

namespace Project.Di
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IDialogService>().To<DialogService>().AsSingle();
        }
    }
}
