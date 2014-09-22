namespace Vindinium.Util
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    internal interface IEither<in TLeft, in TRight>
    {
        object Value { get; }
    }

    internal static class Util
    {
        internal static Tuple<int, int> JObject2TupleIntInt(IDictionary<string, JToken> inp)
        {
            var x = JToken2T<int>(inp, "x");
            var y = JToken2T<int>(inp, "y");
            return new Tuple<int, int>(x, y);
        }

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
                throw new ArgumentNullException("inp", "No dictionary specified");
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
                throw new ArgumentNullException("inp", "No dictionary specified"); 
            }
        }
    }

    internal sealed class Left<TLeft, TRight> : IEither<TLeft, TRight>
    {
        private readonly TLeft value;

        public Left(TLeft left)
        {
            this.value = left;
        }

        public object Value
        {
            get { return (object)this.value; }
        }
    }

    internal sealed class Right<TLeft, TRight> : IEither<TLeft, TRight>
    {
        private readonly TRight value;

        public Right(TRight right)
        {
            this.value = right;
        }

        public object Value
        {
            get { return (object)this.value; }
        }
    }
}