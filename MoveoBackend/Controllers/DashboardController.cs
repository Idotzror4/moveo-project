using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoveoBackend.Data;
using MoveoBackend.Models;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace MoveoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public DashboardController(IHttpClientFactory httpClientFactory, ApplicationDbContext context, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _configuration = configuration;
        }

        private async Task<List<string>> GetUserContentTypesAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return new List<string>();
            }

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preferences == null || string.IsNullOrEmpty(preferences.ContentTypes))
            {
                return new List<string>();
            }

            try
            {
                var contentTypes = JsonSerializer.Deserialize<List<string>>(preferences.ContentTypes);
                return contentTypes ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        [HttpGet("news")]
        public async Task<ActionResult> GetMarketNews()
        {
            var today = DateTime.UtcNow.Date;
            
            var cachedContent = await _context.DailyContents
                .FirstOrDefaultAsync(dc => dc.ContentType == "News" && dc.Date == today);
            
            if (cachedContent != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(cachedContent.ContentData);
                    var results = doc.RootElement.GetProperty("results");
                    bool hasValidUrls = false;
                    bool hasOldLink = false;
                    
                    foreach (var item in results.EnumerateArray())
                    {
                        if (item.TryGetProperty("url", out var urlElement))
                        {
                            var url = urlElement.GetString();
                            if (url?.StartsWith("http") == true)
                            {
                                hasValidUrls = true;
                                if (url == "https://www.coindesk.com/bitcoin")
                                {
                                    hasOldLink = true;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (hasOldLink || !hasValidUrls)
                    {
                        _context.DailyContents.Remove(cachedContent);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return Content(cachedContent.ContentData, "application/json");
                    }
                    
                    _context.DailyContents.Remove(cachedContent);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    _context.DailyContents.Remove(cachedContent);
                    await _context.SaveChangesAsync();
                }
            }
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetAsync("https://cryptopanic.com/api/v1/posts/?auth_token=demo&public=true");
                
                string jsonString;
                if (response.IsSuccessStatusCode)
                {
                    jsonString = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    jsonString = JsonSerializer.Serialize(new
                    {
                        results = new[]
                        {
                            new { title = "Bitcoin reaches new high", url = "https://www.coindesk.com/tag/bitcoin/", published_at = DateTime.UtcNow.ToString() },
                            new { title = "Ethereum upgrade successful", url = "https://ethereum.org/upgrades/", published_at = DateTime.UtcNow.ToString() },
                            new { title = "Crypto market shows bullish trends", url = "https://www.coindesk.com/markets/", published_at = DateTime.UtcNow.ToString() }
                        }
                    });
                }
                
                var dailyContent = new DailyContent
                {
                    ContentType = "News",
                    ContentData = jsonString,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DailyContents.Add(dailyContent);
                await _context.SaveChangesAsync();
                
                return Content(jsonString, "application/json");
            }
            catch
            {
                var fallbackJson = JsonSerializer.Serialize(new
                {
                    results = new[]
                    {
                        new { title = "Bitcoin reaches new high", url = "https://www.coindesk.com/tag/bitcoin/", published_at = DateTime.UtcNow.ToString() },
                        new { title = "Ethereum upgrade successful", url = "https://ethereum.org/upgrades/", published_at = DateTime.UtcNow.ToString() },
                        new { title = "Crypto market shows bullish trends", url = "https://www.coindesk.com/markets/", published_at = DateTime.UtcNow.ToString() }
                    }
                });
                
                var dailyContent = new DailyContent
                {
                    ContentType = "News",
                    ContentData = fallbackJson,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DailyContents.Add(dailyContent);
                await _context.SaveChangesAsync();
                
                return Content(fallbackJson, "application/json");
            }
        }

        [HttpGet("prices")]
        public async Task<ActionResult> GetCoinPrices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var coinMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Bitcoin", "bitcoin" },
                { "Ethereum", "ethereum" },
                { "Solana", "solana" },
                { "Cardano", "cardano" },
                { "Polygon", "matic-network" }
            };

            var defaultCoins = new[] { "bitcoin", "ethereum", "solana" };
            var coinIds = defaultCoins.ToList();

            if (preferences != null && !string.IsNullOrEmpty(preferences.InterestedAssets))
            {
                try
                {
                    var selectedAssets = JsonSerializer.Deserialize<List<string>>(preferences.InterestedAssets);
                    if (selectedAssets != null && selectedAssets.Any())
                    {
                        coinIds = selectedAssets
                            .Where(asset => coinMapping.ContainsKey(asset))
                            .Select(asset => coinMapping[asset])
                            .ToList();
                        
                        if (!coinIds.Any())
                        {
                            coinIds = defaultCoins.ToList();
                        }
                    }
                }
                catch
                {
                    coinIds = defaultCoins.ToList();
                }
            }
            var coinIdsString = string.Join(",", coinIds);
            var apiUrl = $"https://api.coingecko.com/api/v3/simple/price?ids={coinIdsString}&vs_currencies=usd";
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(apiUrl);
                
                object data;
                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadFromJsonAsync<object>();
                }
                else
                {
                    var fallbackData = new Dictionary<string, object>();
                    foreach (var coinId in coinIds)
                    {
                        fallbackData[coinId] = new { usd = coinId == "bitcoin" ? 45000 : coinId == "ethereum" ? 3000 : coinId == "solana" ? 150 : coinId == "cardano" ? 0.5 : 0.8 };
                    }
                    data = fallbackData;
                }
                
                return Ok(data);
            }
            catch
            {
                var fallbackData = new Dictionary<string, object>();
                foreach (var coinId in coinIds)
                {
                    fallbackData[coinId] = new { usd = coinId == "bitcoin" ? 45000 : coinId == "ethereum" ? 3000 : coinId == "solana" ? 150 : coinId == "cardano" ? 0.5 : 0.8 };
                }
                
                return Ok(fallbackData);
            }
        }

        [HttpGet("ai-insight")]
        public async Task<ActionResult> GetAIInsight()
        {
            var contentTypes = await GetUserContentTypesAsync();
            bool prefersCharts = contentTypes.Contains("Charts", StringComparer.OrdinalIgnoreCase);
            bool prefersFun = contentTypes.Contains("Fun", StringComparer.OrdinalIgnoreCase);
            
            string prompt;
            if (prefersCharts)
            {
                prompt = "Give a brief technical and financial analysis (2-3 sentences) about current crypto market trends, focusing on price movements, technical indicators, and trading opportunities for Bitcoin and Ethereum.";
            }
            else if (prefersFun)
            {
                prompt = "Give a fun and lighthearted crypto market insight (2-3 sentences) with a humorous tone about current trends. Make it entertaining while mentioning Bitcoin, Ethereum, and general market sentiment.";
            }
            else
            {
                prompt = "Give a brief daily crypto market insight (2-3 sentences) about current trends, focusing on Bitcoin, Ethereum, and general market sentiment.";
            }
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var openRouterApiKey = _configuration["OpenRouter:ApiKey"];

                object insightData;
                
                if (!string.IsNullOrEmpty(openRouterApiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openRouterApiKey}");
                    client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000");
                    client.DefaultRequestHeaders.Add("X-Title", "Moveo Crypto Advisor");

                    var requestBody = new
                    {
                        model = "openai/gpt-3.5-turbo",
                        messages = new[]
                        {
                            new { role = "user", content = prompt }
                        },
                        max_tokens = 150
                    };

                    var response = await client.PostAsJsonAsync(
                        "https://openrouter.ai/api/v1/chat/completions",
                        requestBody
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                        if (result.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var insight = choices[0].GetProperty("message").GetProperty("content").GetString();
                            insightData = new { insight = insight };
                        }
                        else
                        {
                            throw new Exception("Invalid response from OpenRouter");
                        }
                    }
                    else
                    {
                        throw new Exception("OpenRouter API failed");
                    }
                }
                else
                {
                    string[] fallbackInsights;
                    if (prefersCharts)
                    {
                        fallbackInsights = new[]
                        {
                            "Technical analysis shows Bitcoin testing key resistance levels around $45,000. Ethereum's RSI indicates potential bullish momentum. Watch for breakout patterns and volume confirmation.",
                            "Market structure suggests consolidation phase. Bitcoin's support at $42,000 remains strong while Ethereum shows relative strength. Consider range trading strategies.",
                            "Price action indicates potential trend reversal. Bitcoin's moving averages are converging, suggesting volatility ahead. Ethereum's chart shows bullish divergence."
                        };
                    }
                    else if (prefersFun)
                    {
                        fallbackInsights = new[]
                        {
                            "Bitcoin is mooning again! 🚀 Ethereum is doing its thing, and the crypto market is basically a rollercoaster that never stops. HODL strong, my friends!",
                            "The crypto market is wilder than a rodeo! Bitcoin's doing its best impression of a rocket ship, and Ethereum is just chilling like a boss. Buckle up!",
                            "Crypto markets are more unpredictable than the weather! Bitcoin's up, Ethereum's vibing, and we're all just here for the ride. To the moon! 🌙"
                        };
                    }
                    else
                    {
                        fallbackInsights = new[]
                        {
                            "The crypto market is showing strong momentum. Bitcoin and Ethereum continue to lead, with growing institutional adoption. Consider diversifying your portfolio and staying updated with market trends.",
                            "Market volatility remains a key factor in crypto investing. Bitcoin's dominance continues while altcoins show mixed signals. Stay informed and invest responsibly.",
                            "Crypto markets are experiencing increased activity. Ethereum's ecosystem growth and Bitcoin's store-of-value narrative remain strong. Keep an eye on regulatory developments."
                        };
                    }

                    var random = new Random();
                    insightData = new { insight = fallbackInsights[random.Next(fallbackInsights.Length)] };
                }
                
                return Ok(insightData);
            }
            catch
            {
                string fallbackInsight;
                if (prefersCharts)
                {
                    fallbackInsight = "Technical analysis shows Bitcoin testing key resistance levels. Ethereum's RSI indicates potential bullish momentum. Watch for breakout patterns.";
                }
                else if (prefersFun)
                {
                    fallbackInsight = "Bitcoin is mooning again! 🚀 Ethereum is doing its thing, and the crypto market is basically a rollercoaster. HODL strong!";
                }
                else
                {
                    fallbackInsight = "The crypto market is showing strong momentum. Bitcoin and Ethereum continue to lead, with growing institutional adoption.";
                }
                
                var fallbackData = new
                {
                    insight = fallbackInsight
                };
                
                return Ok(fallbackData);
            }
        }

        [HttpGet("meme")]
        public async Task<ActionResult> GetMeme()
        {
            var contentTypes = await GetUserContentTypesAsync();
            bool prefersFun = contentTypes.Contains("Fun", StringComparer.OrdinalIgnoreCase);
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("User-Agent", "MoveoCryptoApp/1.0");
                client.Timeout = TimeSpan.FromSeconds(10);
                
                string subreddit = prefersFun ? "cryptomemes" : "cryptocurrency";
                var response = await client.GetAsync($"https://www.reddit.com/r/{subreddit}/hot.json?limit=25");
                
                object memeData;
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    
                    var posts = doc.RootElement
                        .GetProperty("data")
                        .GetProperty("children")
                        .EnumerateArray()
                        .Select(child => child.GetProperty("data"))
                        .Where(post => 
                            post.TryGetProperty("post_hint", out var hint) && 
                            hint.GetString() == "image" &&
                            post.TryGetProperty("url_overridden_by_dest", out var url) &&
                            !string.IsNullOrEmpty(url.GetString())
                        )
                        .Take(10)
                        .Select(post => new
                        {
                            title = post.GetProperty("title").GetString() ?? "",
                            url = post.GetProperty("url_overridden_by_dest").GetString() ?? ""
                        })
                        .ToList();
                    
                    if (posts.Any())
                    {
                        var random = new Random();
                        var selectedMeme = posts[random.Next(posts.Count)];
                        memeData = selectedMeme;
                    }
                    else
                    {
                        memeData = GetFallbackMeme(prefersFun);
                    }
                }
                else
                {
                    memeData = GetFallbackMeme(prefersFun);
                }
                
                return Ok(memeData);
            }
            catch
            {
                var fallbackMeme = GetFallbackMeme(prefersFun);
                return Ok(fallbackMeme);
            }
        }
        
        private object GetFallbackMeme(bool prefersFun)
        {
            if (prefersFun)
            {
                var funMemes = new[]
                {
                    new { title = "HODL!", url = "" },
                    new { title = "To the moon!", url = "" },
                    new { title = "Diamond hands!", url = "" }
                };
                var random = new Random();
                return funMemes[random.Next(funMemes.Length)];
            }
            else
            {
                var seriousMemes = new[]
                {
                    new { title = "Stay informed", url = "" },
                    new { title = "Do your own research", url = "" },
                    new { title = "Invest wisely", url = "" }
                };
                var random = new Random();
                return seriousMemes[random.Next(seriousMemes.Length)];
            }
        }
    }
}