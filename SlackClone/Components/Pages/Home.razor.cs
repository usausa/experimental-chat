namespace SlackClone.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public partial class Home
{
    private const string KeydownListenerScript = """
        document.addEventListener('keydown', function(e) {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                var input = document.querySelector('.sidebar-search-box input');
                if (input) input.focus();
            }
        });
        """;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private string selectedChannelId = "ch-general";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await JS.InvokeVoidAsync("eval", KeydownListenerScript);
            }
            catch (JSDisconnectedException)
            {
                // Ignored during prerender
            }
        }
    }

    private void HandleChannelSelected(string channelId)
    {
        selectedChannelId = channelId;
    }
}
