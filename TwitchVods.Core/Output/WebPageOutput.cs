using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Output
{
    internal class WebPageOutput : IOutputWriter
    {
        private StreamWriter _writer;
        private readonly Channel _channel;
        private readonly Settings _settings;
        private string _outputFilePath;

        public WebPageOutput(Channel channel, Settings settings)
        {
            _channel = channel;
            _settings = settings;
            PopulateOutputFileName();
        }

        private void PopulateOutputFileName()
        {
            var outputFileName = $"{_channel.Name.ToLower()}.html";
            var outputFilePath = Path.Combine(_settings.OutputDir, outputFileName);
            _outputFilePath = outputFilePath;
        }

        public async Task WriteOutputAsync()
        {
            var channelName = _channel.Name.ToLower();

            using (_writer = new StreamWriter(new FileStream(_outputFilePath, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8))
            {
                await WriteHeaderAsync(channelName);

                foreach (var video in _channel.Videos)
                    await WriteContent(video);

                await WriteFooter();
            }
        }

        private async Task WriteHeaderAsync(string channelName)
        {
            var googleAnalytics = $"<script async src=\"https://www.googletagmanager.com/gtag/js?id={_settings.GoogleAnalyticsTrackingId}\"></script><script>window.dataLayer = window.dataLayer || [];function gtag(){{dataLayer.push(arguments);}}gtag('js', new Date());gtag('config', '{_settings.GoogleAnalyticsTrackingId}');</script>";

            await _writer.WriteAsync($"<html><head>{googleAnalytics}<title>{channelName} - Past Broadcasts</title>");
            await _writer.WriteAsync("<link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta.2/css/bootstrap.min.css\" integrity=\"sha384-PsH8R72JQ3SOdhVi3uxftmaW6Vc51MKb0q5P2rRUpPvrszuE4W1povHYgTpBfshb\" crossorigin=\"anonymous\">");
            await _writer.WriteAsync("<style> .datacontent a:visited { color: #FF0000; }</style>");
            await _writer.WriteAsync("</head><body><a href=\"#\" ID=\"backToTop\"></a>");

            await _writer.WriteAsync($"<div class=\"jumbotron\"><div class=\"container-fluid\"><h1>twitch.tv/{channelName} - past broadcasts</h1>");
            await _writer.WriteAsync($"<p>This page lists all {_channel.TotalVideoCount:n0} of the past broadcasts for the channel twitch.tv/{channelName}.</p>");
            await _writer.WriteAsync($"<p>Total video views: {_channel.TotalViewCount:n0}.</p>");
            await _writer.WriteAsync($"<p>Total games played: {_channel.TotalGamesCount:n0}.</p>");
            await _writer.WriteAsync($"<p>Total broadcast hours: {_channel.TotalBroadcastHours:n0} ({ _channel.TotalBroadcastDays} days).</p>");
            await _writer.WriteAsync($"<p>Average length of broadcast: {_channel.AverageBroadcastTime}.</p>");
            await _writer.WriteAsync($"<p><a class=\"btn btn-primary btn-lg\" role=\"button\" href=\"http://twitch.tv/{channelName}\">Go to twitch.tv/{channelName} &raquo;</a></p>");
            await _writer.WriteAsync($"<p><a href=\"{_channel.Name.ToLower()}.json\">Download the raw data from this page as JSON.</a></p>");
            await _writer.WriteAsync("<p><a href=\"https://github.com/RandomUser99/TwitchVods/\">Source code available on GitHub.</a></p>");

            if (!string.IsNullOrWhiteSpace(_settings.RedditUsername))
            {
                var redditLink = HttpUtility.HtmlEncode($"https://reddit.com/message/compose?to={_settings.RedditUsername}&subject=tvods");
                await _writer.WriteAsync($"<p><a href=\"{redditLink}\">Contact me on Reddit.</a></p>");
            }
            
            await _writer.WriteAsync("</div></div>");

            await _writer.WriteAsync("<div class=\"container-fluid datacontent\">");
            await _writer.WriteAsync("<div class=\"alert alert-danger\" role=\"alert\">You can filter by selecting a game from the list below or search for a game, date, day or anything you like!</div>");
            await _writer.WriteAsync(GetGamesList());
            await _writer.WriteAsync("</br>");
            await _writer.WriteAsync("<div class=\"input-group\"> <span class=\"input-group-addon input-lg\">Search</span><input id=\"filter\" type=\"text\" class=\"form-control input-lg\" placeholder=\"Type here to filter by game or any of the fields below ...\"></div></br>");
            await _writer.WriteAsync("<table class=\"table table-sm\">");
            await _writer.WriteAsync("<thead><tr><th scope=\"col\">Date</th><th scope=\"col\">Day</th><th scope=\"col\"> Time (UTC)</th><th scope=\"col\">Game</th><th scope=\"col\">Title</th><th scope=\"col\">Runtime</th><th scope=\"col\">Views</th></tr></thead><tbody class=\"searchable\">");
        }

        private async Task WriteContent(Video video)
        {
            await _writer.WriteAsync("<tr class=\"table-primary\">");

            await _writer.WriteAsync($"<td>{video.CreatedAt.ToUniversalTime():yyyy-MM-dd}</td>");
            await _writer.WriteAsync($"<td>{video.CreatedAt.DayOfWeek}</td>");
            await _writer.WriteAsync($"<td>{video.CreatedAt.ToUniversalTime():HH: mm}</td>");
            await _writer.WriteAsync($"<td>{video.Game}</td>");
            await _writer.WriteAsync($"<td><a href=\"{video.Url}\">{video.Title}</a></td>");
            await _writer.WriteAsync($"<td>{video.RunTime}</td>");
            await _writer.WriteAsync($"<td>{video.Views}</td>");

            await _writer.WriteAsync("</tr>");

            if (!video.Markers.Any())
                return;

            foreach (var marker in video.Markers)
            {
                await _writer.WriteAsync("<tr class=\"table-secondary\">");
                await _writer.WriteAsync($"<td>{video.CreatedAt.ToUniversalTime():yyyy-MM-dd}</td>");
                await _writer.WriteAsync($"<td>{video.CreatedAt.DayOfWeek}</td>");
                await _writer.WriteAsync($"<td>{video.CreatedAt.AddSeconds(marker.TimeSeconds).ToUniversalTime():HH: mm}</td>");
                await _writer.WriteAsync($"<td>{marker.Label}</td>");

                await _writer.WriteAsync($"<td><a href=\"{video.Url}?t={marker.TimeQueryString}\">Game change</a></td>");
                await _writer.WriteAsync("<td>&nbsp;</td>");
                await _writer.WriteAsync("<td>&nbsp;</td>");

                await _writer.WriteAsync("</tr>");
            }
        }

        private async Task WriteFooter()
        {
            await _writer.WriteAsync($"</tbody></table><p>Page generated: {DateTime.UtcNow:yyyy-MM-dd HH\\:mm\\:ss} UTC</p>");
            await _writer.WriteAsync("</div>");
            await _writer.WriteAsync("<script src=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/js/bootstrap.min.js\"></script>");
            await _writer.WriteAsync("<script src=\"https://code.jquery.com/jquery-2.1.1.min.js\"></script>");

            await _writer.WriteAsync("<script type=\"text/javascript\">");

            // Filter javascript
            await _writer.WriteAsync("$(document).ready(function(){!function(e){e(\"#filter\").keyup(function(){var t=new RegExp(e(this).val(),\"i\");e(\".searchable tr\").hide(),e(\".searchable tr\").filter(function(){return t.test(e(this).text())}).show()}),e(\"#gamesList\").change(function(){var t=e(\"option:selected\",this).text(),r=new RegExp(t);\"/View all games/\"==r?(console.log(\"show all\"),e(\".searchable tr\").show()):(e(\".searchable tr\").hide(),e(\".searchable tr\").filter(function(){return r.test(e(this).text())}).show())})}(jQuery)});");
            await _writer.WriteAsync("</script>");
            await _writer.WriteAsync("</body></html>");
        }

        private string GetGamesList()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("<div class=\"input-group\"><span class=\"input-group-addon input-lg\">Select a game</span>");
            stringBuilder.Append("<select id=\"gamesList\" class=\"form-control\">");
            stringBuilder.Append($"<option selected>View all games</option>");

            foreach (var game in _channel.Games)
                stringBuilder.Append($"<option>{game}</option>");

            stringBuilder.Append("</select></div>");

            return stringBuilder.ToString();
        }
    }
}
