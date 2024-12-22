using Microsoft.AspNetCore.Components;

namespace Service.Components.Layout;

public class BlankLayout : LayoutComponentBase
{
  // protected override void BuildRenderTree(RenderTreeBuilder builder)
  // {
  //   // Start a div for the layout
  //   builder.OpenElement(0, "div");

  //   // Render the body of the layout
  //   builder.AddContent(1, RenderBody()); // This will render the content of @Body

  //   builder.CloseElement(); // Close the div
  // }

  // This method is used to render the body content
  private RenderFragment RenderBody()
  {
    return (builder) =>
    {
      builder.AddContent(0, Body); // Body is provided by LayoutComponentBase
    };
  }
}
