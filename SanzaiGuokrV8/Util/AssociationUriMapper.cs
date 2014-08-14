using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SanzaiGuokrV8.Util
{
    public class AssociationUriMapper : UriMapperBase
    {
        private string tempUri;
        public override Uri MapUri(Uri uri)
        {
            tempUri = uri.ToString();
            if (tempUri.Contains("/FileTypeAssociation"))
            {
                try
                {
                    return IsolatedStorageSettings.ApplicationSettings["lastUri"] as Uri;
                }
                catch
                {
                    return new Uri("/MainPage.xaml", UriKind.Relative);
                }
            }
            else
            {
                return uri;
            }
        }
    }
}
