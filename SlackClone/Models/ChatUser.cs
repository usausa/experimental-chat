namespace SlackClone.Models;

public record ChatUser(
    string Id,
    string DisplayName,
    string AvatarColor,
    UserStatus Status,
    string? CustomStatusEmoji = null,
    string? CustomStatusText = null);

public enum UserStatus
{
    Online,
    Away,
    DoNotDisturb,
    Offline
}
