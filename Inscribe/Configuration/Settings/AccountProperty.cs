using System.Linq;
using Inscribe.Model;
using Inscribe.Storage;

namespace Inscribe.Configuration.Settings
{
    public class AccountProperty
    {
        public AccountInfo[] Accounts
        {
            get { return AccountStorage.Accounts.ToArray(); }
            set
            {
                if (value != null)
                {
                    value.ForEach(a => AccountStorage.RegisterAccount(a));
                }
            }
        }
    }
}
