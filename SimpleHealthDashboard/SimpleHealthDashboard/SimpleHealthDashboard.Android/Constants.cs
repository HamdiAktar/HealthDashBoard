namespace SimpleHealthDashboard.Droid
{
    class Constants
    {
        //Request code for displaying the sign in authorization screen using the startActivityForResult method.
        public static int RequestSignInLogin = 1002;
        //Request code for displaying the HUAWEI Health authorization screen using the startActivityForResult method.
        public static int RequestHealthAuth = 1003;
        //Scheme of Huawei Health Authorization Activity
        public static string HEALTH_APP_SETTING_DATA_SHARE_HEALTHKIT_ACTIVITY_SCHEME = "huaweischeme://healthapp/achievement?module=kit";

    }
}