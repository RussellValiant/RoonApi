using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RoonApiLib.Helper;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoonApiLib
{
    public class RoonApiTransport : BindableBase
    {
        public enum EState
        {
            unknown = 0,
            stopped, playing, paused, loading
        }
        public enum ELoop
        {
            unknown = 0,
            disabled, loop, loop_one, next
        }
        public enum EControl
        {
            stop = 0,
            play, pause, playpause, previous, next
        }
        public enum EVolumeMode
        {
            absolute = 0,
            relative, relative_step
        }
        public enum EMute
        {
            mute = 0,
            unmute
        }
        public enum ESeek
        {
            absolute = 0,
            relative
        }
        public enum ESourceControlStatus
        {
            indeterminate = 0,
            selected, deselected, standby
        }
        public class Volume
        {
            internal void Copy (Volume src)
            {
                Type = src.Type;
                Min = src.Min;
                Max = src.Max;
                HardLimitMin = src.HardLimitMin;
                HardLimitMax = src.HardLimitMax;
                Value = src.Value;
                Step = src.Step;
                IsMuted = src.IsMuted;
            }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("min")]
            public int Min { get; set; }
            [JsonProperty("max")]
            public int Max { get; set; }
            [JsonProperty("hard_limit_min")]
            public int HardLimitMin { get; set; }
            [JsonProperty("hard_limit_max")]
            public int HardLimitMax { get; set; }
            [JsonProperty("value")]
            public int Value { get; set; }
            [JsonProperty("step")]
            public int Step { get; set; }
            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }
        }
        public class Settings
        {
            internal void Copy(Settings src)
            {
                Loop = src.Loop;
                Shuffle = src.Shuffle;
                AutoRadio = src.AutoRadio;
            }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("loop")]
            public ELoop Loop { get; set; }
            [JsonProperty("shuffle")]
            public bool Shuffle { get; set; }
            [JsonProperty("auto_radio")]
            public bool AutoRadio { get; set; }
        }
        public class LineOne
        {
            internal void Copy(LineOne src)
            {
                Line1 = src.Line1;
            }
            [JsonProperty("line1")]
            public string Line1 { get; set; }
        }
        public class LineTwo : LineOne
        {
            internal void Copy(LineTwo src)
            {
                base.Copy(src);
                Line2 = src.Line2;
            }
            [JsonProperty("line2")]
            public string Line2 { get; set; }
        }
        public class LineThree : LineTwo
        {
            internal void Copy(LineThree src)
            {
                base.Copy(src);
                Line3 = src.Line3;
            }
            [JsonProperty("line3")]
            public string Line3 { get; set; }
        }
        public class NowPlaying
        {
            internal void Copy(NowPlaying src)
            {
                SeekPosition = src.SeekPosition;
                Length = src.Length;
                if (OneLine == null || src.OneLine == null)
                    OneLine = src.OneLine;
                else
                    OneLine.Copy(src.OneLine);
                if (TwoLine == null || src.TwoLine == null)
                    TwoLine = src.TwoLine;
                else
                    TwoLine.Copy(src.TwoLine);
                if (ThreeLine == null || src.ThreeLine == null)
                    ThreeLine = src.ThreeLine;
                else
                    ThreeLine.Copy(src.ThreeLine);

                ImageKey = src.ImageKey;
                ArtistImageKeys = src.ArtistImageKeys;
            }
            [JsonProperty("seek_position")]
            public int? SeekPosition { get; set; }
            [JsonProperty("length")]
            public int Length { get; set; }
            [JsonProperty("one_line")]
            public LineOne OneLine { get; set; }
            [JsonProperty("two_line")]
            public LineTwo TwoLine { get; set; }
            [JsonProperty("three_line")]
            public LineThree ThreeLine { get; set; }
            [JsonProperty("image_key")]
            public string ImageKey { get; set; }
            [JsonProperty("artist_image_keys")]
            public string[] ArtistImageKeys { get; set; }
        }
        public class SourceControl
        {
            internal void Copy(SourceControl src)
            {
                ControlKey = src.ControlKey;
                DisplayName = src.DisplayName;
                Status = src.Status;
                SupportsStandby = src.SupportsStandby;
            }
            [JsonProperty("control_key")]
            public int ControlKey { get; set; }
            [JsonProperty("display_name")]
            public string DisplayName { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("status")]
            public ESourceControlStatus Status { get; set; }
            [JsonProperty("supports_standby")]
            public bool SupportsStandby { get; set; }
        }
        public class Output
        {
            internal void Copy (Output src)
            {
                OutputId = src.OutputId;
                ZoneId = src.ZoneId;
                CanGroupWithOuputIds = src.CanGroupWithOuputIds;
                DisplayName = src.DisplayName;
                State = src.State;
                ZoneId = src.ZoneId;
                if (Volume == null || src.Volume == null)
                    Volume = src.Volume;
                else
                    Volume.Copy(src.Volume);
                if (SourceControls == null || src.SourceControls == null || SourceControls.Length != src.SourceControls.Length)
                    SourceControls = src.SourceControls;
                else
                {
                    for (int i = 0; i < SourceControls.Length; i++)
                        SourceControls[i].Copy(src.SourceControls[i]);
                }
            }

            [JsonProperty("output_id")]
            public string OutputId { get; set; }
            [JsonProperty("zone_id")]
            public string ZoneId { get; set; }
            [JsonProperty("can_group_with_output_ids")]
            public string[] CanGroupWithOuputIds { get; set; }
            [JsonProperty("display_name")]
            public string DisplayName { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("state")]
            public EState State { get; set; }
            [JsonProperty("volume")]
            public Volume Volume { get; set; }
            [JsonProperty("source_controls")]
            public SourceControl[] SourceControls { get; set; }         // never seen
        }
        public class Zone
        {
            internal void Copy(Zone src)
            {
                ZoneId = src.ZoneId;
                DisplayName = src.DisplayName;
                if (Outputs == null || src.Outputs == null || Outputs.Length != src.Outputs.Length)
                    Outputs = src.Outputs;
                else
                {
                    for (int i = 0; i < Outputs.Length; i++)
                        Outputs[i].Copy(src.Outputs[i]);
                }
                State = src.State;
                IsNextAllowed = src.IsNextAllowed;
                IsPreviousAllowed = src.IsPreviousAllowed;
                IsPauseAllowed = src.IsPauseAllowed;
                IsPlayAllowed = src.IsPlayAllowed;
                IsSeekAllowed = src.IsSeekAllowed;
                QueueItemsRemaining = src.QueueItemsRemaining;
                QueueTimeRemaining = src.QueueTimeRemaining;
                if (Settings == null || src.Settings == null)
                    Settings = src.Settings;
                else
                    Settings.Copy(src.Settings);
                if (NowPlaying == null || src.NowPlaying == null)
                    NowPlaying = src.NowPlaying;
                else
                    NowPlaying.Copy(src.NowPlaying);
            }

            [JsonProperty("zone_id")]
            public string ZoneId { get; set; }
            [JsonProperty("display_name")]
            public string DisplayName { get; set; }
            [JsonProperty("outputs")]
            public Output[] Outputs { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("state")]
            public EState State { get; set; }
            [JsonProperty("is_next_allowed")]
            public bool IsNextAllowed { get; set; }
            [JsonProperty("is_previous_allowed")]
            public bool IsPreviousAllowed { get; set; }
            [JsonProperty("is_pause_allowed")]
            public bool IsPauseAllowed { get; set; }
            [JsonProperty("is_play_allowed")]
            public bool IsPlayAllowed { get; set; }
            [JsonProperty("is_seek_allowed")]
            public bool IsSeekAllowed { get; set; }
            [JsonProperty("queue_items_remaining")]
            public int QueueItemsRemaining { get; set; }
            [JsonProperty("queue_time_remaining")]
            public int QueueTimeRemaining { get; set; }
            [JsonProperty("settings")]
            public Settings Settings { get; set; }
            [JsonProperty("now_playing")]
            public NowPlaying NowPlaying { get; set; }

            public Output FindOutput(string displayName)
            {
                if (Outputs == null)
                    return null;
                return Outputs.FirstOrDefault((o) => o.DisplayName == displayName);
            }
            public Output FindOutputById(string id)
            {
                if (Outputs == null)
                    return null;
                return Outputs.FirstOrDefault((o) => o.OutputId == id);
            }

        }
        public class ZoneSeek
        {
            internal void Copy(ZoneSeek src)
            {
                ZoneId = src.ZoneId;
                QueueTimeRemaining = src.QueueTimeRemaining;
                SeekPosition = src.SeekPosition;
            }

            [JsonProperty("zone_id")]
            public string ZoneId { get; set; }
            [JsonProperty("queue_time_remaining")]
            public int QueueTimeRemaining { get; set; }
            [JsonProperty("seek_position")]
            public int? SeekPosition { get; set; }
        }
        public class AllZones
        {
            [JsonProperty("zones")]
            public Zone[] Zones { get; set; }
        }
        public class ChangedZoones
        {
            [JsonProperty("zones_changed")]
            public Zone[] ZonesChanged { get; set; }
            [JsonProperty("zones_seek_changed")]
            public ZoneSeek[] ZonesSeekChanged { get; set; }
            [JsonProperty("zones_added")]
            public Zone[] ZonesAdded { get; set; }
            [JsonProperty("zones_removed")]
            public string[] ZonesRemoved { get; set; }
        }
        public class ChangedQueue
        {
            [JsonProperty("items")]
            public QueueItem[] Items { get; set; }
            [JsonProperty("changes")]
            public QueueChange[] Changes { get; set; }
        }
        public partial class QueueChange
        {
            [JsonProperty("operation")]
            public string Operation { get; set; }
            [JsonProperty("index")]
            public int Index { get; set; }
            [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
            public int? Count { get; set; }
            [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
            public QueueItem[] Items { get; set; }
        }
        public class QueueItem
        {
            [JsonProperty("queue_item_id")]
            public int QueueItemId { get; set; }
            [JsonProperty("length")]
            public int Length { get; set; }
            [JsonProperty("image_key")]
            public string ImageKey { get; set; }
            [JsonProperty("one_line")]
            public LineOne OneLine { get; set; }
            [JsonProperty("two_line")]
            public LineTwo TwoLine { get; set; }
            [JsonProperty("three_line")]
            public LineThree ThreeLine { get; set; }
        }
        public class RoonControl
        {
            [JsonProperty("zone_or_output_id")]
            public string ZoneOrOutputId { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("control")]
            public EControl Control { get; set; }
        }
        public class RoonSeek
        {
            [JsonProperty("zone_or_output_id")]
            public string ZoneOrOutputId { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("how")]
            public ESeek How { get; set; }
            [JsonProperty("seconds")]
            public int Seconds { get; set; }
        }
        public class RoonChangeSettings : Settings
        {
            [JsonProperty("zone_or_output_id")]
            public string ZoneOrOutputId { get; set; }
        }
        public class RoonChangeVolume
        {
            [JsonProperty("output_id")]
            public string OutputId { get; set; }
            [JsonProperty("how")]
            [JsonConverter(typeof(StringEnumConverter))]
            public EVolumeMode How { get; set; }
            [JsonProperty("value")]
            public int Value { get; set; }
        }
        public class RoonMute
        {
            [JsonProperty("output_id")]
            public string OutputId { get; set; }
            [JsonProperty("how")]
            [JsonConverter(typeof(StringEnumConverter))]
            public EMute How { get; set; }
        }
        public class RoonSourceControl
        {
            [JsonProperty("output_id")]
            public string OutputId { get; set; }
            [JsonProperty("control_key")]
            public int ControlKey { get; set; }
        }
        public class RoonQueue
        {
            [JsonProperty("zone_or_output_id")]
            public string ZoneOrOutputId { get; set; }
            [JsonProperty("max_item_count")]
            public int MaxItemCount { get; set; }
            [JsonProperty("subscription_key")]
            public int? SubscriptionKey { get; set; }
        }
        public class RoonPlayFromHere
        {
            [JsonProperty("zone_or_output_id")]
            public string ZoneOrOutputId { get; set; }
            [JsonProperty("queue_item_id")]
            public int QueueItemId { get; set; }

        }
        public class RoonUnSubscribe
        {
            [JsonProperty("subscription_key")]
            public int? SubscriptionKey { get; set; }
        }

        RoonApi _api;
        Func<ChangedZoones, Task> _onChangedZones;
        Func<ChangedQueue, Task> _onChangedQueue;

        Dictionary<string, Zone> _zones;
        Dictionary<string, ZoneSeek> _zonesSeek;

        List<ChangedQueue> _zoneQueue;

        public RoonApiTransport(RoonApi api)
        {
            _api = api;
        }
        public Dictionary<string, Zone> Zones
        {
            get => _zones;
        }
        public Dictionary<string, ZoneSeek> ZonesSeek
        {
            get => _zonesSeek;
        }
        public async Task<bool> SubscribeZones(int subscriptionKey, Func<ChangedZoones, Task> onChangedZones)
        {
            _onChangedZones = onChangedZones;
            _zones = new Dictionary<string, Zone>();
            _zonesSeek = new Dictionary<string, ZoneSeek>();
            int requestId = await _api.SendSubscription(RoonApi.ServiceTransport + "/subscribe_zones", subscriptionKey);
            if (requestId < 0)
                return false;
            _api.AddSubscription(requestId, OnReceived);
            return true;
        }
        public async Task<bool> SubscribeQueue(string outputOrZoneId, int maxItemCount, int? subscriptionKey, Func<ChangedQueue, Task> onChangedQueue)
        {
            _onChangedQueue = onChangedQueue;
            var queue = new RoonQueue { ZoneOrOutputId = outputOrZoneId, MaxItemCount = maxItemCount, SubscriptionKey = subscriptionKey };
            int requestId = await _api.SendQueueSubscription<RoonQueue>(RoonApi.ServiceTransport + "/subscribe_queue", queue, 1);
            if (requestId < 0)
                return false;
            
            _api.AddSubscription(requestId, OnQueueReceived);
            return true;
        }
        public async Task<bool> UnSubscribe(string reqName, int? subscriptionKey)
        {
            return await UnSubscribe(reqName , new RoonUnSubscribe { SubscriptionKey = subscriptionKey });
        }
        public async Task<bool> UnSubscribe(string reqName, RoonUnSubscribe unSubscribe)
        {
            var result = await _api.SendReceive<bool, RoonUnSubscribe>(RoonApi.ServiceTransport + "/unsubscribe_" + reqName, unSubscribe);
            return result;
        }
        public async Task<bool> Control(string outputOrZoneId, EControl control)
        {
            return await Control(new RoonControl { ZoneOrOutputId = outputOrZoneId, Control = control });
        }
        public async Task<bool> Control(RoonControl control)
        {
            var result = await _api.SendReceive<bool, RoonControl>(RoonApi.ServiceTransport + "/control", control);
            return result;
        }
        public async Task<bool> ChangeSettings(RoonChangeSettings changeSettings)
        {
            var result = await _api.SendReceive<bool, RoonChangeSettings>(RoonApi.ServiceTransport + "/change_settings", changeSettings);
            return result;
        }
        public async Task<bool> ChangeSettings(string zoneOrOutputId, bool shuffle, bool autoRadio, ELoop loop)
        {
            RoonChangeSettings changeSettings = new RoonChangeSettings
            {
                ZoneOrOutputId = zoneOrOutputId,
                Shuffle = shuffle,
                AutoRadio = autoRadio,
                Loop = loop
            };
            return await ChangeSettings(changeSettings);
        }
        public async Task<bool> ChangeVolume(RoonChangeVolume changeVolume)
        {
            var result = await _api.SendReceive<bool, RoonChangeVolume>(RoonApi.ServiceTransport + "/change_volume", changeVolume);
            return result;
        }
        public async Task<bool> ChangeVolume(string outputId, EVolumeMode mode, int value)
        {
            RoonChangeVolume changeVolume = new RoonChangeVolume { OutputId = outputId, How = mode, Value = value };
            return await ChangeVolume(changeVolume);
        }
        public async Task<bool> Mute(RoonMute mute)
        {
            var result = await _api.SendReceive<bool, RoonMute>(RoonApi.ServiceTransport + "/mute", mute);
            return result;
        }
        public async Task<bool> Mute(string outputId, EMute mute)
        {
            RoonMute setmute = new RoonMute { OutputId = outputId, How = mute };
            return await Mute(setmute);
        }
        public async Task<bool> MuteAll(RoonMute mute)
        {
            var result = await _api.SendReceive<bool, RoonMute>(RoonApi.ServiceTransport + "/mute_all", mute);
            return result;
        }
        public async Task<bool> Seek(RoonSeek seek)
        {
            var result = await _api.SendReceive<bool, RoonSeek>(RoonApi.ServiceTransport + "/seek", seek);
            return result;
        }
        public async Task<bool> Seek(string outputId, ESeek how, int seconds)
        {
            RoonSeek seekControl = new RoonSeek { ZoneOrOutputId = outputId, How = how, Seconds = seconds };
            return await Seek(seekControl);
        }
        public async Task<bool> Standby(RoonSourceControl sourceControl)
        {
            var result = await _api.SendReceive<bool, RoonSourceControl>(RoonApi.ServiceTransport + "/standby", sourceControl);
            return result;
        }
        public async Task<bool> Standby(string outputId, int controlKey)
        {
            RoonSourceControl sourceControl = new RoonSourceControl { OutputId = outputId, ControlKey = controlKey };
            return await Standby(sourceControl);
        }
        public async Task<bool> ToggleStandby(RoonSourceControl sourceControl)
        {
            var result = await _api.SendReceive<bool, RoonSourceControl>(RoonApi.ServiceTransport + "/toggle_standby", sourceControl);
            return result;
        }
        public async Task<bool> ToggleStandby(string outputId, int controlKey)
        {
            RoonSourceControl sourceControl = new RoonSourceControl { OutputId = outputId, ControlKey = controlKey };
            return await ToggleStandby(sourceControl);
        }
        public async Task<bool> PlayFromHere(RoonPlayFromHere playFromHere)
        {
            var result = await _api.SendReceive<bool, RoonPlayFromHere>(RoonApi.ServiceTransport + "/play_from_here", playFromHere);
            return result;
        }
        public async Task<bool> PlayFromHere(string outputId, int queueItemId)
        {
            RoonPlayFromHere playFromHere = new RoonPlayFromHere { ZoneOrOutputId = outputId, QueueItemId = queueItemId };
            return await PlayFromHere(playFromHere);
        }
        async Task<bool> OnReceived(string information, int requestId, string body)
        {
            AllZones zones;
            ChangedZoones changedZones;
            switch (information)
            {
                case "Subscribed":
                    zones = JsonConvert.DeserializeObject<AllZones>(body);
                    changedZones = new ChangedZoones { ZonesChanged = zones.Zones };
                    if (_onChangedZones != null)
                        await _onChangedZones(changedZones);
                    OnReceivedZones(changedZones);
                    break;
                case "UnSubscribed":
                    break;
                case "Changed":
                    changedZones = JsonConvert.DeserializeObject<ChangedZoones>(body);
                    if (_onChangedZones != null)
                        await _onChangedZones(changedZones);
                    OnReceivedZones(changedZones);
                    break;
            }
            return true;
        }
        async Task<bool> OnQueueReceived(string information, int requestId, string body)
        {
            ChangedQueue changedQueue;
            switch (information)
            {
                case "Subscribed":
                    changedQueue = JsonConvert.DeserializeObject<ChangedQueue>(body);
                    if (_onChangedQueue != null)
                        await _onChangedQueue(changedQueue);
                    OnReceivedQueue(changedQueue);
                    break;
                case "UnSubscribed":
					break;
                case "Changed":
                    changedQueue = JsonConvert.DeserializeObject<ChangedQueue>(body);
                    
                    if (_onChangedQueue != null)
                        await _onChangedQueue(changedQueue);
                    OnReceivedQueue(changedQueue);
                    break;
            }
            return true;
        }
        public static Zone FindZone(Zone[] zones, string displayName)
        {
            if (zones == null)
                return null;
            return zones.FirstOrDefault((zone) => zone.DisplayName == displayName);
        }
        public static Zone FindZoneById(Zone[] zones, string id)
        {
            if (zones == null)
                return null;
            return zones.FirstOrDefault((zone) => zone.ZoneId == id);
        }
        void OnReceivedZones(ChangedZoones zones)
        {

            lock (_zones)
            {
                if (zones.ZonesRemoved != null)
                {
                    foreach (string removedZoneId in zones.ZonesRemoved)
                    {
                        if (_zones.ContainsKey(removedZoneId))
                            _zones.Remove(removedZoneId);
                        if (_zonesSeek.ContainsKey(removedZoneId))
                            _zonesSeek.Remove(removedZoneId);
                    }
                }
                if (zones.ZonesAdded != null)
                {
                    foreach (var zone in zones.ZonesAdded)
                    {
                        Zone destZone;
                        if (_zones.TryGetValue(zone.ZoneId, out destZone))
                            destZone.Copy(zone);
                        else
                            _zones.Add(zone.ZoneId, zone);
                    }
                }
                if (zones.ZonesChanged != null)
                {
                    foreach (var zone in zones.ZonesChanged)
                    {
                        Zone destZone;
                        if (_zones.TryGetValue(zone.ZoneId, out destZone))
                            destZone.Copy(zone);
                        else
                            _zones.Add(zone.ZoneId, zone);
                    }
                }
                if (zones.ZonesSeekChanged != null)
                {
                    foreach (var zone in zones.ZonesSeekChanged)
                    {
                        Zone destZone;
                        if (_zones.TryGetValue(zone.ZoneId, out destZone))
                        {
                            if (destZone.NowPlaying != null)
                            {
                                ZoneSeek destZoneSeek;
                                if (_zonesSeek.TryGetValue(zone.ZoneId, out destZoneSeek))
                                    destZoneSeek.Copy(zone);
                                else
                                    _zonesSeek.Add(zone.ZoneId, zone); ;
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("Zones");
        }
        void OnReceivedQueue(ChangedQueue queue)
        {
            _zoneQueue = new List<ChangedQueue>();

            lock (_zoneQueue)
            {
                if (queue.Items != null)
                {
                    _zoneQueue.Add(queue);
                }
                if (queue.Changes != null)
                {
                    _zoneQueue.Add(queue);
                }
            }

        }
    }
}
