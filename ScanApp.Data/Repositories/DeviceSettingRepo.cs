using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data.Repositories
{
    public interface IDeviceSettingRepo : IGenericRepository<DeviceSetting> { }

    public class DeviceSettingRepo : GenericRepository<DeviceSetting>, IDeviceSettingRepo
    {
        public DeviceSettingRepo(ScanContext context) : base(context) { }
    }
}
