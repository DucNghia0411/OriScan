using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.DeviceSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IDeviceSettingService
    {
        DeviceSettingModel? SelectedSetting { get; set; }

        void SetSetting(DeviceSettingModel setting);

        void ClearSelectedSetting();

        Task<IEnumerable<DeviceSetting>> Get(Expression<Func<DeviceSetting, bool>> predicate);

        Task<DeviceSetting?> FirstOrDefault(Expression<Func<DeviceSetting, bool>> predicate);

        Task<int> Create(DeviceSettingCreateRequest request);

        Task<IEnumerable<DeviceSetting>> GetAll();

        Task<int> Update(DeviceSettingUpdateRequest request);

        Task<bool> Delete(int id);
    }
}
