namespace SlackClone.Components.Chat;

using Microsoft.AspNetCore.Components;

using SlackClone.Models;
using SlackClone.Services;

public partial class Sidebar
{
    [Inject]
    private IChatService ChatService { get; set; } = default!;

    [Parameter]
    public string? SelectedChannelId { get; set; }

    [Parameter]
    public EventCallback<string> OnChannelSelected { get; set; }

    private IReadOnlyList<Channel> channels = [];
    private IReadOnlyList<Channel> directMessages = [];
    private ChatUser currentUser = default!;
    private bool channelsExpanded = true;
    private bool dmsExpanded = true;
    private string filterText = string.Empty;

    private bool showComposeDialog;
    private bool showAddChannelDialog;
    private bool showAddDmDialog;
    private bool showThreadsDialog;
    private bool showDraftsDialog;
    private bool showSavedDialog;
    private bool showStatusDialog;
    private string statusText = string.Empty;

    private IEnumerable<Channel> FilteredChannels =>
        string.IsNullOrWhiteSpace(filterText)
            ? channels
            : channels.Where(c => c.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<Channel> FilteredDirectMessages =>
        string.IsNullOrWhiteSpace(filterText)
            ? directMessages
            : directMessages.Where(c => c.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase));

    protected override void OnInitialized()
    {
        channels = ChatService.GetChannels();
        directMessages = ChatService.GetDirectMessages();
        currentUser = ChatService.GetCurrentUser();
    }

    private Task SelectChannel(string channelId)
    {
        return OnChannelSelected.InvokeAsync(channelId);
    }

    private void CloseAllDialogs()
    {
        showComposeDialog = false;
        showAddChannelDialog = false;
        showAddDmDialog = false;
        showThreadsDialog = false;
        showDraftsDialog = false;
        showSavedDialog = false;
        showStatusDialog = false;
    }

    private void ApplyStatus(string emoji, string text)
    {
        statusText = text;
        ChatService.SetCustomStatus(emoji, text);
        currentUser = ChatService.GetCurrentUser();
        showStatusDialog = false;
    }

    private void ClearStatus()
    {
        statusText = string.Empty;
        ChatService.SetCustomStatus(null, null);
        currentUser = ChatService.GetCurrentUser();
        showStatusDialog = false;
    }
}
