namespace SlackClone.Components.Chat;

using System.Globalization;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using SlackClone.Models;

public partial class MessageItem
{
    [Parameter]
    [EditorRequired]
    public ChatMessage Message { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public ChatUser Author { get; set; } = default!;

    [Parameter]
    public bool IsFirstInGroup { get; set; } = true;

    [Parameter]
    public string CurrentUserId { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> OnReactionClick { get; set; }

    [Parameter]
    public EventCallback<string> OnThreadOpen { get; set; }

    [Parameter]
    public EventCallback<(string Id, string Content)> OnEdit { get; set; }

    [Parameter]
    public EventCallback<string> OnDelete { get; set; }

    [Parameter]
    public EventCallback<string> OnProfileOpen { get; set; }

    [Parameter]
    public EventCallback<string> OnBookmark { get; set; }

    [Parameter]
    public EventCallback<string> OnPin { get; set; }

    [Parameter]
    public bool IsBookmarked { get; set; }

    private bool showReactionPicker;
    private bool showMoreMenu;
    private bool isEditing;
    private string editText = string.Empty;

    private void ToggleReactionPicker()
    {
        showReactionPicker = !showReactionPicker;
        showMoreMenu = false;
    }

    private void ToggleMoreMenu()
    {
        showMoreMenu = !showMoreMenu;
        showReactionPicker = false;
    }

    private Task HandlePin()
    {
        showMoreMenu = false;
        return OnPin.InvokeAsync(Message.Id);
    }

    private Task HandleUnpin()
    {
        showMoreMenu = false;
        return OnPin.InvokeAsync(Message.Id);
    }

    private Task AddReactionAsync(string emoji)
    {
        showReactionPicker = false;
        return OnReactionClick.InvokeAsync(emoji);
    }

    private void StartEdit()
    {
        editText = Message.Content;
        isEditing = true;
        showMoreMenu = false;
    }

    private void CancelEdit()
    {
        isEditing = false;
    }

    private async Task SaveEditAsync()
    {
        if (!string.IsNullOrWhiteSpace(editText))
        {
            await OnEdit.InvokeAsync((Message.Id, editText));
        }

        isEditing = false;
    }

    private Task HandleEditKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            CancelEdit();
        }

        if (e is { Key: "Enter", ShiftKey: false })
        {
            return SaveEditAsync();
        }
        return Task.CompletedTask;
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
}
