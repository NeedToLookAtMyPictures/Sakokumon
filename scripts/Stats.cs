using Godot;
using System.Text;

public partial class Stats : RichTextLabel
{
    public override void _Ready()
    {
        var global = GetNode<Global>("/root/Global");
        var sb     = new StringBuilder();

        sb.AppendLine($"Day {global.DayNumber}  ·  Year {global.DayYear}");
        sb.AppendLine();

        if (global.DayNpcQuota > 0)
        {
            var r = global.CurrentDayRecord;
            sb.AppendLine($"Total inspected:   {r.Total} / {global.DayNpcQuota}");
            sb.AppendLine($"Allowed passage:   {r.Allowed}");
            sb.AppendLine($"Detained:          {r.Detained}");
        }
        else
        {
            sb.AppendLine("No day data recorded.");
        }

        Text = sb.ToString();
    }
}
