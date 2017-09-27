#if UNDOK
using FsApi;
using RoonApiLib;
using Rts.Base.Logger;
using System;
using System.Threading.Tasks;

namespace TestRoonApi
{
    public class RoonControlAdaptorUnDok : IRoonControlAdaptor
    {
        UndokClient             _undok;
        UndokClient.Features    _features;
        UndokClient.Status      _status;
        ILoggerEx               _logger;
        string                  _name;

        public string DisplayName => _name;

        public int MaxVolume => _features.maxVolume;

        public int Volume => _status.volume;

        public bool Muted => _status.mute;

        public bool Power => _status.power;

        public bool Selected => _status.radioMode == ERadioMode.AUXIN;

        public async Task<bool> Start(string ipAddress, string name, ILoggerEx logger)
        {
            _name = name;
            _logger = logger;
            _undok = new UndokClient(new Uri("http://" + ipAddress + "/fsapi/"), true, _logger);

            var res = await _undok.GetFeatures();
            if (!res.Succeeded)
            {
                _logger.Error("GetFeatures", $"HTTP {res.HttpStatusCode.ToString()} {(res.Error != null ? res.Error.Message : string.Empty) }");
                return false;
            }
            _features = res.Value;
            return await GetStatus();
        }
        public async Task<bool> GetStatus ()
        {
            var res = await _undok.GetStatus();
            if (!res.Succeeded)
            {
                _logger.Error("GetStatus", $"HTTP {res.HttpStatusCode.ToString()} {(res.Error != null ? res.Error.Message : string.Empty) }");
                return false;
            }
            _status = res.Value;
            return true;
        }

        public async Task<bool> SetVolume(int volume)
        {
            var res = await _undok.SetVolume(volume);
            if (!res.Succeeded)
                _logger.Error("SetVolume", $"HTTP {res.HttpStatusCode.ToString()} {(res.Error != null ? res.Error.Message : string.Empty) }");
            return res.Succeeded;
        }

        public async Task<bool> SetMuted(bool muted)
        {
            var res = await _undok.SetMute(muted);
            if (!res.Succeeded)
                _logger.Error ("SetMuted", $"HTTP {res.HttpStatusCode.ToString()} {(res.Error != null ? res.Error.Message : string.Empty) }");

            return res.Succeeded;
        }

        public async Task<bool> SetPower(bool on)
        {
            string errorText = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                var rescmd = await _undok.SetPowerStatus(on);
                if (!rescmd.Succeeded)
                {
                    errorText = $"HTTP cmd {rescmd.HttpStatusCode.ToString()} {(rescmd.Error != null ? rescmd.Error.Message : string.Empty) }";
                    continue;
                }
                var resget = await _undok.GetPowerStatus();
                if (!resget.Succeeded)
                {
                    errorText = $"HTTP get {resget.HttpStatusCode.ToString()} {(resget.Error != null ? resget.Error.Message : string.Empty) }";
                    continue;
                }
                if (resget.Value == on)
                    return true;
            }
            _logger.Error("SetPower", errorText);
            return false;
        }

        public async Task<bool> Select()
        {
            var res = await _undok.SetRadioMode(ERadioMode.AUXIN);
            return res.Succeeded;
        }
    }
}
#endif