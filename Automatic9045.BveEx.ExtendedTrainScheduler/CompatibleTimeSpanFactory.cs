using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BveEx.Extensions.MapStatements;

namespace Automatic9045.BveEx.ExtendedTrainScheduler
{
    /// <summary>
    /// BVE 本体互換の <see cref="TimeSpan"/> 生成ロジックを提供します。
    /// </summary>
    internal static class CompatibleTimeSpanFactory
    {
        /// <summary>
        /// 指定した秒数を表す <see cref="TimeSpan"/> を作成します。
        /// </summary>
        /// <remarks>
        /// 時間はミリ秒単位で切り捨てられます。
        /// </remarks>
        public static TimeSpan FromSeconds(double value)
        {
            double milliseconds = value * 1000;
            return TimeSpan.FromMilliseconds((int)milliseconds);
        }

        public static TimeSpan Parse(object obj)
        {
            if (obj is string text)
            {
                string[] texts = text.Split(':');

                int seconds = 0;
                switch (texts.Length)
                {
                    case 2:
                        seconds = int.Parse(texts[0], CultureInfo.InvariantCulture) * 3600 + int.Parse(texts[1], CultureInfo.InvariantCulture) * 60;
                        break;

                    case 3:
                        seconds = int.Parse(texts[0], CultureInfo.InvariantCulture) * 3600 + int.Parse(texts[1], CultureInfo.InvariantCulture) * 60 + int.Parse(texts[2], CultureInfo.InvariantCulture);
                        break;

                    default:
                        throw new SyntaxException();
                }

                return FromSeconds(seconds);
            }
            else
            {
                return FromSeconds(Convert.ToDouble(obj, CultureInfo.InvariantCulture));
            }
        }
    }
}
