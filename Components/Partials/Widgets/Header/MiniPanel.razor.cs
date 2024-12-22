using Microsoft.AspNetCore.Components;
using Service.Core.Engine;

namespace Service.Components.Partials.Widgets.Header;

public class MiniPanelRazor : MyComponentBase
{
  [Parameter] public string Title { get; set; }
  [Parameter] public string Subtitle { get; set; }
  [Parameter] public string Slug { get; set; }
  [Parameter] public string Active { get; set; } = string.Empty;
}
