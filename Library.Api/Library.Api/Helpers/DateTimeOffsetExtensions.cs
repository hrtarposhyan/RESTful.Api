﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Helpers
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset,
            DateTimeOffset? dateOfDeath)
        {
            //var currentDate = DateTime.UtcNow;
            //int age = currentDate.Year - dateTimeOffset.Year;

            //if (currentDate < dateTimeOffset.AddYears(age))
            //{
            //    age--;
            //} 
            //return age;
            var dateToCalculateTo = DateTime.UtcNow;

            if (dateOfDeath != null)
            {
                dateToCalculateTo = dateOfDeath.Value.UtcDateTime;
            }

            int age = dateToCalculateTo.Year - dateTimeOffset.Year;

            if (dateToCalculateTo < dateTimeOffset.AddYears(age))
            {
                age--;
            }
            return age;
        }
    }
}
