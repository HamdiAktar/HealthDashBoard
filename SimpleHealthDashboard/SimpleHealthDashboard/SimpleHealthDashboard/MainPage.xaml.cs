using SimpleHealthDashboard.Model;
using SimpleHealthDashboard.Services;
using SimpleHealthDashboard.ViewModel;
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SimpleHealthDashboard
{
    public partial class MainPage : ContentPage
    {
        private IAuth AuthService;

        public ObservableCollection<HealthData> HealthDataList { get; private set; }
        public MainPage()
        {
            InitializeComponent();
            this.AuthService = DependencyService.Get<IAuth>();
        }

        private void Authorize_Button_clicked(object sender, EventArgs e)
        {
            AuthService.SignIn();
        }

        private async void Fetch_Button_Clicked(object sender, EventArgs e)
        {
           await Navigation.PushAsync(new Cards());
        }
    }
}
