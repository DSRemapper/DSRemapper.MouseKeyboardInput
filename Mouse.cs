using DSRemapper.Core;
using static DSRemapper.MouseKeyboardInput.KeyboardScanner;
using DSRemapper.Types;
using SharpDX.DirectInput;

namespace DSRemapper.MouseKeyboardInput
{
    /// <summary>
    /// Direct Input mouse information class
    /// </summary>
    public class MouseInfo : IDSRInputDeviceInfo
    {
        /// <summary>
        /// DirectX DirectInput Device instance class used to create the mouse
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
        public MouseInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }
        /// <inheritdoc/>
        public IDSRInputController CreateController()
        {
            return new Mouse(this);
        }
    }
    /// <summary>
    /// Direct input mouse scanner class
    /// </summary>
    public class MouseScanner : IDSRDeviceScanner
    {
        /// <summary>
        /// DirecInput object used in the plugin
        /// </summary>
        internal static readonly SharpDX.DirectInput.DirectInput DI = new();
        /// <inheritdoc/>
        public IDSRInputDeviceInfo[] ScanDevices()
        {
            return DI.GetDevices(DeviceClass.Pointer, DeviceEnumerationFlags.AttachedOnly)
                .Select( devInfo => new MouseInfo(devInfo)).ToArray();
        }
    }
    /// <summary>
    /// Direct Input Mouse class
    /// </summary>
    public class Mouse : IDSRInputController
    {
        static SharpDX.DirectInput.DirectInput DI => MouseScanner.DI;
        private readonly MouseInfo deviceInfo;
        private readonly SharpDX.DirectInput.Mouse device = new(DI);
        private readonly IDSRInputReport report = new DefaultDSRInputReport(3,0,8);

        /// <inheritdoc/>
        public string Id => deviceInfo.Id;
        /// <inheritdoc/>
        public string Name => deviceInfo.Name;
        /// <inheritdoc/>
        public string Type => "Mouse";
        /// <inheritdoc/>
        public string ImgPath => "DirectInput.png";
        /// <inheritdoc/>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// DirectInput Mouse class constructor
        /// </summary>
        /// <param name="info">DirectInput mouse information requiered to connect the mouse</param>
        public Mouse(MouseInfo info)
        {
            deviceInfo = info;
            //device = DI.CreateDevice(info.InstanceGuid);
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
                if (IsConnected)
                {
                    device.Poll();
                    MouseState state = device.GetCurrentState();
                    report.Axes[0] = state.X / 120f;
                    report.Axes[1] = state.Y / 120f;
                    report.Axes[2] = state.Z / 120f;
                    report.SetButtons(state.Buttons);
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