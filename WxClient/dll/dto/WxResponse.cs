using System.Collections.Generic;

namespace WxClient.dll.Dto
{
    public class WxResponse
    {
        public int Error { get; set; }
        public int Type { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public TValue GetValueOrDefault<TValue>(
            string key,
            TValue defaultValue)
        {
            return (TValue) (Data.TryGetValue(key, out var value) ? value : defaultValue);
        }
    }
}