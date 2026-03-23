namespace SlackClone.Components.Chat;

using System.Globalization;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SlackClone.Models;
using SlackClone.Services;

public sealed partial class ChannelView : IDisposable
{
    [Inject]
    private IChatService ChatService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public string? ChannelId { get; set; }

    private Channel? Channel => ChannelId is not null ? ChatService.GetChannel(ChannelId) : null;
    private IReadOnlyList<ChatMessage> messages = [];
    private IReadOnlyList<ChatUser> allUsers = [];
    private ChatUser currentUser = default!;
    // ReSharper disable once NotAccessedField.Local
    private ElementReference messagesContainer;

    // Thread
    private string? activeThreadId;
    private ChatMessage? threadParent;
    private IReadOnlyList<ChatMessage> threadReplies = [];

    // Panels
    private bool showMembersPanel;
    private bool showSearchPanel;
    private bool showPinnedPanel;
    private string searchQuery = string.Empty;
    private IReadOnlyList<ChatMessage> searchResults = [];

    // Profile
    private ChatUser? profileUser;

    // Typing indicator
    private string? typingUser;
    private CancellationTokenSource? typingCts;

    private string InputPlaceholder =>
        Channel is null ? "Message" :
        Channel.IsDirectMessage ? $"Message {Channel.Name}" :
        $"Message #{Channel.Name}";

    protected override void OnInitialized()
    {
        currentUser = ChatService.GetCurrentUser();
        allUsers = ChatService.GetAllUsers();
    }

    protected override void OnParametersSet()
    {
        messages = ChannelId is not null ? ChatService.GetMessages(ChannelId) : [];
        if (ChannelId is not null)
        {
            ChatService.MarkAsRead(ChannelId);
        }

        activeThreadId = null;
        threadParent = null;
        threadReplies = [];
        showMembersPanel = false;
        showSearchPanel = false;
        showPinnedPanel = false;
        typingCts?.Cancel();
        typingUser = null;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return ScrollToBottomAsync();
    }

    private async Task HandleSendAsync(string content)
    {
        if (ChannelId is null)
        {
            return;
        }

        ChatService.SendMessage(ChannelId, content);
        RefreshMessages();
        await ScrollToBottomAsync();
        _ = SimulateTypingAsync();
    }

    private void HandleToggleReactionAsync(string messageId, string emoji)
    {
        ChatService.ToggleReaction(messageId, emoji);
        RefreshMessages();
    }

    private void HandleEditAsync((string Id, string Content) args)
    {
        ChatService.EditMessage(args.Id, args.Content);
        RefreshMessages();
    }

    private void HandleDeleteAsync(string messageId)
    {
        ChatService.DeleteMessage(messageId);
        RefreshMessages();
    }

    private void OpenThread(string messageId)
    {
        activeThreadId = messageId;
        threadParent = messages.FirstOrDefault(m => m.Id == messageId);
        threadReplies = ChatService.GetThreadReplies(messageId);
        showMembersPanel = false;
        showSearchPanel = false;
    }

    private void CloseThread()
    {
        activeThreadId = null;
        threadParent = null;
        threadReplies = [];
    }

    private void HandleThreadReplyAsync(string content)
    {
        if (ChannelId is null || activeThreadId is null)
        {
            return;
        }

        ChatService.SendThreadReply(ChannelId, activeThreadId, content);
        threadReplies = ChatService.GetThreadReplies(activeThreadId);
        RefreshMessages();
    }

    private void OpenProfile(string userId)
    {
        profileUser = ChatService.GetUser(userId);
    }

    private void HandleBookmarkAsync(string messageId)
    {
        ChatService.ToggleBookmark(messageId);
    }

    private void HandlePinAsync(string messageId)
    {
        var msg = messages.FirstOrDefault(m => m.Id == messageId);
        if (msg is null)
        {
            return;
        }

        if (msg.IsPinned)
        {
            ChatService.UnpinMessage(messageId);
        }
        else
        {
            ChatService.PinMessage(messageId);
        }

        RefreshMessages();
    }

    private Task HandleSearchKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            return DoSearchAsync();
        }

        return Task.CompletedTask;
    }

    private Task DoSearchAsync()
    {
        searchResults = string.IsNullOrWhiteSpace(searchQuery) ? [] : ChatService.SearchMessages(searchQuery);
        return Task.CompletedTask;
    }

    private void RefreshMessages()
    {
        if (ChannelId is not null)
        {
            messages = ChatService.GetMessages(ChannelId);
        }
    }

    private async Task ScrollToBottomAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("eval", "document.querySelector('.channel-messages')?.scrollTo(0, document.querySelector('.channel-messages')?.scrollHeight)");
        }
        catch (JSDisconnectedException)
        {
            // Ignored during prerender or disconnect
        }
    }

    private static string FormatTimestamp(DateTimeOffset ts)
    {
        var now = DateTimeOffset.Now;
        if (ts.Date == now.Date)
        {
            return ts.ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        if (ts.Date == now.Date.AddDays(-1))
        {
            return "Yesterday " + ts.ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        return ts.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);
    }

    private static string FormatDateDivider(DateTimeOffset ts)
    {
        var now = DateTimeOffset.Now;
        if (ts.Date == now.Date)
        {
            return "Today";
        }

        if (ts.Date == now.Date.AddDays(-1))
        {
            return "Yesterday";
        }

        return ts.ToString("dddd, MMMM d", CultureInfo.InvariantCulture);
    }

    private async Task SimulateTypingAsync()
    {
        if (typingCts is not null)
        {
            await typingCts.CancelAsync();
        }

        typingCts = new CancellationTokenSource();
        var token = typingCts.Token;

        try
        {
            await Task.Delay(800, token);
            var others = allUsers.Where(u => u.Id != currentUser.Id).ToList();
            if (others.Count == 0)
            {
                return;
            }

            typingUser = others[RandomIndex(others.Count)].DisplayName;
            await InvokeAsync(StateHasChanged);

            await Task.Delay(3000, token);
            typingUser = null;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            // Expected when navigating away or sending another message
        }
    }

    private static int RandomIndex(int maxValue)
    {
        return System.Security.Cryptography.RandomNumberGenerator.GetInt32(maxValue);
    }

    public void Dispose()
    {
        typingCts?.Dispose();
    }
}
