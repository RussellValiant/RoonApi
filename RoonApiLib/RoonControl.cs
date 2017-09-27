using Microsoft.Extensions.Logging;
using RoonApiLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoonApiLib
{
    public class RoonControl
    {
        RoonApi                             _api;
        RoonApiControlVolume                _apiControlVolume;
        RoonApiControlSource                _apiControlSource;
        RoonApiStatus                       _apiStatus;
        Discovery.Result                    _core;
        RoonApi.RoonRegister                _roonRegister;
        RoonApiControlVolume.RoonApiVolume  _volume;
        RoonApiControlSource.RoonApiSource  _source;
        ILogger                             _logger;
        IRoonControlAdaptor                 _adaptor;
        bool                                _online;
        RoonApiControlSource.EStatus        _lastStatus;
        int                                 _lastVolume;
        bool                                _lastMuted;
        CancellationTokenSource             _cancellationTokenSource;
        DateTime                            _nextDeviceCycle;
        int                                 _slowCycleMS, _fastCycleMS, _currentCycleMS;


        public async Task<bool> Start (IRoonControlAdaptor adaptor, string coreName, int slowCycleMS, int fastCycleMS, ILogger logger)
        {
            _adaptor = adaptor;
            _logger = logger;            
            _api = new RoonApi(null, null, null, logger);
            _apiStatus = new RoonApiStatus(_api, $"{_adaptor.DisplayName} OK");
            _online = false;
            _cancellationTokenSource = new CancellationTokenSource();
            _slowCycleMS = slowCycleMS;
            _fastCycleMS = fastCycleMS;

            _lastStatus = GetSourceControlStatus();
            _lastVolume = _adaptor.Volume;
            _lastMuted  = _adaptor.Muted;

            SetSlowCycle();

            logger.LogInformation("Start RIC's Roon Controller");
            // Init Controls
            _apiControlVolume = new RoonApiControlVolume(_api, false);
            _volume = new RoonApiControlVolume.RoonApiVolume
            {
                DisplayName = _adaptor.DisplayName + " Volume",
                VolumeMax = _adaptor.MaxVolume,
                VolumeStep = 1,
                VolumeType = "number",
                VolumeValue = _adaptor.Volume,
                IsMuted = _adaptor.Muted
            };
            _apiControlVolume.AddControl(_volume, new RoonApiControlVolume.RoonApiVolumeFunctions
            {
                SetVolume = async (arg) => {
                    _logger.LogTrace($"SETVOLUME {arg.Mode} {arg.Value}"); 
                    await _adaptor.SetVolume(arg.Value);
                    SetFastCycle();
                    return true;
                },
                Mute = async (arg) => {
                    _logger.LogTrace($"MUTE {arg.Mute} ");
                    await _adaptor.SetMuted(arg.Mute == RoonApiTransport.EMute.mute );
                    SetFastCycle();
                    return true;
                }
            });

            _apiControlSource = new RoonApiControlSource(_api, false);
            _source = new RoonApiControlSource.RoonApiSource
            {
                DisplayName = _adaptor.DisplayName + " Source",
                SupportsStandBy = true,
                Status = GetSourceControlStatus ()
            };
            _apiControlSource.AddControl(_source, new RoonApiControlSource.RoonApiSourceFunctions
            {
                SetStandby = async (arg) => {
                    _logger.LogTrace($"SET STANDBY {arg.Status}");
                    await _adaptor.SetPower(false);
                    SetSlowCycle();
                    return true;
                },
                SetConvenience = async (arg) => {
                    _logger.LogTrace($"SETCONVENIENCE");
                    if (!_adaptor.Power)
                        await _adaptor.SetPower(true);
                    if (_adaptor.Selected)
                        await _adaptor.Select();
                    SetFastCycle();
                    return true;
                }
            });

            // Init Service Registration
            _roonRegister = new RoonApi.RoonRegister
            {
                DisplayName = "Ric's Roon Controller",
                DisplayVersion = "1.0.0",
                Publisher = "Christian Riedl",
                Email = "ric@rts.co.at",
                WebSite = "https://github.com/christian-riedl/roon-control",
                ExtensionId = "com.ric.controller",
                Token = null,
                OptionalServices = new string[0],
                RequiredServices = new string[0],
                ProvidedServices = new string[] { RoonApi.ServiceStatus, RoonApi.ControlVolume, RoonApi.ControlSource }
            };

            Discovery discovery = new Discovery(1000, _logger);
            var coreList = await discovery.QueryServiceId((res) => {
                if (res.CoreName == coreName)
                {
                    _core = res;
                    return true;
                }
                return false;
            });

            await _api.Connect(_core.CoreIPAddress, _core.HttpPort);

            RoonApi.RoonReply info = await _api.GetRegistryInfo();

            bool rc = await _api.RegisterService(_roonRegister);
            return rc;
        }
        void SetFastCycle ()
        {
            _currentCycleMS = _fastCycleMS;
            DateTime nextDeviceCycle = DateTime.UtcNow.AddMilliseconds(_fastCycleMS);
            if (nextDeviceCycle < _nextDeviceCycle)
                _nextDeviceCycle = nextDeviceCycle;
        }
        void SetSlowCycle ()
        {
            _currentCycleMS = _slowCycleMS;
            _nextDeviceCycle = DateTime.UtcNow.AddMilliseconds(_slowCycleMS);
        }


        public async Task ProcessDeviceChangesLoop ()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await ProcessDevicesChanges();
                await Task.Delay(_fastCycleMS);
            }
        }
        public async Task ProcessDevicesChanges ()
        {
            if (DateTime.UtcNow < _nextDeviceCycle)
                return;

            _nextDeviceCycle = DateTime.UtcNow.AddMilliseconds(_currentCycleMS);

            if (await _adaptor.GetStatus())
            {
                if (!_online)
                {
                    await _apiStatus.SetStatus($"{_adaptor.DisplayName} OK", false);
                }
                if (_adaptor.Muted != _lastMuted || _adaptor.Volume != _lastVolume)
                {
                    _lastMuted = _volume.IsMuted = _adaptor.Muted;
                    _lastVolume = _volume.VolumeValue = _adaptor.Volume;
                    _logger.LogTrace($"UPDATEVOLUME volume {_volume.VolumeValue} mute{_volume.IsMuted}");
                    await _apiControlVolume.UpdateState(_volume);
                }
                RoonApiControlSource.EStatus newStatus = GetSourceControlStatus();
                if (newStatus != _lastStatus)
                {
                    _lastStatus = _source.Status = newStatus;
                    _logger.LogTrace($"UPDATESTATUS {newStatus}");
                    await _apiControlSource.UpdateState(_source);
                }
                _online = true;
            }
            else
            {
                if (_online)
                {
                    await _apiStatus.SetStatus($"{_adaptor.DisplayName} FAILED", true);
                }
                _online = false;
            }

        }
        public void Close ()
        {
            _logger.LogTrace("Closing");
            _cancellationTokenSource.Cancel();
            _logger.LogTrace("Closed");
        }
        RoonApiControlSource.EStatus GetSourceControlStatus ()
        {
            return _adaptor.Power ? (_adaptor.Selected ? RoonApiControlSource.EStatus.selected : RoonApiControlSource.EStatus.deselected) : RoonApiControlSource.EStatus.standby;
        }
    }
}
