using System.ComponentModel.DataAnnotations;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardsWorker.Controllers;

[ApiController]
[Route("[controller]")]
public class ScoresController : ControllerBase
{
    private readonly IPlayerScoreRepository _playerScoreRepository;
    private readonly ILogger<ScoresController> _logger;

    public ScoresController(IPlayerScoreRepository playerScoreRepository, ILogger<ScoresController> logger)
    {
        _playerScoreRepository = playerScoreRepository;
        _logger = logger;
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<PlayerScore>> GetPlayerScore(int userId)
    {
        PlayerScore? playerScore;

        try
        {
            playerScore = await _playerScoreRepository.GetPlayerScoreAsync(userId, HttpContext.RequestAborted);
            if (playerScore == null)
            {
                return NotFound();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }

        return playerScore;
    }


    [HttpGet]
    public async Task<List<PlayerScore>> GetPlayerScores([Range(0, 1_000_000)] int offset = 0, [Range(1, 100)] int limit = 20)
    {
        List<PlayerScore> playerScores;
        try
        {
            playerScores = await _playerScoreRepository.GetPlayerScoresAsync(offset, limit, HttpContext.RequestAborted);
            return playerScores;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

}