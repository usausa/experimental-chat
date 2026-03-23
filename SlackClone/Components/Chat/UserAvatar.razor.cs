namespace SlackClone.Components.Chat;

using Microsoft.AspNetCore.Components;

using SlackClone.Models;

public partial class UserAvatar
{
    [Parameter]
    [EditorRequired]
    public ChatUser User { get; set; } = default!;

    [Parameter]
    public bool ShowStatus { get; set; }

    [Parameter]
    public AvatarSize Size { get; set; } = AvatarSize.Medium;

    private string SizeClass => Size switch
    {
        AvatarSize.Small => "avatar-sm",
        AvatarSize.Large => "avatar-lg",
        _ => "avatar-md"
    };

    private string StatusClass => User.Status switch
    {
        UserStatus.Online => "status-online",
        UserStatus.Away => "status-away",
        UserStatus.DoNotDisturb => "status-dnd",
        _ => "status-offline"
    };

    public enum AvatarSize
    {
        Small,
        Medium,
        Large
    }
}
