using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Page_Navigation_App.Utilities;
using System.Windows.Input;

namespace Page_Navigation_App.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand CustomersCommand { get; set; }
        public ICommand ProductsCommand { get; set; }
        public ICommand OrdersCommand { get; set; }
        public ICommand TransactionsCommand { get; set; }
        public ICommand ShipmentsCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand AboutCommand {  get; set; }
        public ICommand ModelCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void About(object obj) => CurrentView = new AboutVM();
        private void Model(object obj) => CurrentView = new ModelVM();

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            AboutCommand = new RelayCommand(About);
            ModelCommand = new RelayCommand(Model);
            CurrentView = new HomeVM();
        }
    }
}
