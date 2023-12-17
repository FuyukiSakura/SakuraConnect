
using System.Text;

namespace Sakura.Live.Connect.Dreamer.Models.Games.DND
{
    public class GameData
    {
        public GameSetup GameSetup { get; set; }
        public List<Character> Characters { get; set; }
        public List<SceneOutcome> SceneOutcomes { get; set; }

        public static string CreateGameSummary(GameData gameData)
        {
            if (gameData == null)
                return "No game data available.";

            var builder = new StringBuilder();

            // Game Setup and Progress
            builder.AppendLine($"Initial Scene: {gameData.GameSetup.InitialScene}");
            builder.AppendLine($"Current Scene: {gameData.GameSetup.Progress.CurrentScene}");
            builder.AppendLine($"Next Steps: {gameData.GameSetup.Progress.NextSteps}");

            // Characters
            builder.AppendLine("\nCharacters:");
            foreach (var character in gameData.Characters)
            {
                builder.AppendLine($"- Name: {character.Name}, Race: {character.Race}, Class: {character.Class.Name}");
                builder.AppendLine($"  Level: {character.Class.Level}, HP Base: {character.Class.HitPointBase}");
                
                // Ability Scores and Skills
                builder.Append("  Ability Scores: ");
                foreach (var score in character.AbilityScores)
                    builder.Append($"{score.Key}: {score.Value}, ");
                builder.AppendLine();

                builder.Append("  Skills: ");
                foreach (var skill in character.Skills)
                    builder.Append($"{skill.Key}: [{string.Join(", ", skill.Value)}], ");
                builder.AppendLine();

                // Equipment
                builder.Append("  Equipment: ");
                foreach (var equip in character.Equipment)
                    builder.Append($"{equip.Key}: [{string.Join(", ", equip.Value)}], ");
                builder.AppendLine();

                // Actions and Dice Checks
                if (character.Actions.Any())
                {
                    builder.AppendLine("  Actions:");
                    foreach (var action in character.Actions)
                        builder.AppendLine($"    - {action.Description}: {action.Effect}");
                }

                if (character.DiceChecks.Any())
                {
                    builder.AppendLine("  Dice Checks:");
                    foreach (var check in character.DiceChecks)
                        builder.AppendLine($"    - {check.CheckName}: Roll {check.Roll}, Result: {check.Result}");
                }
            }

            // Scene Outcomes
            if (gameData.SceneOutcomes.Any())
            {
                builder.AppendLine("\nScene Outcomes:");
                foreach (var outcome in gameData.SceneOutcomes)
                {
                    builder.AppendLine($"- Description: {outcome.SceneDescription}");
                    foreach (var effect in outcome.EffectsOnCharacters)
                        builder.AppendLine($"  - {effect.CharacterName}: {effect.Effect}");
                }
            }

            return builder.ToString();
        }
    }

    public class GameSetup
    {
        public string InitialScene { get; set; }
        public Progress Progress { get; set; }
    }

    public class Progress
    {
        public string CurrentScene { get; set; }
        public string NextSteps { get; set; }
    }

    public class Character
    {
        public string Name { get; set; }
        public string Race { get; set; }
        public string Subrace { get; set; }
        public PlayerClass Class { get; set; }
        public Dictionary<string, int> AbilityScores { get; set; }
        public Dictionary<string, string> Modifiers { get; set; }
        public Dictionary<string, List<string>> Skills { get; set; }
        public Dictionary<string, List<string>> Equipment { get; set; }
        public List<string> Notes { get; set; }
        public List<PlayerAction> Actions { get; set; }
        public List<DiceCheck> DiceChecks { get; set; }
    }

    public class PlayerClass
    {
        public string Name { get; set; }
        public string Subclass { get; set; }
        public int Level { get; set; }
        public int HitPointBase { get; set; }
    }

    public class PlayerAction
    {
        public string Description { get; set; }
        public string Effect { get; set; }
    }

    public class DiceCheck
    {
        public string CheckName { get; set; }
        public int Roll { get; set; }
        public string Result { get; set; }
    }

    public class SceneOutcome
    {
        public string SceneDescription { get; set; }
        public List<EffectOnCharacter> EffectsOnCharacters { get; set; }
    }

    public class EffectOnCharacter
    {
        public string CharacterName { get; set; }
        public string Effect { get; set; }
    }
}
