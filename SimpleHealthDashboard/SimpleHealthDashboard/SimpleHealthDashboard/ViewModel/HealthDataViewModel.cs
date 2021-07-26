using SimpleHealthDashboard.Model;
using SimpleHealthDashboard.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace SimpleHealthDashboard.ViewModel
{
    class HealthDataViewModel: INotifyPropertyChanged
    {
        private IDataController dataControllerService;

        private ObservableCollection<HealthData> healthDataList;
        private double totalDistance, totalCal;
        private int lastHeartRateReading;
        private string lastHeartRateReadingDate;
        private bool isLoading;

        public ObservableCollection<HealthData> HealthDataList
        {
            get
            {
                return healthDataList;
            }
            set
            {
                if (healthDataList != value)
                {
                    healthDataList = value;
                    OnPropertyChanged("HealthDataList");
                }
            }
        }
        public int LastHeartRateReading
        {
            get
            {
                return lastHeartRateReading;
            }
            set
            {
                if (lastHeartRateReading != value)
                {
                    lastHeartRateReading = value;
                    OnPropertyChanged("LastHeartRateReading");
                }
            }
        }
        public string LastHeartRateReadingDate
        {
            get
            {
                return lastHeartRateReadingDate;
            }
            set
            {
                if (lastHeartRateReadingDate != value)
                {
                    lastHeartRateReadingDate = value;
                    OnPropertyChanged("LastHeartRateReadingDate");
                }
            }
        }
        public double TotalDistance
        {
            get
            {
                return totalDistance;
            }
            set
            {
                if (totalDistance != value)
                {
                    totalDistance = value;
                    OnPropertyChanged("TotalDistance");
                }
            }
        }
        public double TotalCal
        {
            get
            {
                return totalCal;
            }
            set
            {
                if (totalCal != value)
                {
                    totalCal = value;
                    OnPropertyChanged("TotalCal");
                }
            }
        }
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged("IsLoading");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public HealthDataViewModel()
        {
            this.dataControllerService = DependencyService.Get<IDataController>();
            IsLoading = true;
            GetHealthData();
        }
        private async void GetHealthData()
        {
            var Response = dataControllerService.GetHealthData();
            try
            {
                await Response;
                if (Response.Result != null)
                {
                    HealthDataList = new ObservableCollection<HealthData>(Response.Result);
                    UpdateFields();
                }
            }
            catch (Exception Ex)
            {

            }
        }
        void UpdateFields()
        {
            foreach (HealthData data in HealthDataList)
            {
                if (data.HeartBeat != 0)
                {
                    LastHeartRateReading = data.HeartBeat;
                    LastHeartRateReadingDate = data.Date.ToString("dd MMM yy");
                }
                TotalCal += data.Calories;
                TotalDistance += data.Distance;
            }
            TotalDistance = TotalDistance / 1000;
            IsLoading = false;

        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
