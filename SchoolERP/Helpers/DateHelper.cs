namespace SchoolERP.Helpers
{
    public static class DateHelper
    {
        public static int CalculateAge(DateTime dob)
        {
            return DateTime.Now.Year - dob.Year;
        }

        //public static string GetDaySuffix(int day)
        //{
        //    if (day >= 11 && day <= 13)
        //        return "th";

        //    return day % 10 switch
        //    {
        //        1 => "st",
        //        2 => "nd",
        //        3 => "rd",
        //        _ => "th"
        //    };
        //}

        public static string GetDaySuffix(int day)
        {
            int d = day; // force int locally

            if (d >= 11 && d <= 13)
                return "th";

            return (d % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

    }
}
