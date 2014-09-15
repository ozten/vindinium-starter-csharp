using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Vindinium
{
    // TODO a direction enum so we don't have to care about the default value of T and U
    internal sealed class Either<T, U>  where T : class where U : class {
        internal T Left { get; private set; }
        internal U Right { get; private set; }
        internal Either(T t)
        {
            this.Left = t;
        }

        internal Either(U u)
        {
            this.Right = u;
        }

        internal object GetValue()
        {
            if (this.Left == null)
            {
                return this.Right;
            }
            else
            {
                return this.Left;
            }
        }
    }

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
                throw new ArgumentNullException("No dictionary specified", "inp");
            }
        }

        internal static T JToken2T<T>(IDictionary<string, JToken> inp, string key)
        {
            if (inp != null)
            {
                if (inp.Keys.Contains(key))
                {
                    var outp = inp[key];
                    if (outp == null)
                    {
                        throw new ArgumentException("Key ["+key+"] null in message ["+inp+"]", "key");
                    }
                    else
                    {
                        return outp.Value<T>();
                    }
                }
                else
                {
                    throw new ArgumentException("Key ["+key+"] absent from message ["+inp+"]", "key");
                }
            }
            else
            {
                throw new ArgumentNullException("No dictionary specified", "inp");
            }
        }

    }
}

