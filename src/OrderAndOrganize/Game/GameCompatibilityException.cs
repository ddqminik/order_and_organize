using System;

namespace OrderAndOrganize.Game
{
    public class GameCompatibilityException : Exception
    {
        public string ExpectedType { get; }
        public string ExpectedMember { get; }
        public string ExpectedSignature { get; }
        public string ActualResult { get; }
        public string RecommendedAction { get; }

        public GameCompatibilityException(
            string expectedType,
            string expectedMember,
            string expectedSignature,
            string actualResult,
            string recommendedAction)
            : base($"Game compatibility failure: {expectedType}.{expectedMember} " +
                   $"(expected: {expectedSignature}, found: {actualResult}). " +
                   $"Action: {recommendedAction}")
        {
            ExpectedType = expectedType;
            ExpectedMember = expectedMember;
            ExpectedSignature = expectedSignature;
            ActualResult = actualResult;
            RecommendedAction = recommendedAction;
        }
    }
}
