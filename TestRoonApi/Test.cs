using Microsoft.Extensions.Logging;
using RoonApiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestRoonApi
{
    public partial class Test : Form
    {
        LoggerFactory _loggerFactory;
        string _selectedZoneId;
        string _selectedOutputId;
        RoonApi _api;
        RoonApiTransport _apiTransport;
        RoonApiBrowse _apiBrowse;
        RoonApiImage _apiImage;
        RoonApiControlVolume _apiControlVolume;
        RoonApiControlSource _apiControlSource;
        RoonApiStatus _apiStatus;
        RoonApiSettings _apiSettings;
        string _lastImageKey;
        Stack<string> _itemStack;
        bool _setZoneSettings;
        bool _setOutputVolume;
        Discovery.Result _core;
        RoonApi.RoonRegister _roonRegister;
        List<RoonApiBrowse.LoadItem> _items;
        List<RoonApiSettings.LayoutBase> _layout;
        Dictionary<string, string> _values;
        public Test()
        {
            InitializeComponent();

            // Init Logger
            _loggerFactory = new LoggerFactory();
            LogLevel logLevel;
            Enum.TryParse(Properties.Settings.Default.LogLevel, out logLevel);
            _loggerFactory.AddDebug(logLevel);

            // Init Roon Api
            _api = new RoonApi(OnPaired, OnUnPaired, Properties.Settings.Default.PersistDirectory, _loggerFactory.CreateLogger("RoonApi"));
            _apiTransport = new RoonApiTransport(_api);
            _apiImage = new RoonApiImage(_api);
            _apiBrowse = new RoonApiBrowse(_api);
            _apiStatus = new RoonApiStatus(_api, "All systems roger");

            _layout = new List<RoonApiSettings.LayoutBase>(new RoonApiSettings.LayoutBase[]
            {
                new RoonApiSettings.LayoutLabel    ("*A string setting*"),
                new RoonApiSettings.LayoutString   ("A string setting", "text", 20) { SubTitle = "subtitle" },
                new RoonApiSettings.LayoutButton   ("A Button", "button", "1"),
                new RoonApiSettings.LayoutDropDown ("A combo setting",  "combo", new RoonApiSettings.LayoutDropDownValue[] {
                                                         new RoonApiSettings.LayoutDropDownValue("text1"),
                                                         new RoonApiSettings.LayoutDropDownValue("text2"),
                                                         new RoonApiSettings.LayoutDropDownValue("text3")
                                                    })
            });
            _values = new Dictionary<string, string>();
            _values.Add("text", "*hudriwudri*");
            _values.Add("button", "true");
            _values.Add("combo", "text2");

            _apiSettings = new RoonApiSettings(_api, _layout, _values, new RoonApiSettings.Functions {
                ButtonPressed = (bp) => {
                    return Task.FromResult(true);
                },
                SaveSettings = (s) =>
                {
                    //values["text"] = "HASERROR";
                    //layout[0].Error = "Her is an error";
                    _values["combo"] = s.Settings.Values["combo"];
                    if (_values["combo"] == "text3")
                        return Task.FromResult(true);
                    else
                        return Task.FromResult(false);
                }
            });

            // Init Controls
            _apiControlVolume = new RoonApiControlVolume(_api, true);
            RoonApiControlVolume.Volume volume = new RoonApiControlVolume.Volume
            {
                DisplayName = "Ric Volume Control",
                VolumeMax = 100,
                VolumeStep = 1,
                VolumeType = "number",
                VolumeValue = 4
            };
            _apiControlVolume.AddControl(volume, new RoonApiControlVolume.VolumeFunctions {
                SetVolume = (arg) => { System.Diagnostics.Debug.WriteLine($"SETVOLUME {arg.Mode} {arg.Value}"); return Task.FromResult(true); },
                Mute = (arg) => { System.Diagnostics.Debug.WriteLine($"MUTE {arg.Mute} "); return Task.FromResult(true); }
            });

            _apiControlSource = new RoonApiControlSource(_api, true);
            RoonApiControlSource.Source source = new RoonApiControlSource.Source
            {
                DisplayName = "Ric Source Control",
                SupportsStandBy = true,
                Status = RoonApiControlSource.EStatus.selected
            };
            _apiControlSource.AddControl(source, new RoonApiControlSource.SourceFunctions {
                SetStandby = (arg) => { System.Diagnostics.Debug.WriteLine($"STATE {arg.Status}"); return Task.FromResult(true); },
                SetConvenience = (arg) => { System.Diagnostics.Debug.WriteLine($"SETCONVENIENCE"); return Task.FromResult(true); }
            });

            // Init Service Registration
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
                RequiredServices = new string[] { RoonApi.ServiceTransport, RoonApi.ServiceImage, RoonApi.ServiceBrowse,  },
                ProvidedServices = new string[] { RoonApi.ServiceStatus, RoonApi.ServicePairing, RoonApi.ServiceSettings, RoonApi.ServicePing, RoonApi.ControlVolume, RoonApi.ControlSource }
            };

            // Init UI
            textRoonCoreName.Text = Properties.Settings.Default.RoonCoreName;
            textRoonCoreName.TextChanged += (s, e) => Properties.Settings.Default.RoonCoreName = textRoonCoreName.Text;
            textPersistenceDirectory.Text = Properties.Settings.Default.PersistDirectory;
            textPersistenceDirectory.TextChanged += (s, e) => Properties.Settings.Default.PersistDirectory = textPersistenceDirectory.Text;
            comboLogLevel.Items.AddRange(Enum.GetNames(typeof(LogLevel)));
            comboLogLevel.SelectedItem = Properties.Settings.Default.LogLevel;
            comboLogLevel.SelectedValueChanged += (s, e) => Properties.Settings.Default.LogLevel = comboLogLevel.SelectedItem.ToString();
            comboLoop.Items.AddRange(Enum.GetNames(typeof(RoonApiTransport.ELoop)));
            _itemStack = new Stack<string>();
            _setZoneSettings = true;
            _setOutputVolume = true;
        }
        async Task OnPaired (string coreId)
        {
            var zones = await _apiTransport.SubscribeZones(0, onChangedZones);

        }
        Task OnUnPaired (string coreId)
        {
            return Task.CompletedTask;
        }
        private async void Test_Load(object sender, EventArgs e)
        {
            await DiscoverCore();

            if (_core == null)
                return;

            _api.StartReceiver(_core.CoreIPAddress, _core.HttpPort, _roonRegister);
        }
        async Task DiscoverCore ()
        {
            Discovery discovery = new Discovery(textIpAddress.Text, 1000, _loggerFactory.CreateLogger("Discovery"));
            var coreList = await discovery.QueryServiceId((res) => {
                if (res.CoreName == textRoonCoreName.Text)
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
                    textRoonCoreName.Text = _core.CoreName;
                }
                else
                {
                    string corenames = string.Join(", ", coreList.Select((s) => s.CoreName));
                    MessageBox.Show($"Enter name of Roon core [{corenames}] and restart");
                    return;
                }
            }
        }
        Task onChangedZones (RoonApiTransport.ChangedZoones zones)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    onChangedZones(zones);
                });
                return Task.CompletedTask;
            }
            if (comboZone.Items.Count == 0 && zones.ZonesChanged != null)
            {
                foreach (var zone in zones.ZonesChanged)
                {
                    comboZone.Items.Add(zone.DisplayName);
                }
            }
            if (zones.ZonesRemoved != null)
            {
                foreach (string zoneid in zones.ZonesRemoved)
                {
                    RoonApiTransport.Zone zone;
                    if (_apiTransport.Zones.TryGetValue(zoneid, out zone))
                    {
                        foreach (var item in comboZone.Items)
                        {
                            if (item.ToString() == zone.DisplayName)
                            {
                                if (zone.ZoneId == _selectedZoneId) 
                                    _selectedZoneId = _selectedOutputId = null;
                                comboZone.Items.Remove(item);
                                break;
                            }
                        }
                    }
                }
            }
            if (zones.ZonesAdded != null)
            {
                foreach (var zone in zones.ZonesAdded)
                {
                    comboZone.Items.Add(zone.DisplayName);
                }
            }
            UpdateZone();
            return Task.CompletedTask;
        }
        void UpdateZone ()
        {
            if (_selectedZoneId == null)
                return;
            RoonApiTransport.Zone zone;
            if (!_apiTransport.Zones.TryGetValue(_selectedZoneId, out zone))
                return;

            textState.Text = zone.State.ToString();
            if (zone.NowPlaying != null)
            {
                textLength.Text = zone.NowPlaying.Length.ToString();
                if (zone.NowPlaying.ThreeLine != null)
                {
                    textLine1.Text = zone.NowPlaying.ThreeLine.Line1;
                    textLine2.Text = zone.NowPlaying.ThreeLine.Line2;
                    textLine3.Text = zone.NowPlaying.ThreeLine.Line3;
                }
                if (zone.NowPlaying.SeekPosition != null)
                    textPosition.Text = zone.NowPlaying.SeekPosition.Value.ToString();
                else
                    textPosition.Text = string.Empty;

                if (_lastImageKey == null || _lastImageKey != zone.NowPlaying.ImageKey)
                {
                    _lastImageKey = zone.NowPlaying.ImageKey;
                    UpdateImage(zone.NowPlaying.ImageKey);
                }
            }
            if (zone.Outputs != null && zone.Outputs[0].Volume != null)
            {
                textVolume.Text = zone.Outputs[0].Volume.Value.ToString();
                volumeSlider.Minimum = zone.Outputs[0].Volume.Min;
                volumeSlider.Maximum = zone.Outputs[0].Volume.Max;
                checkMuted.Checked = zone.Outputs[0].Volume.IsMuted;
                if (_setOutputVolume)
                {
                    volumeSlider.Value = zone.Outputs[0].Volume.Value;
                    checkMute.Checked = zone.Outputs[0].Volume.IsMuted;
                    _setOutputVolume = false;
                }
            }
            if (zone.Settings != null)
            {
                checkRadio.Checked = zone.Settings.AutoRadio;
                checkShuffle.Checked = zone.Settings.Shuffle;
                textLoop.Text = zone.Settings.Loop.ToString();
                if (_setZoneSettings)
                {
                    checkSetRadio.Checked = zone.Settings.AutoRadio;
                    checkSetShuffle.Checked = zone.Settings.Shuffle;
                    comboLoop.SelectedItem = zone.Settings.Loop.ToString();
                    _setZoneSettings = false;
                }
            }
        }
        async Task LoadItems (string itemKey, string input) 
        {
            _items = null;
            if (string.IsNullOrEmpty(input))
                input = null;
            List<RoonApiBrowse.LoadItem> list = new List<RoonApiBrowse.LoadItem>();
            var browseResult = await _apiBrowse.Browse(new RoonApiBrowse.BrowseOptions { Hierarchy = "browse", ZoneOrOutputId = _selectedZoneId, PopAll = itemKey == null, ItemKey = itemKey, Input = input });
            for (int i = 0; i < browseResult.List.Count; i+= 100)
            {
                var loadResult = await _apiBrowse.Load(new RoonApiBrowse.LoadOptions { Hierarchy = "browse", Offset = i, SetDisplayOffset = i });
                list.AddRange(loadResult.Items);
            }
            if (itemKey != null)
            {
                _itemStack.Push(itemKey);
            }
            listItems.Items.Clear();
            foreach (var item in list)
            {
                listItems.Items.Add(item.Title);
                System.Diagnostics.Debug.WriteLine($"Item {item.ItemKey} - {item.Title} - {item.SubTitle} - {item.Hint}");
            }
            _items = list;
        }
        async void UpdateImage(string imageKey)
        {
            _apiImage = new RoonApiImage(_api);
            RoonApiImage.Image image = new RoonApiImage.Image
            {
                ImageKey = imageKey,
                Options = new RoonApiImage.Options
                {
                    Format = "image/png",
                    Height = 512,
                    Width = 512,
                    Scale = RoonApiImage.EScale.fill
                }
            };
            byte[] data = await _apiImage.GetImage(image);
            Bitmap bitmap;
            using (MemoryStream stream = new MemoryStream(data))
            {
                bitmap = new Bitmap(stream);
                pictureBox.Image = bitmap;
            }
        }

        private void Test_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void comboZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var zone in _apiTransport.Zones.Values)
            {
                if (zone.DisplayName == comboZone.SelectedItem.ToString())
                {
                    _setZoneSettings = true;
                    _setOutputVolume = true;
                    _selectedZoneId = zone.ZoneId;
                    _selectedOutputId = zone.Outputs[0].OutputId;
                    UpdateZone();
                    break;
                }
            }
        }

        private async void buttonHome_Click(object sender, EventArgs e)
        {
            _itemStack.Clear();
            await LoadItems(null, null);
        }

        private async void buttonBack_Click(object sender, EventArgs e)
        {
            if (_itemStack.Count > 1)
            {
                string key = _itemStack.Pop();
                key = _itemStack.Pop();
                await LoadItems(key, null);
            }
        }

        private async void listItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listItems.SelectedIndex >= 0 && _items != null)
            {
                RoonApiBrowse.LoadItem item = _items[listItems.SelectedIndex];
                if (item.Hint != "action")
                    await LoadItems(item.ItemKey, textSearch.Text);
                else
                {
                    var browseResult = await _apiBrowse.Browse(new RoonApiBrowse.BrowseOptions { Hierarchy = "browse", ZoneOrOutputId = _selectedZoneId, ItemKey = item.ItemKey });
                }
            }
        }

        private async void buttonControl_Clicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            RoonApiTransport.EControl control;
            Enum.TryParse(btn.Tag.ToString(), out control);
            bool rc = await _apiTransport.Control(_selectedZoneId, control);
        }

        private async void volumeSlider_ValueChanged(object sender, EventArgs e)
        {
            if (_setOutputVolume)
                return;
            bool rc = await _apiTransport.ChangeVolume(_selectedOutputId, RoonApiTransport.EVolumeMode.absolute, volumeSlider.Value);
        }

        private async void checkMute_CheckedChanged(object sender, EventArgs e)
        {
            if (_setOutputVolume)
                return;
            bool rc = await _apiTransport.Mute (_selectedOutputId, checkMute.Checked ? RoonApiTransport.EMute.mute : RoonApiTransport.EMute.unmute);
        }

        private async void settings_Changed(object sender, EventArgs e)
        {
            if (_setZoneSettings)
                return;
            RoonApiTransport.ELoop loop;
            Enum.TryParse(comboLoop.SelectedItem.ToString(), out loop);
            bool rc = await _apiTransport.ChangeSettings(_selectedZoneId, checkSetShuffle.Checked, checkSetRadio.Checked, loop);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            _api.Close();
            comboZone.Items.Clear();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            _api.StartReceiver(_core.CoreIPAddress, _core.HttpPort, _roonRegister);
        }

        private async void buttonSendStatus_Click(object sender, EventArgs e)
        {
            await _apiStatus.SetStatus(textStatus.Text, checkIsError.Checked);
        }

        private async void buttonPlayRadio_Click(object sender, EventArgs e)
        {
            var options = new RoonApiBrowse.BrowseOptions { Hierarchy = "browse", ZoneOrOutputId = _selectedZoneId, PopAll = true };
            var loadResult = await _apiBrowse.BrowseAndLoad(options);
            var loadItem = loadResult.FindItem(RoonApiBrowse.BrowseInternetRadio);
            if (loadItem != null)
            {
                options.PopAll = false;
                options.ItemKey = loadItem.ItemKey;
            }
            loadResult = await _apiBrowse.BrowseAndLoad(options);
            loadItem = loadResult.Items[0];
            options.ItemKey = loadItem.ItemKey;
            loadResult = await _apiBrowse.BrowseAndLoad(options);
        }
    }
}
