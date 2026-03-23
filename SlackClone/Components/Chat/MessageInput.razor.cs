namespace SlackClone.Components.Chat;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class MessageInput
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public string Placeholder { get; set; } = "Message";

    [Parameter]
    public EventCallback<string> OnSend { get; set; }

    private string MessageText { get; set; } = string.Empty;
    // ReSharper disable once NotAccessedField.Local
    private ElementReference inputRef;
    private bool showEmojiPicker;
    private bool showAttachDialog;

    private Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e is { Key: "Enter", ShiftKey: false })
        {
            return SendAsync();
        }
        return Task.CompletedTask;
    }

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
        {
            return;
        }

        var text = MessageText.Trim();
        MessageText = string.Empty;
        showEmojiPicker = false;
        await OnSend.InvokeAsync(text);
        await PlaySendSoundAsync();
    }

    private async Task PlaySendSoundAsync()
    {
        try
        {
            var script = "(function(){try{var a=new AudioContext(),o=a.createOscillator(),g=a.createGain();" +
                "o.type='sine';o.frequency.setValueAtTime(400,a.currentTime);" +
                "o.frequency.exponentialRampToValueAtTime(800,a.currentTime+0.08);" +
                "g.gain.setValueAtTime(0.12,a.currentTime);" +
                "g.gain.exponentialRampToValueAtTime(0.001,a.currentTime+0.1);" +
                "o.connect(g);g.connect(a.destination);o.start();o.stop(a.currentTime+0.1)}catch(e){}})()";
            await JS.InvokeVoidAsync("eval", script);
        }
        catch (JSDisconnectedException)
        {
            // Ignored during prerender or disconnect
        }
    }

    private void ToggleEmojiPicker()
    {
        showEmojiPicker = !showEmojiPicker;
        showAttachDialog = false;
    }

    private void CloseEmojiPicker()
    {
        showEmojiPicker = false;
    }

    private Task InsertEmojiAsync(string emoji)
    {
        MessageText += emoji;
        showEmojiPicker = false;
        return Task.CompletedTask;
    }

    private void ToggleAttachDialog()
    {
        showAttachDialog = !showAttachDialog;
        showEmojiPicker = false;
    }

    private Task InsertFormatAsync(string prefix, string suffix)
    {
        MessageText += prefix + "text" + suffix;
        return Task.CompletedTask;
    }

    private Task InsertMentionAsync()
    {
        MessageText += "@";
        return Task.CompletedTask;
    }
}
