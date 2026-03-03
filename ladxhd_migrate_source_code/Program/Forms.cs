namespace LADXHD_Migrater
{
    internal class Forms
    {
        public static Form_MainForm  MainDialog;
        public static Form_OkayForm  OkayDialog; 
        public static Form_YesNoForm YesNoDialog; 

        public static void Initialize()
        {
            MainDialog  = new Form_MainForm();
            OkayDialog  = new Form_OkayForm();
            YesNoDialog = new Form_YesNoForm();
        }
    }
}
