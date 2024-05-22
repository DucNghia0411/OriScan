using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.DeviceSetting
{
    public class DeviceSettingUpdateRequest
    {
        public int Id { get; set; }

        public string SettingName { get; set; } = null!;

        public string DeviceName { get; set; } = null!;

        public int IsDuplex { get; set; }

        public string? Size { get; set; } = null!;

        public int Dpi { get; set; }

        public string? PixelType { get; set; } = null!;

        public int BitDepth { get; set; }

        public int RotateDegree { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }
    }
}
