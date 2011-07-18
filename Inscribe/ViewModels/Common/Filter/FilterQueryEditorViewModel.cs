using System;
using System.Linq;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using Livet;
using System.Threading.Tasks;

namespace Inscribe.ViewModels.Common.Filter
{
    public class FilterQueryEditorViewModel : ViewModel
    {
        public bool Success = false;

        public FilterCluster FilterCluster { get; private set; }

        public FilterQueryEditorViewModel(IFilter[] clusters)
        {
            var filter = QueryCompiler.ToFilter("(" + String.Join("|", clusters.Select(s => s.ToQuery())) + ")");
            this.Text = filter.ToQuery();
        }

        private string _result;

        public string Result
        {
            get { return _result; }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged(() => Text);
                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            this.FilterCluster = QueryCompiler.ToFilter(value);
                            this._result = "クエリは正しく解釈できました。";
                            this.Success = true;
                        }
                        catch (Exception e)
                        {
                            this.FilterCluster = null;
                            this.Success = false;
                            this._result = e.Message;
                        }
                        RaisePropertyChanged(() => Result);
                    });
            }
        }
    }
}
