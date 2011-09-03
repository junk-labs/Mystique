using System;
using System.ComponentModel.Composition;
using System.Linq;
using Acuerdo.Plugin;
using Inscribe.Common;
using Inscribe.Communication.Posting;
using Inscribe.Core;
using Inscribe.Storage;
using Inscribe.Subsystems;

namespace SookIkemen
{
    [Export(typeof(IPlugin))]
    public class EntryPoint : IPlugin
    {
        public string Name
        {
            get { return "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰ"; }
        }

        public double Version
        {
            get { return 1.1; }
        }

        public void Loaded()
        {
            KeyAssignCore.RegisterOperation("SookIkemen", () =>
                KeyAssignHelper.ExecuteTabAction(tab =>
                {
                    try
                    {
                        tab.TabProperty.LinkAccountInfos.ForEach(a => PostOffice.UpdateTweet(a, "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰ #sookikemen"));
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.PluginError, "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰに失敗しました: " + e.Message, () => Loaded());
                    }
                }));
            KeyAssignCore.RegisterOperation("HageSon", () =>
                KeyAssignHelper.ExecuteTVMAction(tvm =>
                    {
                        KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                    }));
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
