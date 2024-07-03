using Page_Navigation_App.Model;
using Page_Navigation_App.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Page_Navigation_App.ViewModel
{
    class AboutVM : ViewModelBase
    {
        private readonly PageModel _pageModel;
        public string AboutMessage
        {
            get { return _pageModel.AboutMessage; }
            set { _pageModel.AboutMessage = value; OnPropertyChanged(); }
        }

        public AboutVM()
        {
            _pageModel = new PageModel();
            AboutMessage = "This is the About Page";
        }
    }
}
