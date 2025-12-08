using System.Text.Json.Serialization;

namespace AiTranslator.Models;

/// <summary>
/// Complete response from Grammar Learner API
/// </summary>
public class GrammarLearnerResponse
{
    [JsonPropertyName("originalText")]
    public string OriginalText { get; set; } = string.Empty;
    
    [JsonPropertyName("correctedText")]
    public string CorrectedText { get; set; } = string.Empty;
    
    [JsonPropertyName("fullTranslationFa")]
    public string FullTranslationFa { get; set; } = string.Empty;
    
    [JsonPropertyName("learningTipsFa")]
    public string LearningTipsFa { get; set; } = string.Empty;
    
    [JsonPropertyName("grammarTeaching")]
    public GrammarTeaching GrammarTeaching { get; set; } = new();
    
    [JsonPropertyName("idiomPhrases")]
    public List<IdiomPhrase> IdiomPhrases { get; set; } = new();
    
    [JsonPropertyName("sentenceStructure")]
    public List<SentenceStructure> SentenceStructure { get; set; } = new();
}

public class GrammarTeaching
{
    [JsonPropertyName("overviewEn")]
    public string OverviewEn { get; set; } = string.Empty;
    
    [JsonPropertyName("overviewFa")]
    public string OverviewFa { get; set; } = string.Empty;
    
    [JsonPropertyName("sentenceTenseEn")]
    public string SentenceTenseEn { get; set; } = string.Empty;
    
    [JsonPropertyName("sentenceTenseFa")]
    public string SentenceTenseFa { get; set; } = string.Empty;
    
    [JsonPropertyName("tenseExplanationFa")]
    public string TenseExplanationFa { get; set; } = string.Empty;
    
    [JsonPropertyName("structurePatternEn")]
    public string StructurePatternEn { get; set; } = string.Empty;
    
    [JsonPropertyName("structurePatternFa")]
    public string StructurePatternFa { get; set; } = string.Empty;
    
    [JsonPropertyName("difficultyLevel")]
    public string DifficultyLevel { get; set; } = "A1";
    
    [JsonPropertyName("similarExamples")]
    public List<SimilarExample> SimilarExamples { get; set; } = new();
    
    [JsonPropertyName("keyPoints")]
    public List<KeyPoint> KeyPoints { get; set; } = new();
    
    [JsonPropertyName("commonMistakes")]
    public List<CommonMistake> CommonMistakes { get; set; } = new();
}

public class SimilarExample
{
    [JsonPropertyName("exampleEn")]
    public string ExampleEn { get; set; } = string.Empty;
    
    [JsonPropertyName("exampleFa")]
    public string ExampleFa { get; set; } = string.Empty;
}

public class KeyPoint
{
    [JsonPropertyName("titleEn")]
    public string TitleEn { get; set; } = string.Empty;
    
    [JsonPropertyName("explanationFa")]
    public string ExplanationFa { get; set; } = string.Empty;
}

public class CommonMistake
{
    [JsonPropertyName("originalSegmentEn")]
    public string OriginalSegmentEn { get; set; } = string.Empty;
    
    [JsonPropertyName("correctedSegmentEn")]
    public string CorrectedSegmentEn { get; set; } = string.Empty;
    
    [JsonPropertyName("explanationFa")]
    public string ExplanationFa { get; set; } = string.Empty;
}

public class IdiomPhrase
{
    [JsonPropertyName("phraseEn")]
    public string PhraseEn { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "idiom"; // idiom, phrasalVerb, collocation, fixedExpression
    
    [JsonPropertyName("meaningFa")]
    public string MeaningFa { get; set; } = string.Empty;
    
    [JsonPropertyName("explanationFa")]
    public string ExplanationFa { get; set; } = string.Empty;
    
    [JsonPropertyName("exampleEn")]
    public string ExampleEn { get; set; } = string.Empty;
    
    [JsonPropertyName("exampleFa")]
    public string ExampleFa { get; set; } = string.Empty;
}

public class SentenceStructure
{
    [JsonPropertyName("sentenceIndex")]
    public int SentenceIndex { get; set; }
    
    [JsonPropertyName("sentenceText")]
    public string SentenceText { get; set; } = string.Empty;
    
    [JsonPropertyName("sentenceTranslationFa")]
    public string SentenceTranslationFa { get; set; } = string.Empty;
    
    [JsonPropertyName("patternFa")]
    public string PatternFa { get; set; } = string.Empty;
    
    [JsonPropertyName("tokens")]
    public List<Token> Tokens { get; set; } = new();
}

public class Token
{
    [JsonPropertyName("token")]
    public string TokenText { get; set; } = string.Empty;
    
    [JsonPropertyName("normalized")]
    public string Normalized { get; set; } = string.Empty;
    
    [JsonPropertyName("partOfSpeech")]
    public string PartOfSpeech { get; set; } = "other"; // noun, verb, adjective, etc.
    
    [JsonPropertyName("meaningFa")]
    public string MeaningFa { get; set; } = string.Empty;
    
    [JsonPropertyName("roleExplanationFa")]
    public string RoleExplanationFa { get; set; } = string.Empty;
}
