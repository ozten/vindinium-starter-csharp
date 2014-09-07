using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Vindinium
{
    internal static class Util
    {

        internal static T? JToken2NullableT<T>(IDictionary<string, JToken> inp, string key) where T : struct
        {
            if (inp != null)
            {
                if (inp.Keys.Contains(key))
                {
                    var outp = inp[key];
                    if (outp == null)
                    {
                        return null;
                    }
                    else
                    {
                        return outp.Value<T>();
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }


        }

        internal static T JToken2T<T>(IDictionary<string, JToken> inp, string key) where T : class
        {
            if (inp != null)
            {
                if (inp.Keys.Contains(key))
                {
                    var outp = inp[key];
                    if (outp == null)
                    {
                        return null;
                    }
                    else
                    {
                        return outp.Value<T>();
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}

