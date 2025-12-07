namespace AiTranslator.Services;

public class NotificationService : INotificationService
{
    private NotifyIcon? _notifyIcon;
    private readonly IConfigService _configService;

    public NotificationService(IConfigService configService)
    {
        _configService = configService;
    }

    public void SetNotifyIcon(NotifyIcon notifyIcon)
    {
        _notifyIcon = notifyIcon;
    }

    public void ShowNotification(string title, string message)
    {
        if (_configService.Config.Ui.ShowNotifications && _notifyIcon != null)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(3000);
        }
    }
}

