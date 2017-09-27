using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoonApiLib
{
    public class RoonApiSubscriptionHandler
    {
        internal class RoonSubscription
        {
            [JsonProperty("subscription_key")]
            public int SubscriptionKey { get; set; }
        }
        Dictionary<int, int> _subscriptions = new Dictionary<int, int>();

        internal bool AddSubscription(string body, int requestId)
        {
            RoonSubscription subscription = JsonConvert.DeserializeObject<RoonSubscription>(body);
            return AddSubscription(subscription.SubscriptionKey, requestId);
        }
        internal bool AddSubscription(int key, int requestId)
        {
            lock (_subscriptions)
            {
                if (_subscriptions.ContainsKey(key))
                    return false;
                _subscriptions.Add(key, requestId);
                return true;
            }
        }
        internal bool RemoveSubscription(string body)
        {
            RoonSubscription subscription = JsonConvert.DeserializeObject<RoonSubscription>(body);
            return RemoveSubscription(subscription.SubscriptionKey);
        }
        internal bool RemoveSubscription(int key)
        {
            lock (_subscriptions)
            {
                if (!_subscriptions.ContainsKey(key))
                    return false;
                _subscriptions.Remove(key);
                return true;
            }
        }
        public async Task<int> ReplyAll(RoonApi api, string command, string body = null, string contentType = "application/json")
        {
            int count = 0;
            foreach (var subscription in _subscriptions)
            {
                if (await api.Reply(command, subscription.Value, true, body, false, contentType))
                    count++;
            }
            return count;
        }
    }
}
