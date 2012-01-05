using System;
using System.Collections.Generic;
using System.Linq;
using Inscribe.Configuration.Tabs;
using Inscribe.Core;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using Inscribe.Filter.Filters.Attributes;
using Inscribe.Filter.Filters.Numeric;
using Inscribe.Filter.Filters.ScreenName;
using Inscribe.Filter.Filters.Text;
using Inscribe.Storage;
using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

namespace Inscribe.ViewModels.Dialogs
{
    public class TabQuickBuilderViewModel : ViewModel
    {
        public TabQuickBuilderViewModel()
        {
        }

        #region Defined timelines

        #region CreateGeneralTimelineCommand
        private ViewModelCommand _CreateGeneralTimelineCommand;

        public ViewModelCommand CreateGeneralTimelineCommand
        {
            get
            {
                if (_CreateGeneralTimelineCommand == null)
                {
                    _CreateGeneralTimelineCommand = new ViewModelCommand(CreateGeneralTimeline);
                }
                return _CreateGeneralTimelineCommand;
            }
        }

        public void CreateGeneralTimeline()
        {
            CreateTab("General", new IFilter[] { new FilterCluster(null, concatAnd: true) });
            Close();
        }
        #endregion

        #region CreateHomeTimelineCommand
        private ViewModelCommand _CreateHomeTimelineCommand;

        public ViewModelCommand CreateHomeTimelineCommand
        {
            get
            {
                if (_CreateHomeTimelineCommand == null)
                {
                    _CreateHomeTimelineCommand = new ViewModelCommand(CreateHomeTimeline);
                }
                return _CreateHomeTimelineCommand;
            }
        }

        public void CreateHomeTimeline()
        {
            CreateTab("Home", new IFilter[]{
                new FilterFollowFrom("*"),
                new FilterTo("*"),
                new FilterUser("*"), new FilterDirectMessage()});
            Close();
        }
        #endregion

        #region CreateMentionTimelineCommand
        private ViewModelCommand _CreateMentionTimelineCommand;

        public ViewModelCommand CreateMentionTimelineCommand
        {
            get
            {
                if (_CreateMentionTimelineCommand == null)
                {
                    _CreateMentionTimelineCommand = new ViewModelCommand(CreateMentionTimeline);
                }
                return _CreateMentionTimelineCommand;
            }
        }

        public void CreateMentionTimeline()
        {
            CreateTab("Mentions",
                new IFilter[]{
                    new FilterCluster(
                        new IFilter[]{
                            new FilterTo("*"),
                            new FilterRetweeted(){Negate=true}},
                            concatAnd: true)});
            Close();
        }
        #endregion

        #region CreateDirectMessagesTimelineCommand
        private ViewModelCommand _CreateDirectMessagesTimelineCommand;

        public ViewModelCommand CreateDirectMessagesTimelineCommand
        {
            get
            {
                if (_CreateDirectMessagesTimelineCommand == null)
                {
                    _CreateDirectMessagesTimelineCommand = new ViewModelCommand(CreateDirectMessagesTimeline);
                }
                return _CreateDirectMessagesTimelineCommand;
            }
        }

        public void CreateDirectMessagesTimeline()
        {
            CreateTab("Messages",
                new IFilter[] { new FilterDirectMessage() });
            Close();
        }
        #endregion

        #region CreateActivitiesTimelineCommand
        private ViewModelCommand _CreateActivitiesTimelineCommand;

        public ViewModelCommand CreateActivitiesTimelineCommand
        {
            get
            {
                if (_CreateActivitiesTimelineCommand == null)
                {
                    _CreateActivitiesTimelineCommand = new ViewModelCommand(CreateActivitiesTimeline);
                }
                return _CreateActivitiesTimelineCommand;
            }
        }

        public void CreateActivitiesTimeline()
        {
            CreateTab("Activities",
                new IFilter[]{
                    new FilterCluster(
                        new IFilter[]{
                            new FilterCluster(
                                    new IFilter[]{
                                    new FilterFavoriteCount(new LongRange(from:1)),
                                    new FilterRetweetCount(new LongRange(from:1))}),
                            new FilterUser("*")}, concatAnd:true)});
            Close();
        }
        #endregion

        #endregion

        #region Account dependent timelines

        public IEnumerable<String> AccountScreenNames
        {
            get { return AccountStorage.Accounts.Select(a => a.ScreenName); }
        }

        private int _selectedIndex = 0;
        public int AccountSelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(() => AccountSelectedIndex);
            }
        }

        private string SelectedScreenName
        {
            get
            {
                return AccountStorage.Accounts
                    .Select(a => a.ScreenName)
                    .ElementAtOrDefault(AccountSelectedIndex) ?? "*";
            }
        }

        #region CreateAccountHomeTimelineCommand
        private ViewModelCommand _CreateAccountHomeTimelineCommand;

        public ViewModelCommand CreateAccountHomeTimelineCommand
        {
            get
            {
                if (_CreateAccountHomeTimelineCommand == null)
                {
                    _CreateAccountHomeTimelineCommand = new ViewModelCommand(CreateAccountHomeTimeline);
                }
                return _CreateAccountHomeTimelineCommand;
            }
        }

        public void CreateAccountHomeTimeline()
        {
            CreateTab("Home", new IFilter[]{
                new FilterFollowFrom(SelectedScreenName),
                new FilterTo(SelectedScreenName),
                new FilterUser(SelectedScreenName), 
                new FilterCluster(
                    new IFilter[]{
                        new FilterDirectMessage(),
                        new FilterTo(SelectedScreenName)
                    }, concatAnd: true)});
            Close();
        }
        #endregion

        #region CreateAccountMentionTimelineCommand
        private ViewModelCommand _CreateAccountMentionTimelineCommand;

        public ViewModelCommand CreateAccountMentionTimelineCommand
        {
            get
            {
                if (_CreateAccountMentionTimelineCommand == null)
                {
                    _CreateAccountMentionTimelineCommand = new ViewModelCommand(CreateAccountMentionTimeline);
                }
                return _CreateAccountMentionTimelineCommand;
            }
        }

        public void CreateAccountMentionTimeline()
        {
            CreateTab("Mentions",
                new IFilter[]{
                    new FilterCluster(
                        new IFilter[]{
                            new FilterTo(SelectedScreenName),
                            new FilterRetweeted(){Negate=true}},
                            concatAnd: true)});
            Close();
        }
        #endregion

        #region CreateAccountDirectMessagesTimelineCommand
        private ViewModelCommand _CreateAccountDirectMessagesTimelineCommand;

        public ViewModelCommand CreateAccountDirectMessagesTimelineCommand
        {
            get
            {
                if (_CreateAccountDirectMessagesTimelineCommand == null)
                {
                    _CreateAccountDirectMessagesTimelineCommand = new ViewModelCommand(CreateAccountDirectMessagesTimeline);
                }
                return _CreateAccountDirectMessagesTimelineCommand;
            }
        }

        public void CreateAccountDirectMessagesTimeline()
        {
            CreateTab("Messages",
                new IFilter[] {
                    new FilterCluster(
                        new IFilter[]
                        {
                            new FilterDirectMessage(),
                            new FilterTo(SelectedScreenName)
                        }, concatAnd: true)});
            Close();
        }
        #endregion


        #region CreateAccountActivitiesTimelineCommand
        private ViewModelCommand _CreateAccountActivitiesTimelineCommand;

        public ViewModelCommand CreateAccountActivitiesTimelineCommand
        {
            get
            {
                if (_CreateAccountActivitiesTimelineCommand == null)
                {
                    _CreateAccountActivitiesTimelineCommand = new ViewModelCommand(CreateAccountActivitiesTimeline);
                }
                return _CreateAccountActivitiesTimelineCommand;
            }
        }

        public void CreateAccountActivitiesTimeline()
        {
            CreateTab("Activities",
                new IFilter[]{
                    new FilterCluster(
                        new IFilter[]{
                            new FilterCluster(
                                    new IFilter[]{
                                    new FilterFavoriteCount(new LongRange(from:1)),
                                    new FilterRetweetCount(new LongRange(from:1))}),
                            new FilterUser(SelectedScreenName)}, concatAnd:true)});
            Close();
        }
        #endregion



        #endregion

        #region Keyword extraction

        string _extractionKeywords = String.Empty;
        public string ExtractionKeywords
        {
            get { return _extractionKeywords; }
            set
            {
                _extractionKeywords = value;
                RaisePropertyChanged(() => ExtractionKeywords);
                CreateTextExtractionTimelineCommand.RaiseCanExecuteChanged();
            }
        }

        #region CreateTextExtractionTimelineCommand
        private ViewModelCommand _CreateTextExtractionTimelineCommand;

        public ViewModelCommand CreateTextExtractionTimelineCommand
        {
            get
            {
                if (_CreateTextExtractionTimelineCommand == null)
                {
                    _CreateTextExtractionTimelineCommand = new ViewModelCommand(CreateTextExtractionTimeline, CanCreateTextExtractionTimeline);
                }
                return _CreateTextExtractionTimelineCommand;
            }
        }

        public bool CanCreateTextExtractionTimeline()
        {
            return !String.IsNullOrWhiteSpace(_extractionKeywords.Replace(Environment.NewLine, ""));
        }

        public void CreateTextExtractionTimeline()
        {
            var keywords = ExtractionKeywords
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            CreateTab(keywords.First(),
                keywords.Select(k => new FilterText(k)).ToArray());
            Close();
        }
        #endregion


        #endregion

        #region CloseCommand
        private ViewModelCommand _CloseCommand;

        public ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new ViewModelCommand(Close);
                }
                return _CloseCommand;
            }
        }

        private void Close()
        {
            this.Messenger.Raise(new WindowActionMessage("WindowAction", WindowAction.Close));
        }

        #endregion

        private void CreateTab(String name, IEnumerable<IFilter> filter)
        {
            CreateTab(new TabProperty(name, filter));
        }

        private void CreateTab(TabProperty property)
        {
            KernelService.MainWindowViewModel.ColumnOwnerViewModel.CurrentFocusColumn.AddTab(property);
        }
    }
}
