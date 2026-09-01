namespace SlackClone.Components.Pages;

using System.Diagnostics;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

#pragma warning disable CA1716
public partial class Error
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }
    private bool ShowRequestId => !String.IsNullOrEmpty(RequestId);

    protected override void OnInitialized() =>
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}
#pragma warning restore CA1716
