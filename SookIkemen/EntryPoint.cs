using System;
using System.ComponentModel.Composition;
using System.Linq;
using Acuerdo.Plugin;
using Dulcet.Twitter;
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
                        tab.TabProperty.LinkAccountInfos.ForEach(a =>
                            PostOffice.UpdateTweet(a, "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰｰ #sook_ikemen"));
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.PluginError, "スークイケメンﾅｰｰｰｰｰｰｰｰｰｰｰｰｰｰに失敗しました: " + e.Message);
                    }
                }));
            KeyAssignCore.RegisterOperation("SenselessRetweet", () =>
                KeyAssignHelper.ExecuteTVMAction(tvm =>
                    {
                        var ts = tvm.Tweet.Status as TwitterStatus;
                        if (ts == null) return;
                        KernelService.MainWindowViewModel.InputBlockViewModel.SetOpenText(true, true);
                        KernelService.MainWindowViewModel.InputBlockViewModel.SetText(BuildSenseless(ts));
                    }));
            KeyAssignCore.RegisterOperation("SenselessRetweetFast", () =>
                KeyAssignHelper.ExecuteTVMAction(tvm =>
                {
                    try
                    {
                        var ts = tvm.Tweet.Status as TwitterStatus;
                        if (ts == null) return;
                        tvm.Parent.TabProperty.LinkAccountInfos.ForEach(
                            ai => PostOffice.UpdateTweet(ai, BuildSenseless(ts)));
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.PluginError, "非常識RTに失敗しました: " + e.Message);
                    }
                }));
        }

        private string BuildSenseless(TwitterStatus ts)
        {
            if (ts.RetweetedOriginal != null)
            {
                return "… RT @" + ts.RetweetedOriginal.User.ScreenName + ": " + ts.RetweetedOriginal.Text;
            }
            else
            {
                return "… RT @" + ts.User.ScreenName + ": " + ts.Text;
            }
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
