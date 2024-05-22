using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Model.Requests.DeviceSetting;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Services
{
    public class DeviceSettingService : IDeviceSettingService
    {
        private readonly IDeviceSettingRepo _deviceSettingRepo;
        private readonly IUnitOfWork _unitOfWork;
        public DeviceSettingModel? SelectedSetting { get; set; }

        public DeviceSettingService(ScanContext context)
        {
            _deviceSettingRepo = new DeviceSettingRepo(context);
            _unitOfWork = new UnitOfWork(context);
        }

        public void SetSetting(DeviceSettingModel setting)
        {
            SelectedSetting = setting;
        }

        public void ClearSelectedSetting()
        {
            SelectedSetting = null;
        }

        public async Task<IEnumerable<DeviceSetting>> Get(Expression<Func<DeviceSetting, bool>> predicate)
        {
            return await _deviceSettingRepo.GetAsync(predicate);
        }

        public async Task<DeviceSetting?> FirstOrDefault(Expression<Func<DeviceSetting, bool>> predicate)
        {
            return await _deviceSettingRepo.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> Create(DeviceSettingCreateRequest request)
        {
            try
            {
                DeviceSetting deviceSetting = new DeviceSetting()
                {
                    DeviceName = request.DeviceName,
                    IsDuplex = request.IsDuplex,
                    Size = request.Size,
                    Dpi = request.Dpi,
                    PixelType = request.PixelType,
                    BitDepth = request.BitDepth,
                    RotateDegree = request.RotateDegree,
                    Brightness = request.Brightness,
                    Contrast = request.Contrast,
                    CreatedDate = FormatDateTime(request.CreatedDate)
                };

                await _deviceSettingRepo.AddAsync(deviceSetting);
                await _unitOfWork.Save();

                _unitOfWork.ClearChangeTracker();

                return deviceSetting.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceSetting>> GetAll()
        {
            return await _deviceSettingRepo.GetAllAsync();
        }

        public async Task<int> Update(DeviceSettingUpdateRequest request)
        {
            try
            {
                var editSetting = await _deviceSettingRepo.GetByIdAsync(request.Id);

                if (editSetting == null)
                {
                    return 0;
                }

                editSetting.DeviceName = request.DeviceName;
                editSetting.IsDuplex = request.IsDuplex;
                editSetting.Size = request.Size;
                editSetting.Dpi = request.Dpi;
                editSetting.PixelType = request.PixelType;
                editSetting.BitDepth = request.BitDepth;
                editSetting.RotateDegree = request.RotateDegree;
                editSetting.Brightness = request.Brightness;
                editSetting.Contrast = request.Contrast;

                _deviceSettingRepo.Update(editSetting);
                await _unitOfWork.Save();

                _unitOfWork.ClearChangeTracker();

                return editSetting.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                await _deviceSettingRepo.DeleteAsync(id);
                await _unitOfWork.Save();

                return true;
            }
            catch (Exception)
            {
                throw;

            }
        }

        public string FormatDateTime(string? inputDate)
        {
            if (inputDate == null) return "";
            string formattedDate = "";
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            DateTimeFormatInfo dateTimeFormat = currentCulture.DateTimeFormat;
            string[] allDatePatterns = dateTimeFormat.GetAllDateTimePatterns();

            bool isConverted = false;

            foreach (string format in allDatePatterns)
            {
                DateTime createdDate;

                if (DateTime.TryParseExact(inputDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate))
                {
                    formattedDate = createdDate.ToString("dd/MM/yyyy h:mm:ss tt");
                    isConverted = true;
                }
            }

            if (!isConverted)
            {
                formattedDate = inputDate;
            }

            return formattedDate;
        }
    }
}
