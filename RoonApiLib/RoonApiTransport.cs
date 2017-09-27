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
            stopped = 0,
            playing, paused, loading
        }
        public enum ELoop
        {
            disabled = 0,
            loop, loop_one, next
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
        public class RoonVolume
        {
            internal void Copy (RoonVolume src)
            {
                Type = src.Type;
                Min = src.Min;
                Max = src.Max;
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
            [JsonProperty("value")]
            public int Value { get; set; }
            [JsonProperty("step")]
            public int Step { get; set; }
            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }
        }
        public class RoonSettings
        {
            internal void Copy(RoonSettings src)
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
        public class RoonLine1
        {
            internal void Copy(RoonLine1 src)
            {
                Line1 = src.Line1;
            }
            [JsonProperty("line1")]
            public string Line1 { get; set; }
        }
        public class RoonLine2 : RoonLine1
        {
            internal void Copy(RoonLine2 src)
            {
                base.Copy(src);
                Line2 = src.Line2;
            }
            [JsonProperty("line2")]
            public string Line2 { get; set; }
        }
        public class RoonLine3 : RoonLine2
        {
            internal void Copy(RoonLine3 src)
            {
                base.Copy(src);
                Line3 = src.Line3;
            }
            [JsonProperty("line3")]
            public string Line3 { get; set; }
        }
        public class RoonNowPlaying
        {
            internal void Copy(RoonNowPlaying src)
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
            }
            [JsonProperty("seek_position")]
            public int? SeekPosition { get; set; }
            [JsonProperty("length")]
            public int Length { get; set; }
            [JsonProperty("one_line")]
            public RoonLine1 OneLine { get; set; }
            [JsonProperty("two_line")]
            public RoonLine2 TwoLine { get; set; }
            [JsonProperty("three_line")]
            public RoonLine3 ThreeLine { get; set; }
            [JsonProperty("image_key")]
            public string ImageKey { get; set; }
        }
        public class RoonOutput
        {
            internal void Copy (RoonOutput src)
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
            public RoonVolume Volume { get; set; }
        }
        public class RoonZone
        {
            internal void Copy(RoonZone src)
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
            public RoonOutput[] Outputs { get; set; }

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
            [JsonProperty("settings")]
            public RoonSettings Settings { get; set; }
            [JsonProperty("now_playing")]
            public RoonNowPlaying NowPlaying { get; set; }

            public RoonOutput FindOutput(string displayName)
            {
                if (Outputs == null)
                    return null;
                return Outputs.FirstOrDefault((o) => o.DisplayName == displayName);
            }
            public RoonOutput FindOutputById(string id)
            {
                if (Outputs == null)
                    return null;
                return Outputs.FirstOrDefault((o) => o.OutputId == id);
            }

        }
        public class RoonZones
        {
            [JsonProperty("zones")]
            public RoonZone[] Zones { get; set; }
        }
        public class RoonZonesChanged
        {
            [JsonProperty("zones_changed")]
            public RoonZone[] ZonesChanged { get; set; }
            [JsonProperty("zones_added")]
            public RoonZone[] ZonesAdded { get; set; }
            [JsonProperty("zones_removed")]
            public string[] ZonesRemoved { get; set; }
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
        public class RoonChangeSettings : RoonSettings
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

        RoonApi                         _api;
        Func<RoonZonesChanged, Task>    _onChangedZones;
        Dictionary<string, RoonZone>    _zones;

        public RoonApiTransport(RoonApi api)
        {
            _api = api;
        }
        public Dictionary<string, RoonZone> Zones
        {
            get => _zones;
        }
        public async Task<bool> SubscribeZones(int subscriptionKey, Func<RoonZonesChanged, Task> onChangedZones)
        {
            _onChangedZones = onChangedZones;
            _zones = new Dictionary<string, RoonZone>();
            int requestId = await _api.SendSubscription(RoonApi.ServiceTransport + "/subscribe_zones", subscriptionKey);
            if (requestId < 0)
                return false;
            _api.AddSubscription(requestId, OnReceived);
            return true;
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
                ZoneOrOutputId = zoneOrOutputId, Shuffle = shuffle, AutoRadio = autoRadio, Loop = loop
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
        async Task<bool> OnReceived (string information, int requestId, string body)
        {
            RoonZones zones;
            RoonZonesChanged changedZones;
            switch (information)
            {
                case "Subscribed":
                    zones = JsonConvert.DeserializeObject<RoonZones>(body);
                    changedZones = new RoonZonesChanged { ZonesAdded = zones.Zones };
                    if (_onChangedZones != null)
                        await _onChangedZones(changedZones);
                    OnReceivedZones(changedZones);
                    break;
                case "Changed":
                    changedZones = JsonConvert.DeserializeObject<RoonZonesChanged>(body);
                    if (_onChangedZones != null)
                        await _onChangedZones(changedZones);
                    OnReceivedZones(changedZones);
                    break;
            }
            return true;
        }
        public static RoonZone FindZone(RoonZone[] zones, string displayName)
        {
            if (zones == null)
                return null;
            return zones.FirstOrDefault((zone) => zone.DisplayName == displayName);
        }
        public static RoonZone FindZoneById(RoonZone[] zones, string id)
        {
            if (zones == null)
                return null;
            return zones.FirstOrDefault((zone) => zone.ZoneId == id);
        }
        void OnReceivedZones(RoonZonesChanged zones)
        {

            lock (_zones)
            {
                if (zones.ZonesRemoved != null)
                {
                    foreach (string removedZoneId in zones.ZonesRemoved)
                    {
                        if (_zones.ContainsKey(removedZoneId))
                            _zones.Remove(removedZoneId);
                    }
                }
                if (zones.ZonesAdded != null)
                {
                    foreach (var zone in zones.ZonesAdded)
                    {
                        if (!_zones.ContainsKey(zone.ZoneId))
                            _zones.Add(zone.ZoneId, zone);
                    }
                }
                if (zones.ZonesChanged != null)
                {
                    foreach (var zone in zones.ZonesChanged)
                    {
                        RoonZone destZone;
                        if (_zones.TryGetValue(zone.ZoneId, out destZone))
                            destZone.Copy(zone);
                        else
                            _zones.Add(zone.ZoneId, zone);
                    }
                }
            }
            OnPropertyChanged("Zones");
        }
    }
}
