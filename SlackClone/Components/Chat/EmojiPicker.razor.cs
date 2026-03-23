namespace SlackClone.Components.Chat;

using Microsoft.AspNetCore.Components;

public partial class EmojiPicker
{
    [Parameter]
    public EventCallback<string> OnSelect { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private string activeCategory = "Frequently Used";
    private string search = string.Empty;

    private sealed record EmojiCategory(string Name, string Icon, string[] Emojis);

    private static readonly EmojiCategory[] Categories =
    [
        new("Frequently Used", "🕐", ["👍", "❤️", "😂", "🎉", "👀", "🚀", "💯", "✅", "🔥", "👏", "🙌", "💪", "🙏", "⭐", "💡", "✨"]),
        new("Smileys", "😀", ["😀", "😊", "😄", "😁", "🤣", "😅", "😆", "😉", "😍", "🥰", "😘", "😋", "😛", "🤔", "🤨", "😐", "😑", "🙄", "😏", "😥", "😮", "😯", "😴", "🤤", "😌", "🤗", "🤩", "😎", "🥳", "😤", "😡", "🥺"]),
        new("People", "👋", ["👋", "🤚", "✋", "👌", "✌️", "🤞", "🤟", "🤘", "🤙", "👈", "👉", "👆", "👇", "👍", "👎", "✊", "👊", "🤛", "🤜", "👏", "🙌", "🤝", "🙏", "💪", "🤷", "🤦", "💁", "🙋", "🙆", "🙅"]),
        new("Nature", "🌿", ["🌞", "🌙", "⭐", "🔥", "💧", "🌊", "🌈", "☁️", "⛈️", "❄️", "🌸", "🌺", "🌻", "🌿", "🍀", "🍃", "🐱", "🐶", "🐼", "🦊", "🦁", "🐸", "🐧", "🦋", "🐝"]),
        new("Food", "🍔", ["🍎", "🍊", "🍋", "🍇", "🍓", "🍑", "🥑", "🍔", "🍕", "🍜", "🍣", "🍰", "🍩", "🍪", "☕", "🍵", "🧋", "🍺", "🥂", "🧃"]),
        new("Objects", "💼", ["❤️", "💔", "💬", "💭", "📌", "📎", "✏️", "📝", "📅", "⏰", "🔔", "📢", "💻", "📱", "💼", "📧", "📁", "🗂️", "📊", "📈", "🔑", "🔒", "🛠️", "⚙️", "🎯", "🏆", "🎨", "🎵", "📸", "🎬"])
    ];

    private async Task SelectAsync(string emoji)
    {
        await OnSelect.InvokeAsync(emoji);
        await OnClose.InvokeAsync();
    }

    private Task CloseAsync()
    {
        return OnClose.InvokeAsync();
    }
}
