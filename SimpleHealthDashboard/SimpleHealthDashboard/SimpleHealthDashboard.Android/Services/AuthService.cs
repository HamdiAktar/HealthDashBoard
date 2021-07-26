using Android.Content;
using System;
using SimpleHealthDashboard.Services;
using Huawei.Hms.Hihealth;
using Huawei.Hms.Support.Hwid.Service;
using Android.Util;
using Huawei.Hms.Support.Hwid;
using Huawei.Hms.Support.Hwid.Result;
using System.Collections.Generic;
using Huawei.Hms.Support.Api.Entity.Auth;
using Huawei.Hms.Hihealth.Data;
using Huawei.Hms.Support.Hwid.Request;
using System.Threading.Tasks;
using Xamarin.Forms;
using SimpleHealthDashboard.Droid.Services;

[assembly: Dependency(typeof(AuthService))]
namespace SimpleHealthDashboard.Droid.Services
{
    class AuthService : IAuth
    {

        private static string TAG = "AuthClient";

        // HUAWEI Health kit SettingController
        private SettingController MySettingController;

        public static IHuaweiIdAuthService AuthticationService;

        public AuthService()
        {
            InitService();
        }

        private void InitService()
        {

            Log.Info(TAG, "HiHealthKitClient connect to service");
            // Initialize SettingController
            HiHealthOptions fitnessOptions = HiHealthOptions.HiHealthOptionsBulider().Build();
            AuthHuaweiId signInHuaweiId = HuaweiIdAuthManager.GetExtendedAuthResult(fitnessOptions);
            MySettingController = HuaweiHiHealth.GetSettingController(Android.App.Application.Context, signInHuaweiId);
        }

        //Sign-in and authorization method.
        //The authorization screen will display up if authorization has not granted by the current account.
        public async void SignIn()
        {
            IList<Scope> scopeList = new List<Scope>();
           
            // View and save steps in HUAWEI Health Kit.
            scopeList.Add(new Scope(Scopes.HealthkitStepRead));

            // View and save distance data in HUAWEI Health Kit.
            scopeList.Add(new Scope(Scopes.HealthkitDistanceRead));

            // View and save the heart rate data in HUAWEI Health Kit.
            scopeList.Add(new Scope(Scopes.HealthkitHeartrateRead));

            // View and save calories data in HUAWEI Health Kit.
            scopeList.Add(new Scope(Scopes.HealthkitCaloriesRead));

            // Configure authorization parameters.
            HuaweiIdAuthParamsHelper AuthParamsHelper = new HuaweiIdAuthParamsHelper(HuaweiIdAuthParams.DefaultAuthRequestParam);
            HuaweiIdAuthParams AuthParams = AuthParamsHelper.SetIdToken().SetAccessToken().SetScopeList(scopeList).CreateParams();

            // Initialize the HuaweiIdAuthService object.
            AuthticationService = HuaweiIdAuthManager.GetService(Android.App.Application.Context, AuthParams);

            // Silent sign-in. If authorization has been granted by the current account,
            // the authorization screen will not display.
            var AuthHuaweiIdTask = AuthticationService.SilentSignInAsync();

            try
            {
                await AuthHuaweiIdTask;

                if (AuthHuaweiIdTask.IsCompleted && AuthHuaweiIdTask.Result != null)
                {
                    if (AuthHuaweiIdTask.Exception == null)
                    {

                        Log.Debug(TAG, "SilentSignIn success");

                        // anfter Huawei ID authorization, perform Huawei Health authorization.
                        CheckOrAuthorizeHealth();
                    }
                    else
                    {
                        // The silent sign-in fails.
                        // This indicates that the authorization has not been granted by the current account.
                        Log.Error(TAG, "Sign failed status:" + AuthHuaweiIdTask.Exception.Message);

                        // Call the sign-in API using the SignInIntent.
                        Intent signInIntent = AuthticationService.SignInIntent;

                        // Display the authorization screen by using the startActivityForResult() method of the activity.
                        MainActivity.Instance.StartActivityForResult(signInIntent, Constants.RequestSignInLogin);

                        MainActivity.Instance.SigninTaskCompletionSource = new TaskCompletionSource<Intent>();
                        await MainActivity.Instance.SigninTaskCompletionSource.Task;

                        HandleSignInResult(MainActivity.Instance.SigninTaskCompletionSource.Task.Result);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(TAG, "Sign failed :" + ex.Message);

                // Call the sign-in API using the SignInIntent.
                Intent signInIntent = AuthticationService.SignInIntent;

                // Display the authorization screen by using the startActivityForResult() method of the activity.
                MainActivity.Instance.StartActivityForResult(signInIntent, Constants.RequestSignInLogin);

                MainActivity.Instance.SigninTaskCompletionSource = new TaskCompletionSource<Intent>();
                await MainActivity.Instance.SigninTaskCompletionSource.Task;

                HandleSignInResult(MainActivity.Instance.SigninTaskCompletionSource.Task.Result);
            }

        }

        public void HandleSignInResult(Intent data)
        {
            // Obtain the authorization response from the intent.
            HuaweiIdAuthResult result = HuaweiIdAuthAPIManager.HuaweiIdAuthAPIService.ParseHuaweiIdFromIntent(data);
            if (result != null)
            {
                if (result.IsSuccess)
                {
                    Log.Debug(TAG, "Sign in is success");
                    // after Huawei ID authorization, perform Huawei Health authorization.
                    CheckOrAuthorizeHealth();
                }
            }
        }

        //Check HUAWEI Health authorization status.
        //if not, start HUAWEI Health authorization Activity for user authorization.
        public async void CheckOrAuthorizeHealth()
        {
            // Calling SettingController to query HUAWEI Health authorization status.
            var AuthTask = MySettingController.GetHealthAppAuthorizationAsync();
            try
            {
                await AuthTask;

                if (AuthTask.IsCompleted)
                {
                    if (AuthTask.Exception == null)
                    {
                        if (AuthTask.Result)
                            Log.Debug(TAG, "CheckOrAuthorizeHealth get result success");
                        else
                        {
                            // If not, start HUAWEI Health authorization Activity by schema with User-defined requestCode.
                            Android.Net.Uri healthKitSchemaUri = Android.Net.Uri.Parse(Constants.HEALTH_APP_SETTING_DATA_SHARE_HEALTHKIT_ACTIVITY_SCHEME);
                            Intent intent = new Intent(Intent.ActionView, healthKitSchemaUri);
                            // Before start, Determine whether the HUAWEI health authorization Activity can be opened.
                            if (intent.ResolveActivity(MainActivity.Instance.PackageManager) != null)
                            {
                                MainActivity.Instance.StartActivityForResult(intent, Constants.RequestHealthAuth);

                                MainActivity.Instance.AuthTaskCompletionSource = new TaskCompletionSource<bool>();
                                await MainActivity.Instance.AuthTaskCompletionSource.Task;

                                QueryHealthAuthorization();
                            }
                            else
                                Log.Error(TAG, "CheckOrAuthorizeHealth has failed");
                        }
                    }
                    else
                        Log.Error(TAG, "CheckOrAuthorizeHealth has exception" + AuthTask.Exception.Message);
                }

            }
            catch (Exception ex)
            {
                Log.Error(TAG, "CheckOrAuthorizeHealth has exception" + ex.Message);
            }
        }


        //Query Huawei Health authorization result.
        private async void QueryHealthAuthorization()
        {
            // Calling SettingController to query HUAWEI Health authorization status.
            var QueryTask = MySettingController.GetHealthAppAuthorizationAsync();
            try
            {
                await QueryTask;

                if (QueryTask.IsCompleted)
                {
                    if (QueryTask.Exception == null)
                    {
                        if (QueryTask.Result)
                            Log.Debug(TAG, "CheckOrAuthorizeHealth get result success");
                        else
                            Log.Error(TAG, "QueryHealthAuthorization has failed");
                    }
                    else
                        Log.Error(TAG, "QueryHealthAuthorization has exception" + QueryTask.Exception.Message);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, "QueryHealthAuthorization has exception" + ex.Message);
            }
        }

    }
}