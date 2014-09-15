using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Vindinium
{
    internal interface Either<in TLeft, in TRight>
    {
        object Value { get; }
    }

    internal sealed class Left<TLeft> : Either<TLeft, object>
    {
        private readonly TLeft _value;

        public Left(TLeft left)
        {
            this._value = left;
        }

        public object Value { get { return (object)_value; } }
    }


    internal sealed class Right<TRight> : Either<object, TRight>
    {
        private readonly TRight _value;

        public Right(TRight right)
        {
            this._value = right;
        }

        public object Value { get { return (object)_value; } }
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
                        throw new ArgumentException("Key [" + key + "] null in message [" + inp + "]", "key");
                    }
                    else
                    {
                        return outp.Value<T>();
                    }
                }
                else
                {
                    throw new ArgumentException("Key [" + key + "] absent from message [" + inp + "]", "key");
                }
            }
            else
            {
                throw new ArgumentNullException("No dictionary specified", "inp");
            }
        }

    }
}

