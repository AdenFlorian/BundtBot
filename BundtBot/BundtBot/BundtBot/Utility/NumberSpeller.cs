using System;
using System.Text;

namespace BundtBot.BundtBot.Utility {
    /// <summary>
    /// Adam Robinson
    /// http://stackoverflow.com/questions/2600746/print-value-of-number-int-spelled-out
    /// </summary>
    public static class NumberSpeller {
        const long Quadrillion = Trillion*1000;
        const long Trillion = Billion*1000;
        const long Billion = Million*1000;
        const long Million = Thousand*1000;
        const long Thousand = Hundred*10;
        const long Hundred = 100;

        public static string ToVerbal(this int value) {
            return ToVerbal((long) value);
        }

        public static string ToVerbal(this long value) {
            if (value == 0) return "zero";

            if (value < 0) {
                return "negative " + ToVerbal(Math.Abs(value));
            }

            var builder = new StringBuilder();

            int unit;

            if (value >= Quadrillion) {
                unit = (int) (value/Quadrillion);
                value -= unit*Quadrillion;

                builder.Append($"{(builder.Length > 0 ? ", " : string.Empty)}{ToVerbal(unit)} quadrillion");
            }

            if (value >= Trillion) {
                unit = (int) (value/Trillion);
                value -= unit*Trillion;

                builder.Append($"{(builder.Length > 0 ? ", " : string.Empty)}{ToVerbal(unit)} trillion");
            }

            if (value >= Billion) {
                unit = (int) (value/Billion);
                value -= unit*Billion;

                builder.Append($"{(builder.Length > 0 ? ", " : string.Empty)}{ToVerbal(unit)} billion");
            }

            if (value >= Million) {
                unit = (int) (value/Million);
                value -= unit*Million;

                builder.Append($"{(builder.Length > 0 ? ", " : string.Empty)}{ToVerbal(unit)} million");
            }

            if (value >= Thousand) {
                unit = (int) (value/Thousand);
                value -= unit*Thousand;

                builder.Append($"{(builder.Length > 0 ? ", " : string.Empty)}{ToVerbal(unit)} thousand");
            }

            if (value >= Hundred) {
                unit = (int) (value/Hundred);
                value -= unit*Hundred;

                builder.Append($"{(builder.Length > 0 ? ", " : string.Empty)}{ToVerbal(unit)} hundred");
            }

            if (builder.Length > 0 && value > 0) builder.AppendFormat(" and");

            if (value >= 90) {
                value -= 90;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}ninety");
            }

            if (value >= 80) {
                value -= 80;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}eighty");
            }

            if (value >= 70) {
                value -= 70;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}seventy");
            }

            if (value >= 60) {
                value -= 60;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}sixty");
            }

            if (value >= 50) {
                value -= 50;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}fifty");
            }

            if (value >= 40) {
                value -= 40;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}forty");
            }

            if (value >= 30) {
                value -= 30;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}thirty");
            }

            if (value >= 20) {
                value -= 20;

                builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}twenty");
            }

            if (value == 19) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}nineteen");
            if (value == 18) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}eighteen");
            if (value == 17) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}seventeen");
            if (value == 16) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}sixteen");
            if (value == 15) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}fifteen");
            if (value == 14) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}fourteen");
            if (value == 13) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}thirteen");
            if (value == 12) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}twelve");
            if (value == 11) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}eleven");
            if (value == 10) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}ten");
            if (value == 9) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}nine");
            if (value == 8) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}eight");
            if (value == 7) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}seven");
            if (value == 6) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}six");
            if (value == 5) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}five");
            if (value == 4) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}four");
            if (value == 3) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}three");
            if (value == 2) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}two");
            if (value == 1) builder.Append($"{(builder.Length > 0 ? " " : string.Empty)}one");

            return builder.ToString();
        }
    }
}