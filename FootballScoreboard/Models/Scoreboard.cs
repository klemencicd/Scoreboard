﻿using FootballScoreboard.Interfaces;
using FootballScoreboard.Exceptions;
using FluentValidation.Results;

namespace FootballScoreboard.Models;

public class Scoreboard(IMatchRepository _matchRepository, IMatchValidator _matchValidator) : IScoreboard
{
    public Ulid StartMatch(string homeTeam, string awayTeam, DateTime matchStartTime)
    {
        Match match = new(Ulid.NewUlid(), homeTeam, awayTeam, matchStartTime);
        ValidationResult result = _matchValidator.ValidateStart(match, _matchRepository.GetAllActive());

        if (!result.IsValid)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(error => error.ErrorMessage));
            throw new ScoreboardException($"Match validation failed: {errorMessage}");
        }

        _matchRepository.Add(match);
        return match.Id;
    }

    public void UpdateScore(Ulid id, int homeTeamScore, int awayTeamScore)
    {
        Match? match = _matchRepository.GetSingle(id);
        if (match == null) return;

        match.UpdateScore(homeTeamScore, awayTeamScore);
        ValidationResult result = _matchValidator.ValidateScore(match);

        if (!result.IsValid)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(error => error.ErrorMessage));
            throw new ScoreboardException($"Match validation failed: {errorMessage}");
        }
    }

    public void FinishMatch(Ulid id)
    {
        Match? match = _matchRepository.GetSingle(id);
        if (match != null)
        {
            _matchRepository.Remove(match);
        }
    }

    public List<Match> GetMatches()
    {
        return _matchRepository.GetAllActive();
    }
}
