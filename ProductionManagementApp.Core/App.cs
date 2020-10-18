using MvvmCross;
using MvvmCross.ViewModels;
using ProductionManagementApp.Core.ViewModels;
using System;

namespace ProductionManagementApp.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<ProfileSelectViewModel>();
        }
    }
}
