using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Domain.Requests;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class UsersSettingsManager : IUsersSettingsManager
{
    private readonly ICacheRepository _cacheRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsersSettingsRepository _usersSettingsRepository;

    public UsersSettingsManager(IUsersSettingsRepository usersSettingsRepository, [FromKeyedServices(CacheType.InMemory)] ICacheRepository cacheRepository, IUnitOfWork unitOfWork)
    {
        _usersSettingsRepository = usersSettingsRepository;
        _cacheRepository = cacheRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserSetting>> GetUserSettingsByUserId(Guid userId)
    {
        // Trying getting the calendar from the cache first
        var usrSettingsItem =
            $"{userId}-settings"; //TODO: Replicated. Set in config? Or in better place.
        var userSettingsFromCache =
            await _cacheRepository.Get<List<UserSetting>>(usrSettingsItem);
        if (userSettingsFromCache != null) return userSettingsFromCache;

        var userSettings = new List<UserSetting>();

        var userSettingsDto = await _usersSettingsRepository.GetUserSettings(userId);

        foreach (var userSettingDto in userSettingsDto)
            userSettings.Add(
                new UserSetting
                {
                    Id = userSettingDto.Id,
                    UserId = userSettingDto.UserId,
                    SettingValue = userSettingDto.SettingValue,
                    SettingName = userSettingDto.SettingName
                });

        // Cache settings for one month. Cache will be invalidated when new settings are created or updated
        // TODO: Time is replicated in CreateOrUpdateUserSettings
        await _cacheRepository.Save(usrSettingsItem, userSettings, 2592000);

        return userSettings;
    }


    public async Task<List<UserSetting>> CreateOrUpdateUserSettings(
        CreateOrUpdateUserSettingsRequest createOrUpdateUserSettingsRequest)
    {
        if (!createOrUpdateUserSettingsRequest.UserSettings.Any())
            throw new Exception("No settings to be created or updated");

        var userId = createOrUpdateUserSettingsRequest.UserSettings.First().UserId;
        var currentUserSettings = (await _usersSettingsRepository.GetUserSettings(userId)).ToList();

        // Remove current settings from cache
        var usrSettingsItem = $"{userId}-settings"; //TODO: Replicated. Set in config? Or in better place.
        await _cacheRepository.Remove(usrSettingsItem);

        // Lists to track settings for update and creation
        var settingsToUpdate = new List<UserSetting>();
        var settingsToCreate = new List<UserSetting>();

        foreach (var userSetting in createOrUpdateUserSettingsRequest.UserSettings)
        {
            // Check if the setting already exists for the user
            var existingSetting = currentUserSettings.FirstOrDefault(s => s.SettingName == userSetting.SettingName);

            if (existingSetting != null)
            {
                // Setting already exists, update it
                settingsToUpdate.Add(new UserSetting
                {
                    Id = existingSetting.Id,
                    SettingName = userSetting.SettingName, // Possible New value
                    SettingValue = userSetting.SettingValue, // Possible New value
                    UserId = userId,
                    IsEnabled = userSetting.IsEnabled
                });
            }
            else
            {
                // Setting doesn't exist, create it
                userSetting.Id = Guid.NewGuid();
                userSetting.IsEnabled = true;
                settingsToCreate.Add(userSetting);
            }
        }

        await _unitOfWork.BeginTransaction();
        try
        {
            foreach (var userSetting in settingsToUpdate)
                // Update settings in the repository
                await _usersSettingsRepository.UpdateUserSetting(userSetting);

            foreach (var userSetting in settingsToCreate)
                // Create new settings in the repository
                await _usersSettingsRepository.CreateUserSetting(userSetting);

            await _unitOfWork.Commit();
        }
        catch
        {
            await _unitOfWork.Rollback();
            throw;
        }

        var allSettings = settingsToUpdate.Concat(settingsToCreate).ToList();

        // Cache settings for one month. Cache will be invalidated when new settings are created or updated
        // TODO: Time is replicated in GetUserSettings
        await _cacheRepository.Save(usrSettingsItem, allSettings, 2592000);

        // Return the updated user settings
        return allSettings;
    }
}