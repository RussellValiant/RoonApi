using Microsoft.Extensions.Logging;
using RoonApiLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestRoonApiUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ILoggerFactory          _loggerFactory;
        Discovery.Result        _core;
        RoonApi                 _api;
        RoonApiTransport        _apiTransport;
        RoonApi.RoonRegister    _roonRegister;
        string                  _myIpAddress;

        public MainPage()
        {
            this.InitializeComponent();
            _loggerFactory = new LoggerFactory();
            _api = new RoonApi(OnPaired, OnUnPaired, ApplicationData.Current.TemporaryFolder.Path, _loggerFactory.CreateLogger("RoonApi"));
            _apiTransport = new RoonApiTransport(_api);
            _myIpAddress = "192.168.1.130";
            _roonRegister = new RoonApi.RoonRegister
            {
                DisplayName = "RICs Roon API Test",
                DisplayVersion = "1.0.0",
                Publisher = "Christian Riedl",
                Email = "ric@rts.co.at",
                WebSite = "https://github.com/christian-riedl/roon-extension-test",
                ExtensionId = "com.ric.test",
                Token = null,
                OptionalServices = new string[0],
                RequiredServices = new string[] { RoonApi.ServiceTransport, RoonApi.ServiceImage, RoonApi.ServiceBrowse, },
                ProvidedServices = new string[] { RoonApi.ServiceStatus, RoonApi.ServicePairing, RoonApi.ServiceSettings, RoonApi.ServicePing, RoonApi.ControlVolume, RoonApi.ControlSource }
            };

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // await DiscoverCore();
            await DiscoverCore("RICSRV");   // Faster when specifying core name
            Task.Run(() => _api.StartReceiver(_core.CoreIPAddress, _core.HttpPort, _roonRegister));
        }
        async Task DiscoverCore(string coreName = null)
        {
            Discovery discovery = new Discovery(_myIpAddress, 10000, _loggerFactory.CreateLogger("Discovery"));
            var coreList = await discovery.QueryServiceId((res) => {
                if (coreName != null && res.CoreName == coreName)
                {
                    _core = res;
                    return true;
                }
                return false;
            });
            if (_core == null)
            {
                if (coreList.Count == 1)
                {
                    _core = coreList[0];
                    //textRoonCoreName.Text = _core.CoreName;
                }
                else
                {
                    string corenames = string.Join(", ", coreList.Select((s) => s.CoreName));
                    throw new Exception ("Multiple Roon Cores found");
                }
            }
        }
        async Task OnPaired(string coreId)
        {
            var zones = await _apiTransport.SubscribeZones(0, OnZooneChanged);
            await UpdateUI(() => text.Text = "Successfully paired");
        }
        async Task UpdateUI (Action updateAction)
        {
            if (Dispatcher.HasThreadAccess)
            {
                updateAction();
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    updateAction();
                });
            }
        }
        Task OnUnPaired(string coreId)
        {
            return Task.CompletedTask;
        }
        Task OnZooneChanged (RoonApiTransport.ChangedZoones changedZones)
        {
            return Task.CompletedTask;
        }
    }
}
