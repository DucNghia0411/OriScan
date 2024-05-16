using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginalScan
{
    public abstract partial class DeviceSettingConverter
    {
        public static bool _isDefault;
        public static bool? _duplex;
        public static object? _size;
        public static object? _dpi;
        public static object? _pixelType;
        public static object? _bitDepth;
        public static object? _rotateDegree;
        public static object? _brightness;
        public static object? _contrast;

        public static void SetIsDefault(bool isDefault)
        {
            _isDefault = isDefault;
        }

        public static void SetDuplex(bool? isDuplex)
        {
            _duplex = isDuplex;
        }

        public static void SetSize(object? size)
        {
            _size = size;
        }

        public static void SetDpi(object? dpi)
        {
            _dpi = dpi;
        }

        public static void SetPixeType(object? pixelType)
        {
            _pixelType = pixelType;
        }

        public static void SetBitDepth(object? depth)
        {
            _bitDepth = depth;
        }

        public static void SetRotateDegree(object? rotateDegree)
        {
            _rotateDegree = rotateDegree;
        }

        public static void SetBrightness(object? brightness)
        {
            _brightness = brightness;
        }

        public static void SetContrast(object? contrast)
        {
            _contrast = contrast;
        }        
    }
}
