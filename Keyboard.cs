using SharpDX.DirectInput;
using DSRemapper.Core;
using static DSRemapper.MouseKeyboardInput.KeyboardScanner;
using DSRemapper.Types;

namespace DSRemapper.MouseKeyboardInput
{
    /// <summary>
    /// Direct Input keyboard information class
    /// </summary>
    public class KeyboardInfo : IDSRInputDeviceInfo
    {
        /// <summary>
        /// DirectX DirectInput Device instance class used to create the keyboard
        /// </summary>
        public DeviceInstance DeviceInstance { get; private set; }
        /// <summary>
        /// Gets product GUID of the device
        /// </summary>
        public Guid ProductGuid { get { return DeviceInstance.ProductGuid; } }
        /// <summary>
        /// Gets instance GUID of the device
        /// </summary>
        public Guid InstanceGuid { get { return DeviceInstance.InstanceGuid; } }

        private byte[] ProductBytes { get { return ProductGuid.ToByteArray(); } }
        /// <inheritdoc/>
        public string Id => InstanceGuid.ToString();
        /// <inheritdoc/>
        public string Name => DeviceInstance.ProductName;
        /// <summary>
        /// Gets device product id
        /// </summary>
        public int ProductId { get { return BitConverter.ToUInt16(ProductBytes, 2); } }
        /// <summary>
        /// Gets device vendor id
        /// </summary>
        public int VendorId { get { return BitConverter.ToUInt16(ProductBytes, 0); } }
        /// <summary>
        /// DirectInput Keyboard Info class contructor
        /// </summary>
        public KeyboardInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }
        /// <inheritdoc/>
        public IDSRInputController CreateController()
        {
            return new Keyboard(this);
        }
    }
    /// <summary>
    /// Direct input keyboard scanner class
    /// </summary>
    public class KeyboardScanner : IDSRDeviceScanner
    {
        /// <summary>
        /// DirecInput object used in the plugin
        /// </summary>
        internal static readonly SharpDX.DirectInput.DirectInput DI = new();
        /// <inheritdoc/>
        public IDSRInputDeviceInfo[] ScanDevices()
        {
            return DI.GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly)
                .Select( devInfo => new KeyboardInfo(devInfo)).ToArray();
        }
    }
    /// <summary>
    /// Direct Input Keyboard class
    /// </summary>
    public class Keyboard : IDSRInputController
    {
        static SharpDX.DirectInput.DirectInput DI => KeyboardScanner.DI;

        private readonly KeyboardInfo deviceInfo;
        private readonly SharpDX.DirectInput.Keyboard device = new(DI);
        private readonly IDSRInputReport report = new DefaultDSRInputReport(0, 0, 255, 0, 0, 0, 0);

        /// <inheritdoc/>
        public string Id => deviceInfo.Id;
        /// <inheritdoc/>
        public string Name => deviceInfo.Name;
        /// <inheritdoc/>
        public string Type => "Keyboard";
        /// <inheritdoc/>
        public string ImgPath => "DirectInput.png";
        /// <inheritdoc/>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// DirectInput Keyboard class constructor
        /// </summary>
        /// <param name="info">DirectInput keyboard information requiered to connect the keyboard</param>
        public Keyboard(KeyboardInfo info)
        {
            deviceInfo = info;
        }
        /// <inheritdoc/>
        public void Connect()
        {
            //device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            //device.Properties.BufferSize = 16;
            IsConnected = true;
            device.Acquire();
            /*if (!IsConnected)
                Disconnect();*/
        }
        /// <inheritdoc/>
        public void Disconnect()
        {
            IsConnected = false;
            device.Unacquire();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Disconnect();
        }
        /// <inheritdoc/>
        public IDSRInputReport GetInputReport()
        {
            try
            {
                if (!device.IsDisposed && IsConnected)
                {
                    device.Poll();
                    
                    KeyboardState state= device.GetCurrentState();
                    report.SetButtons(state.AllKeys.Select(state.IsPressed).ToArray());
                    
                }
            }
            catch { }

            return report;
        }
        /// <inheritdoc/>
        public void SendOutputReport(DefaultDSROutputReport report)
        {

        }
    }
}