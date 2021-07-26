using SimpleHealthDashboard.Droid.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SimpleHealthDashboard.Services;
using System.Threading.Tasks;
using SimpleHealthDashboard.Model;
using Huawei.Hms.Hihealth;
using Huawei.Hms.Hihealth.Data;
using Android.Util;
using Java.Util.Concurrent;
using DataType = Huawei.Hms.Hihealth.Data.DataType;
using Huawei.Hms.Support.Hwid.Result;
using Huawei.Hms.Support.Hwid;

[assembly: Dependency(typeof(DataControllerService))]
namespace SimpleHealthDashboard.Droid.Services
{
    class DataControllerService : IDataController
    {
        private static string TAG = "DataController";

        // Object of controller for fitness and health data, providing APIs for read/write.
        private DataController MyDataController;

        private const double lastSevenDays = -30;
        private const string DateFormat = "yyyyMMdd";

        private List<HealthData> fitnessActivities= new List<HealthData>();
        private DateTime startDate;
        private DateTime endDate;

        public DataControllerService()
        { 
            SetDatesRange();
            InitDataController();

            // initialize the list with entries for the last month days
            for (DateTime temp= startDate;  temp <= endDate; temp=temp.AddDays(1) )
            {
                fitnessActivities.Add(new HealthData
                {
                    FormatedDate = Int32.Parse(temp.ToString(DateFormat)),
                    Date = temp
                });
            }
        }

        private void InitDataController()
        {
            // Obtain and set the read permissions for the required DataTypes.
            // Use the obtained permissions to obtain the data controller object.
            HiHealthOptions hiHealthOptions = HiHealthOptions.HiHealthOptionsBulider()
                .AddDataType(DataType.DtContinuousStepsDelta, HiHealthOptions.AccessRead)
                .AddDataType(DataType.DtContinuousCaloriesBurnt, HiHealthOptions.AccessRead)
                .AddDataType(DataType.DtContinuousDistanceDelta, HiHealthOptions.AccessRead)
                .AddDataType(DataType.DtInstantaneousHeartRate, HiHealthOptions.AccessRead)
                .Build();
            AuthHuaweiId signInHuaweiId = HuaweiIdAuthManager.GetExtendedAuthResult(hiHealthOptions);
            MyDataController = HuaweiHiHealth.GetDataController(Android.App.Application.Context, signInHuaweiId);
        }
        public async Task<List<HealthData>> GetHealthData()
        {
            // Create Tasks to query the different Types of Data
            var StepsData=  QueryStepsData();
            var DistanceData = QueryDistanceData();
            var CalroiesData = QueryCalroiesData();
            var HeartRateData = QueryHeartRateData();

            await System.Threading.Tasks.Task.WhenAll(StepsData, DistanceData, CalroiesData, HeartRateData);

            return fitnessActivities;
        }
        public async Task<bool> QueryStepsData()
        {
            // Initialization start and end time, The first four digits of the shaping data represent the year,
            int intendTime = Int32.Parse(endDate.ToString(DateFormat));
            int intstartTime = Int32.Parse(startDate.ToString(DateFormat));

            // Use the specified data type, start and end time to call the data controller to query the summary data of this data type of the daily
            var DaliySummationTask = MyDataController.ReadDailySummationAsync(DataType.DtContinuousStepsDelta, intstartTime, intendTime);

            try
            {
                await DaliySummationTask;

                if (DaliySummationTask.IsCompleted && DaliySummationTask.Result != null)
                {
                    SampleSet result = DaliySummationTask.Result;
                    if (DaliySummationTask.Exception == null)
                    {
                       foreach (SamplePoint samplePoint in result.SamplePoints)
                        {
                            Logger("Sample point type: " + samplePoint.DataType.Name);
                            string millisecondsPoint = samplePoint.GetStartTime(TimeUnit.Milliseconds).ToString();
                            int datekey = Int32.Parse((GetDateFromMilliseconds(millisecondsPoint)).ToString(DateFormat));
                            foreach (Field field in samplePoint.DataType.Fields)
                            {
                                int steps = samplePoint.GetFieldValue(field).AsIntValue();
                                if (fitnessActivities.Exists(x => x.FormatedDate == datekey))
                                    fitnessActivities.Find(x => x.FormatedDate == datekey).Steps = steps;
                                Logger("Field: " + field.Name + " Value: " + samplePoint.GetFieldValue(field));
                            }
                        }
                    }
                    else
                    {
                        Logger(DaliySummationTask.Exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger(DaliySummationTask.Exception.Message);
            }
            return true;
        }
        public async Task<bool> QueryDistanceData()
        {
            // Initialization start and end time, The first four digits of the shaping data represent the year,
            int intendTime = Int32.Parse(endDate.ToString(DateFormat));
            int intstartTime = Int32.Parse(startDate.ToString(DateFormat));

            // Use the specified data type, start and end time to call the data controller to query the summary data of this data type of the daily
            var DaliySummationTask = MyDataController.ReadDailySummationAsync(DataType.DtContinuousDistanceDelta, intstartTime, intendTime);

            try
            {
                await DaliySummationTask;

                if (DaliySummationTask.IsCompleted && DaliySummationTask.Result != null)
                {
                    SampleSet result = DaliySummationTask.Result;
                    if (DaliySummationTask.Exception == null)
                    {
                        foreach (SamplePoint samplePoint in result.SamplePoints)
                        {
                            Logger("Sample point type: " + samplePoint.DataType.Name);
                            string millisecondsPoint = samplePoint.GetStartTime(TimeUnit.Milliseconds).ToString();
                            int datekey = Int32.Parse((GetDateFromMilliseconds(millisecondsPoint)).ToString(DateFormat));
                            foreach (Field field in samplePoint.DataType.Fields)
                            {
                                float distance = samplePoint.GetFieldValue(field).AsFloatValue();
                                if (fitnessActivities.Exists(x => x.FormatedDate == datekey))
                                    fitnessActivities.Find(x => x.FormatedDate == datekey).Distance = distance;
                                Logger("Field: " + field.Name + " Value: " + samplePoint.GetFieldValue(field));
                            }
                        }
                    }
                    else
                    {
                        Logger(DaliySummationTask.Exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger(DaliySummationTask.Exception.Message);
            }
            return true;
        }
        public async Task<bool> QueryCalroiesData()
        {
            // Initialization start and end time, The first four digits of the shaping data represent the year,
            int intendTime = Int32.Parse(endDate.ToString(DateFormat));
            int intstartTime = Int32.Parse(startDate.ToString(DateFormat));

            // Use the specified data type, start and end time to call the data controller to query the summary data of this data type of the daily
            var DaliySummationTask = MyDataController.ReadDailySummationAsync(DataType.DtContinuousCaloriesBurnt, intstartTime, intendTime);

            try
            {
                await DaliySummationTask;

                if (DaliySummationTask.IsCompleted && DaliySummationTask.Result != null)
                {
                    SampleSet result = DaliySummationTask.Result;
                    if (DaliySummationTask.Exception == null)
                    {
                        foreach (SamplePoint samplePoint in result.SamplePoints)
                        {
                            Logger("Sample point type: " + samplePoint.DataType.Name);
                            string millisecondsPoint = samplePoint.GetStartTime(TimeUnit.Milliseconds).ToString();
                            int datekey = Int32.Parse((GetDateFromMilliseconds(millisecondsPoint)).ToString(DateFormat));
                            foreach (Field field in samplePoint.DataType.Fields)
                            {
                                double calroies = samplePoint.GetFieldValue(field).AsDoubleValue();
                                if (fitnessActivities.Exists(x => x.FormatedDate == datekey))
                                    fitnessActivities.Find(x => x.FormatedDate == datekey).Calories = calroies;
                                Logger("Field: " + field.Name + " Value: " + samplePoint.GetFieldValue(field));
                            }
                        }
                    }
                    else
                    {
                        Logger(DaliySummationTask.Exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger(DaliySummationTask.Exception.Message);
            }
            return true;
        }
        public async Task<bool> QueryHeartRateData()
        {
            // Initialization start and end time, The first four digits of the shaping data represent the year,
            int intendTime = Int32.Parse(endDate.ToString(DateFormat));
            int intstartTime = Int32.Parse(startDate.ToString(DateFormat));

            // Use the specified data type, start and end time to call the data controller to query the summary data of this data type of the daily
            var DaliySummationTask = MyDataController.ReadDailySummationAsync(DataType.DtInstantaneousHeartRate, intstartTime, intendTime);

            try
            {
                await DaliySummationTask;

                if (DaliySummationTask.IsCompleted && DaliySummationTask.Result != null)
                {
                    SampleSet result = DaliySummationTask.Result;
                    if (DaliySummationTask.Exception == null)
                    {
                        foreach (SamplePoint samplePoint in result.SamplePoints)
                        {
                            Logger("Sample point type: " + samplePoint.DataType.Name);
                            string millisecondsPoint = samplePoint.GetStartTime(TimeUnit.Milliseconds).ToString();
                            int datekey = Int32.Parse((GetDateFromMilliseconds(millisecondsPoint)).ToString(DateFormat));
                            float HR = samplePoint.GetFieldValue(Field.FieldAvg).AsFloatValue();
                            if (fitnessActivities.Exists(x => x.FormatedDate == datekey))
                                fitnessActivities.Find(x => x.FormatedDate == datekey).HeartBeat = Int32.Parse(HR.ToString());
                            Logger("Field: " + Field.FieldAvg.Name + " Value: " + samplePoint.GetFieldValue(Field.FieldAvg));
                        }
                    }
                    else
                    {
                        Logger(DaliySummationTask.Exception.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger(DaliySummationTask.Exception.Message);
            }
            return true;
        }

        private void SetDatesRange()
        {
            startDate = DateTime.Now.AddDays(lastSevenDays);

            endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        }

        private DateTime GetDateFromMilliseconds(string milliseconds)
        {
            double ticks = double.Parse(milliseconds);
            TimeSpan time = TimeSpan.FromMilliseconds(ticks);
            DateTime date = new DateTime(1970, 1, 1) + time;
            return date;
        }
        public void Logger(string str)
        {
            Log.Info(TAG, str);
        }

    }
}