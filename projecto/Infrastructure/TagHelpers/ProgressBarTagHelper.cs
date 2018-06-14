using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.Infrastructure.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "percentage")]
    public class ProgressBarTagHelper : TagHelper
    {
        public int Percentage { get; set; }
        public string Color { get; set; } = "#004D40";
        public string BackgroundColor { get; set; } = "#E0F2F1";
        public int Radius { get; set; }
        public int StrokeWidth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Percentage < 0)
            {
                Percentage = 0;
            }
            else if (Percentage > 100)
            {
                Percentage = 100;
            }

            TagBuilder svg = new TagBuilder("svg");
            svg.Attributes["width"] = $"{Width}px";
            svg.Attributes["height"] = $"{Height}px";
            svg.Attributes["version"] = "1.1";
            svg.Attributes["xmlns"] = "http://www.w3.org/2000/svg";

            TagBuilder background = new TagBuilder("circle");
            background.TagRenderMode = TagRenderMode.SelfClosing;
            background.Attributes["cx"] = $"{Width / 2}";
            background.Attributes["cy"] = $"{Height / 2}";
            background.Attributes["r"] = $"{Radius - StrokeWidth / 2}";
            background.Attributes["fill"] = "transparent";
            background.Attributes["stroke"] = BackgroundColor;
            background.Attributes["stroke-width"] = $"{StrokeWidth}px";
            svg.InnerHtml.AppendHtml(background);

            double length = 2 * Math.PI * (Radius - StrokeWidth / 2);
            double stroke = length * (Percentage * 0.01);
            double space = length * (1 - (Percentage * 0.01));

            TagBuilder circle = new TagBuilder("circle");
            circle.TagRenderMode = TagRenderMode.SelfClosing;
            circle.Attributes["cx"] = $"{Width / 2}";
            circle.Attributes["cy"] = $"{Height / 2}";
            circle.Attributes["r"] = $"{Radius - StrokeWidth / 2}";
            circle.Attributes["fill"] = "transparent";
            circle.Attributes["stroke"] = Percentage != 100 ? Color : "#B71C1C";
            circle.Attributes["stroke-width"] = $"{StrokeWidth}px";
            circle.Attributes["stroke-dasharray"] = $"{stroke} {space}";
            svg.InnerHtml.AppendHtml(circle);

            output.PreContent.AppendHtml(svg);
        }
    }
}
