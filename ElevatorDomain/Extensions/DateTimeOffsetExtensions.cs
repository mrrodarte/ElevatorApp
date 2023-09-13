using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElevatorDomain.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToFormattedString(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToFileTime().ToString("MM/dd/yyyy hh:mm:ss tt");
        }
    }
}