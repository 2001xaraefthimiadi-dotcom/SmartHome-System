using Microsoft.Maui.Graphics;
using SmartHome.Mobile.Models;

namespace SmartHome.Mobile;

public class EnergyChartDrawable : IDrawable
{
    public List<EnergyLogModel> Logs { get; set; } = [];

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Logs == null || Logs.Count == 0)
        {
            canvas.FontColor = Colors.Gray;
            canvas.FontSize = 14;

            canvas.DrawString(
                "Δεν υπάρχουν διαθέσιμα δεδομένα ενέργειας",
                dirtyRect,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);

            return;
        }

        var logs = Logs
            .OrderBy(x => x.RecordedAt)
            .TakeLast(10)
            .ToList();

        float leftPadding = 45;
        float rightPadding = 15;
        float topPadding = 20;
        float bottomPadding = 35;

        float chartWidth =
            dirtyRect.Width - leftPadding - rightPadding;

        float chartHeight =
            dirtyRect.Height - topPadding - bottomPadding;

        var maxValue =
            (float)Math.Max(logs.Max(x => x.ConsumedWatts), 1);

        // Axes
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;

        canvas.DrawLine(
            leftPadding,
            topPadding,
            leftPadding,
            topPadding + chartHeight);

        canvas.DrawLine(
            leftPadding,
            topPadding + chartHeight,
            leftPadding + chartWidth,
            topPadding + chartHeight);

        // Max value label
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 11;

        canvas.DrawString(
            $"{maxValue:N1} W",
            0,
            topPadding - 5,
            leftPadding - 5,
            20,
            HorizontalAlignment.Right,
            VerticalAlignment.Center);

        canvas.DrawString(
            "0 W",
            0,
            topPadding + chartHeight - 10,
            leftPadding - 5,
            20,
            HorizontalAlignment.Right,
            VerticalAlignment.Center);

        if (logs.Count == 1)
        {
            float x = leftPadding + chartWidth / 2;

            float y =
                topPadding +
                chartHeight -
                ((float)logs[0].ConsumedWatts / maxValue * chartHeight);

            canvas.FillColor = Colors.Blue;

            canvas.FillCircle(x, y, 5);

            return;
        }

        float stepX =
            chartWidth / (logs.Count - 1);

        var points = new List<PointF>();

        for (int i = 0; i < logs.Count; i++)
        {
            float x =
                leftPadding + i * stepX;

            float y =
                topPadding +
                chartHeight -
                ((float)logs[i].ConsumedWatts /
                 maxValue * chartHeight);

            points.Add(new PointF(x, y));
        }

        // Line
        canvas.StrokeColor = Colors.Blue;
        canvas.StrokeSize = 3;

        for (int i = 0; i < points.Count - 1; i++)
        {
            canvas.DrawLine(
                points[i].X,
                points[i].Y,
                points[i + 1].X,
                points[i + 1].Y);
        }

        // Points
        canvas.FillColor = Colors.Blue;

        foreach (var point in points)
        {
            canvas.FillCircle(
                point.X,
                point.Y,
                4);
        }

        // Time labels
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;

        var firstDate =
            logs.First().RecordedAt.ToLocalTime();

        var lastDate =
            logs.Last().RecordedAt.ToLocalTime();

        canvas.DrawString(
            firstDate.ToString("HH:mm"),
            leftPadding,
            topPadding + chartHeight + 8,
            60,
            20,
            HorizontalAlignment.Left,
            VerticalAlignment.Center);

        canvas.DrawString(
            lastDate.ToString("HH:mm"),
            leftPadding + chartWidth - 60,
            topPadding + chartHeight + 8,
            60,
            20,
            HorizontalAlignment.Right,
            VerticalAlignment.Center);
    }
}