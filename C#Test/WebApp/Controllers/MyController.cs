using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Services;
using System.Collections.Generic;
using SkiaSharp;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyController : ControllerBase
    {
        private readonly ITimeEntryService _entryService;

        public MyController(ITimeEntryService entryService)
        {
            _entryService = entryService;
        }

        [HttpGet("html")]
        public async Task<IActionResult> GetHtml()
        {
            try
            {
                var sorted = await _entryService.GetProcessedEntriesAsync();

                var chartBytes = GeneratePieChart(sorted);
                var base64 = Convert.ToBase64String(chartBytes);
                var chartHtml = $@" <div class='d'><img src='data:image/png;base64,{base64}' alt='Pie Chart'><br>
                        <a href='data:image/png;base64,{base64}'download='chart.png'><button class='b'>Download Chart</button></a></div>";
                var html = @"
                <html><head><title>C#Test - Aleksa Despotovic</title><link rel='stylesheet' href='/css/tablechart.css'></head><br><br><br><br>
                    <body><table><thead><tr><th>Name</th><th>Total time</th></tr></thead><tbody>";
                    foreach (var emp in sorted)
                    {
                        var name = emp.Key;
                        var hours = emp.Value;
                        var rowClass = hours < 100 ? " class='low-hours'" : "";

                        html += $"<tr{rowClass}><td>{name}</td><td>{hours:F2} hrs</td></tr>";
                    }
                    html += @" </tbody></table>";
                    html += chartHtml;
                    html += @" </body></html>";
                return Content(html, "text/html");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error during API call: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error during JSON parsing: {ex.Message}");
            }
        }

        private byte[] GeneratePieChart(List<KeyValuePair<string, double>> sorted)
        {
            int width = 660;
            int height = 500;
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            float centerX = width / 2f;
            float centerY = 200;
            float radius = 180;

            double total = sorted.Sum(x => x.Value);
            float startAngle = -90f;

            var colors = new[]
            {
                SKColor.Parse("#FF6384"), SKColor.Parse("#36A2EB"), SKColor.Parse("#FFCE56"),
                SKColor.Parse("#4BC0C0"), SKColor.Parse("#9966FF"), SKColor.Parse("#FF9F40"),
                SKColor.Parse("#66FF66"), SKColor.Parse("#FF66B2"), SKColor.Parse("#66B2FF"),
                SKColor.Parse("#FFB266"), SKColor.Parse("#B266FF"), SKColor.Parse("#00CC99")
            };

            var paint = new SKPaint { IsAntialias = true };
            var borderLinePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = SKColors.White
            };
            var font = new SKFont(SKTypeface.Default, 14);
            var percentFont = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White,
                Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold),
                TextSize = 14
            };
            var legendFont = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black,
                Typeface = SKTypeface.Default,
                TextSize = 14
            };
            int itemsPerRow = 3;
            int legendStartY = 400;
            int legendStartX = 100;
            int legendSpacingX = 160;
            int legendSpacingY = 20;
            int legendIndex = 0;

            int colorIndex = 0;

            foreach (var emp in sorted)
            {
                var value = emp.Value;
                var sweepAngle = (float)(value / total * 360);
                paint.Color = colors[colorIndex % colors.Length];

                var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
                canvas.DrawArc(rect, startAngle, sweepAngle, true, paint);


                float midAngle = startAngle + sweepAngle / 2;
                float rad = midAngle * (float)Math.PI / 180;
                float labelX = centerX + (float)(Math.Cos(rad) * radius / 1.4f);
                float labelY = centerY + (float)(Math.Sin(rad) * radius / 1.4f);

                var percent = (value / total * 100).ToString("0.00") + "%";
                var textWidth = percentFont.MeasureText(percent);
                canvas.DrawText(percent, labelX - textWidth / 2, labelY + 5, percentFont);

                float angleRad = startAngle * (float)Math.PI / 180f;

                float endX = centerX + (float)Math.Cos(angleRad) * radius;
                float endY = centerY + (float)Math.Sin(angleRad) * radius;

                canvas.DrawLine(centerX, centerY, endX, endY, borderLinePaint);

                int row = legendIndex / itemsPerRow;
                int col = legendIndex % itemsPerRow;
                int legendX = legendStartX + col * legendSpacingX;
                int legendY = legendStartY + row * legendSpacingY;

                canvas.DrawRect(legendX, legendY, 30, 10, paint);
                canvas.DrawText(emp.Key, legendX + 38, legendY + 10, legendFont);

                startAngle += sweepAngle;
                colorIndex++;
                legendIndex++;
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

    }
}
